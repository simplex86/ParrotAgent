using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using ParrotAgent.Kernel;

namespace ParrotAgent.Tool
{
    /// <summary>
    /// 
    /// </summary>
    [Tool(ToolCategory.Read, ToolSafety.Safe)]
    public sealed class GetOSTool : ITool
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name => "get_os";

        /// <summary>
        /// 
        /// </summary>
        public string Description => "获取Agent所在的操作系统名称";

        //public override ToolCategory Category => ToolCategory.Write;

        /// <summary>
        /// 无参数
        /// </summary>
        public IReadOnlyList<ToolParameter> Parameters { get; } = [];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ToolResult> Execute(JsonElement input, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (OperatingSystem.IsWindows()) return ToolResult.Ok("windows");
            if (OperatingSystem.IsMacOS())   return ToolResult.Ok("macos");
            if (OperatingSystem.IsLinux())   return ToolResult.Ok("linux");
            if (OperatingSystem.IsIOS())     return ToolResult.Ok("ios");
            if (OperatingSystem.IsAndroid()) return ToolResult.Ok("android");

            return ToolResult.Fail("unknown os");
        } 
    }
}
