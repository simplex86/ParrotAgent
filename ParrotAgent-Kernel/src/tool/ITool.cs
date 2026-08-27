using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="Type">"string" / "number" / "integer" / "boolean" / "array" / "object"</param>
    /// <param name="Description"></param>
    /// <param name="Required"></param>
    public record ToolParameter(string Name, string Type, string Description, bool Required);

    /// <summary>
    /// 
    /// </summary>
    public enum ToolCategory
    {
        Read,
        Write
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ToolSafety
    {
        /// <summary>
        /// 安全
        /// </summary>
        Safe,
        /// <summary>
        /// 标准
        /// </summary>
        Normal,
        /// <summary>
        /// 危险
        /// </summary>
        Danger
    }

    /// <summary>
    /// 工具执行结果。无论成功失败都返回 ToolResult，不抛异常（异常由 ToolExecutor 捕获转译）。
    /// Success=true 时 Content 含结果文本，Error 为 null。
    /// Success=false 时 Error 含人类可读错误原因（会回灌给 LLM），Content 通常为空。
    /// </summary>
    public sealed record ToolResult(bool Success, string Content, string? Error = null)
    {
        /// <summary>
        /// 成功
        /// </summary>
        public static ToolResult Ok(string content) => new(true, content, null);

        /// <summary>
        /// 失败
        /// </summary>
        public static ToolResult Fail(string error) => new(false, string.Empty, error);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface ITool
    {
        /// <summary>
        /// 
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 
        /// </summary>
        IReadOnlyList<ToolParameter> Parameters { get; }

        /// <summary>
        /// 执行工具。
        /// </summary>
        /// <param name="input">LLM 生成的 JSON 参数</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ToolResult> Execute(JsonElement input, CancellationToken cancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ToolAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public ToolCategory Category { get; }
        public ToolSafety Safety { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        public ToolAttribute(ToolCategory category = ToolCategory.Read, ToolSafety safety = ToolSafety.Normal)
        {
            Category = category;
            Safety = safety;
        }
    }
}
