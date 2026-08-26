using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ParrotAgent.Tool
{
    /// <summary>
    /// glob 模式转正则的工具类
    /// 支持 * / ** / ?：
    /// - ** 匹配任意层级目录（含路径分隔符）
    /// - * 匹配除路径分隔符外的任意字符
    /// - ? 匹配除路径分隔符外的单个字符
    /// </summary>
    internal static class GlobPattern
    {
        /// <summary>
        /// 把 glob 模式转成正则。返回已编译的 Regex，锚定 ^...$。
        /// </summary>
        public static Regex ToRegex(string pattern)
        {
            var sb = new StringBuilder("^");
            var i = 0;
            while (i < pattern.Length)
            {
                if (pattern[i] == '*' && i + 1 < pattern.Length && pattern[i + 1] == '*')
                {
                    sb.Append(".*");
                    i += 2;
                    if (i < pattern.Length && pattern[i] == '/') i++; // 吃掉 **/
                }
                else if (pattern[i] == '*') { sb.Append("[^/]*"); i++; }
                else if (pattern[i] == '?') { sb.Append("[^/]"); i++; }
                else if ("+()|^$.{}\\".IndexOf(pattern[i]) >= 0) { sb.Append('\\').Append(pattern[i]); i++; }
                else { sb.Append(pattern[i]); i++; }
            }
            sb.Append('$');

            return new Regex(sb.ToString(), RegexOptions.Compiled);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class Extension
    {
        /// <summary>
        /// 提取必需的 string 参数。缺失或类型错误时返回 string.Empty 并设置 error。
        /// 用 out 模式而非 (Value, Error) 元组——让编译器能正确推断返回值为非空。
        /// 调用方应立即判断 error 是否非空，非空则 return ToolResult.Fail(err)。
        /// </summary>
        public static string GetRequiredString(this JsonElement input, string name, out string? error)
        {
            error = null;

            if (!input.TryGetProperty(name, out var el))
            {
                error = $"缺少必需参数：{name}";
                return string.Empty;
            }
            if (el.ValueKind != JsonValueKind.String)
            {
                error = $"参数 {name} 类型错误：期望 string，实际 {el.ValueKind}";
                return string.Empty;
            }
            
            return el.GetString() ?? string.Empty;
        }

        /// <summary>
        /// 提取可选的 string 参数。缺失返回 defaultValue，类型错误返回 string.Empty 并设置 error。
        /// </summary>
        public static string GetOptionalString(this JsonElement input, string name, out string? error, string defaultValue = "")
        {
            error = null;

            if (!input.TryGetProperty(name, out var el))
            {
                error = null;
                return defaultValue;
            }
            if (el.ValueKind != JsonValueKind.String)
            {
                error = $"参数 {name} 类型错误：期望 string，实际 {el.ValueKind}";
                return string.Empty;
            }
            
            return el.GetString() ?? string.Empty;
        }

        /// <summary>
        /// 提取可选的 int 参数。缺失返回 defaultValue，类型错误返回 0 并设置 error。
        /// </summary>
        public static int GetOptionalInt(this JsonElement input, string name, out string? error, int defaultValue = 0)
        {
            if (!input.TryGetProperty(name, out var el))
            {
                error = null;
                return defaultValue;
            }
            if (el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out var v))
            {
                error = null;
                return v;
            }

            error = $"参数 {name} 类型错误：期望 integer，实际 {el.ValueKind}";
            return 0;
        }
    }
}
