using ParrotAgent.Kernel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Tool
{
    /// <summary>
    /// 读文件工具：读取指定路径的文件内容，返回完整文本
    /// </summary>
    [Tool(ToolCategory.Read, ToolSafety.Safe)]
    public sealed class ReadFileTool : ITool
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name => "read_file";

        /// <summary>
        /// 
        /// </summary>
        public string Description =>
            "读取指定路径的文件内容，返回完整文本。路径可以是相对或绝对路径。" +
            "不支持读取目录；文件不存在或无权限会返回错误。";

        /// <summary>
        /// 
        /// </summary>
        public IReadOnlyList<ToolParameter> Parameters { get; } = [
            new ToolParameter("path", "string", "要读取的文件路径（相对或绝对）", Required: true)
        ];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ToolResult> Execute(JsonElement input, CancellationToken cancellationToken)
        {
            var path = input.GetRequiredString("path", out var err);
            if (err is not null) return ToolResult.Fail(err);
            if (string.IsNullOrWhiteSpace(path)) return ToolResult.Fail("参数 path 不能为空");

            // 目录检测：File.ReadAllTextAsync 对目录抛 UnauthorizedAccessException，错误信息不友好
            if (Directory.Exists(path))
                return ToolResult.Fail($"路径是目录而非文件：{path}");

            try
            {
                var content = await File.ReadAllTextAsync(path, cancellationToken);
                return ToolResult.Ok(content);
            }
            catch (FileNotFoundException)
            {
                return ToolResult.Fail($"文件不存在：{path}");
            }
            catch (DirectoryNotFoundException)
            {
                return ToolResult.Fail($"路径不存在：{path}");
            }
            catch (IOException ex)
            {
                return ToolResult.Fail($"读取文件失败：{ex.Message}");
            }
            // UnauthorizedAccessException 等其他异常由 ToolExecutor 兜底捕获
        }
    }
}
