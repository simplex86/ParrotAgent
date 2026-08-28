using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// MCP 客户端：管理单个 MCP server 的完整生命周期
    /// 流程：connect → initialize → initialized → tools/list → tools/call → close
    /// 接收：后台 Task 持续从 transport 读取消息，分发给 JsonRpc.HandleMessage。
    /// 超时：initialize 30s，tools/list 10s，tools/call 60s。
    /// </summary>
    internal sealed class McpClient : IAsyncDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly string serverName;
        /// <summary>
        /// 
        /// </summary>
        private readonly ITransport transport;
        /// <summary>
        /// 
        /// </summary>
        private readonly JsonRpc rpc;

        /// <summary>
        /// 接收循环
        /// </summary>
        private Task? receiveLoop;
        /// <summary>
        /// 
        /// </summary>
        private CancellationTokenSource? receiveCancellationTokenSource;

        /// <summary>
        /// 已发现的工具列表
        /// </summary>
        private IReadOnlyList<McpToolInfo> tools = Array.Empty<McpToolInfo>();

        /// <summary>
        /// 已发现的工具列表
        /// </summary>
        public IReadOnlyList<McpToolInfo> Tools => tools;

        /// <summary>
        /// Server 名称
        /// </summary>
        public string ServerName => serverName;

        /// <summary>
        /// 是否已连接并初始化
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="transport"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public McpClient(string serverName, ITransport transport)
        {
            this.serverName = serverName ?? throw new ArgumentNullException(nameof(serverName));
            this.transport = transport ?? throw new ArgumentNullException(nameof(transport));
            rpc = new JsonRpc();
        }

        /// <summary>
        /// 连接 MCP server 并完成初始化握手
        /// 流程：transport.Connect → 启动接收循环 → initialize → initialized → tools/list
        /// </summary>
        public async Task Connect(CancellationToken cancellationToken)
        {
            // 1. 启动传输
            await transport.Connect(cancellationToken);

            // 2. 启动接收循环
            receiveCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            receiveLoop = ReceiveLoop(receiveCancellationTokenSource.Token);

            // 3. initialize 握手
            var initParams = new McpInitializeParams();
            var (json, task) = rpc.CreateRequest(McpMethods.Initialize, initParams);
            await transport.Send(json, cancellationToken);

            var initResult = await task.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);
            Trace.TraceInformation($"MCP Server [{serverName}] initialize 成功：{initResult}");

            // 4. 发送 initialized 通知
            var notification = rpc.CreateNotification(McpMethods.Initialized);
            await transport.Send(notification, cancellationToken);

            // 5. 获取工具列表
            await RefreshTools(cancellationToken);

            IsInitialized = true;
            Trace.TraceInformation($"MCP Server [{serverName}] 就绪，提供 {tools.Count} 个工具");
        }

        /// <summary>
        /// 调用 MCP 工具
        /// </summary>
        public async Task<McpToolCallResult> CallTool(string toolName, JsonElement arguments, CancellationToken cancellationToken)
        {
            var @params = new McpToolCallParams { Name = toolName, Arguments = arguments };
            var (json, task) = rpc.CreateRequest(McpMethods.ToolsCall, @params);
            await transport.Send(json, cancellationToken);

            var result = await task.WaitAsync(TimeSpan.FromSeconds(60), cancellationToken);
            return ParseToolCallResult(result);
        }

        /// <summary>
        /// 关闭 MCP 客户端
        /// </summary>
        public async Task Close(CancellationToken cancellationToken)
        {
            Trace.TraceInformation($"MCP Client [{serverName}] 正在关闭");

            // 停止接收循环
            receiveCancellationTokenSource?.Cancel();

            // 关闭传输
            try
            {
                await transport.Close(cancellationToken);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning($"MCP Transport [{serverName}] 关闭异常: {ex}");
            }

            // 等待接收循环结束
            if (receiveLoop is not null)
            {
                try { await receiveLoop; } catch { }
            }

            // 取消所有 pending 请求
            rpc.CancelAllPending();
            IsInitialized = false;
        }

        /// <summary>
        /// 接收循环：从 transport 读取消息，分发给 JsonRpc 处理
        /// </summary>
        private async Task ReceiveLoop(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var message = await transport.Receive(ct);
                    if (message is null) break;  // 连接关闭
                    if (string.IsNullOrWhiteSpace(message)) continue;
                    rpc.HandleMessage(message);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Trace.TraceError($"MCP Client [{serverName}] 接收循环异常: {ex}");
            }
            finally
            {
                rpc.CancelAllPending();
            }
        }

        /// <summary>
        /// 刷新工具列表（调用 tools/list）
        /// </summary>
        public async Task RefreshTools(CancellationToken cancellationToken)
        {
            var (json, task) = rpc.CreateRequest(McpMethods.ToolsList);
            await transport.Send(json, cancellationToken);

            var result = await task.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken);
            tools = ParseToolsList(result);
        }

        /// <summary>
        /// 解析 tools/list 响应
        /// </summary>
        private static IReadOnlyList<McpToolInfo> ParseToolsList(JsonElement result)
        {
            if (result.ValueKind != JsonValueKind.Object) return Array.Empty<McpToolInfo>();
            if (!result.TryGetProperty("tools", out var toolsEl)) return Array.Empty<McpToolInfo>();
            if (toolsEl.ValueKind != JsonValueKind.Array) return Array.Empty<McpToolInfo>();

            var tools = new List<McpToolInfo>();
            foreach (var toolEl in toolsEl.EnumerateArray())
            {
                var name = toolEl.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                var desc = toolEl.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "";
                var schema = toolEl.TryGetProperty("inputSchema", out var s) ? s.Clone() : default;

                McpToolAnnotations? annotations = null;
                if (toolEl.TryGetProperty("annotations", out var a))
                {
                    annotations = new McpToolAnnotations
                    {
                        ReadOnlyHint = a.TryGetProperty("readOnlyHint", out var ro) && ro.ValueKind == JsonValueKind.True ? true : (ro.ValueKind == JsonValueKind.False ? false : null),
                        DestructiveHint = a.TryGetProperty("destructiveHint", out var dh) && dh.ValueKind == JsonValueKind.True ? true : (dh.ValueKind == JsonValueKind.False ? false : null)
                    };
                }

                tools.Add(new McpToolInfo
                {
                    Name = name,
                    Description = desc,
                    InputSchema = schema,
                    Annotations = annotations
                });
            }

            return tools;
        }

        /// <summary>
        /// 解析 tools/call 响应
        /// </summary>
        private static McpToolCallResult ParseToolCallResult(JsonElement result)
        {
            var content = new List<McpContentBlock>();
            var isError = false;

            if (result.ValueKind == JsonValueKind.Object)
            {
                if (result.TryGetProperty("isError", out var ie) && ie.ValueKind == JsonValueKind.True)
                    isError = true;

                if (result.TryGetProperty("content", out var contentEl) && contentEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var block in contentEl.EnumerateArray())
                    {
                        var type = block.TryGetProperty("type", out var t) ? t.GetString() ?? "text" : "text";
                        var text = block.TryGetProperty("text", out var tx) ? tx.GetString() ?? "" : "";
                        content.Add(new McpContentBlock { Type = type, Text = text });
                    }
                }
            }

            return new McpToolCallResult { Content = content, IsError = isError };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {

        }
    }
}
