using ParrotAgent.Kenel;
using System.Text.Json;

namespace ParrotAgent.Tool
{
    public class GetOSTool : ITool
    {
        public string Name => "get_os";

        public string Description =>
            "获取Agent所在的操作系统名称";

        //public override ToolCategory Category => ToolCategory.Write;

        public IReadOnlyList<ToolParameter> Parameters { get; } = [];

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
