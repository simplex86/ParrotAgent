using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    internal class McpToolAdapter : ITool
    {
        private readonly McpClient _client;
        private readonly McpToolInfo _toolInfo;
        private IReadOnlyList<ToolParameter>? _parameters;

        /// <summary>
        /// 全局工具名（含 server 前缀）：{serverName}/{toolName}。
        /// </summary>
        public string Name => $"{_client.ServerName}/{_toolInfo.Name}";

        public string Description => _toolInfo.Description;

        /// <summary>
        /// 默认 Normal，让安全层和 HITL 覆盖
        /// </summary>
        public ToolSafety Safety => ToolSafety.Normal;

        /// <summary>
        /// 工具分类：annotations.readOnlyHint=true → Read；否则 Write
        /// </summary>
        public ToolCategory Category => _toolInfo.Annotations?.ReadOnlyHint == true ? ToolCategory.Read : ToolCategory.Write;

        /// <summary>
        /// 参数列表：从 MCP InputSchema（JSON Schema）解析
        /// 仅提取顶层 properties 的 name/type/description/required，不做嵌套 object 递归（MCP 工具参数通常是扁平结构）
        /// </summary>
        public IReadOnlyList<ToolParameter> Parameters => _parameters ??= ParseParameters();

        internal McpToolAdapter(McpClient client, McpToolInfo toolInfo)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _toolInfo = toolInfo ?? throw new ArgumentNullException(nameof(toolInfo));
        }

        /// <summary>
        /// 执行 MCP 工具调用
        /// </summary>
        public async Task<ToolResult> Execute(JsonElement input, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _client.CallTool(_toolInfo.Name, input, cancellationToken);

                if (result.IsError)
                {
                    var errorText = string.Join("\n", result.Content.Where(c => c.Type == "text").Select(c => c.Text));
                    return ToolResult.Fail(string.IsNullOrWhiteSpace(errorText) ? "MCP 工具调用失败" : errorText);
                }

                var contentText = string.Join("\n", result.Content.Where(c => c.Type == "text").Select(c => c.Text));
                return ToolResult.Ok(contentText);
            }
            catch (JsonRpcException ex)
            {
                Trace.TraceWarning($"MCP 工具 {Name} 调用失败: {ex}");
                return ToolResult.Fail($"MCP 工具 {Name} 调用失败：{ex.Message}");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;  // 外部取消透传
            }
            catch (Exception ex)
            {
                Trace.TraceWarning($"MCP 工具 {Name} 执行异常: {ex}");
                return ToolResult.Fail($"MCP 工具 {Name} 执行失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 从 MCP InputSchema 解析参数列表（仅顶层 properties）
        /// </summary>
        private IReadOnlyList<ToolParameter> ParseParameters()
        {
            if (_toolInfo.InputSchema.ValueKind != JsonValueKind.Object) return Array.Empty<ToolParameter>();
            if (!_toolInfo.InputSchema.TryGetProperty("properties", out var props)) return Array.Empty<ToolParameter>();
            if (props.ValueKind != JsonValueKind.Object) return Array.Empty<ToolParameter>();

            var required = new HashSet<string>();
            if (_toolInfo.InputSchema.TryGetProperty("required", out var reqEl) && reqEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var r in reqEl.EnumerateArray())
                    if (r.ValueKind == JsonValueKind.String) required.Add(r.GetString()!);
            }

            var parameters = new List<ToolParameter>();
            foreach (var prop in props.EnumerateObject())
            {
                var type = prop.Value.TryGetProperty("type", out var t) ? t.GetString() ?? "string" : "string";
                var desc = prop.Value.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "";
                parameters.Add(new ToolParameter(prop.Name, type, desc, required.Contains(prop.Name)));
            }
            return parameters;
        }
    }
}
