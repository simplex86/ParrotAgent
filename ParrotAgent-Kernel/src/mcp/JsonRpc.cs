using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// JSON-RPC 2.0 编解码 + Future 匹配
    /// 每个请求带自增 id，响应按 id 匹配到 TaskCompletionSource<JsonElement>
    /// 线程安全：ConcurrentDictionary + Interlocked.Increment
    /// </summary>
    internal sealed class JsonRpc
    {
        private int nextId = 0;
        private readonly ConcurrentDictionary<int, TaskCompletionSource<JsonElement>> pending = new();
        private static readonly JsonSerializerOptions CamelCase = new() 
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// 创建带自增 id 的请求，返回 (JSON 字符串, 等待响应的 Task)
        /// 调用方发送 JSON 字符串后 await Task 等待响应
        /// </summary>
        public (string Json, Task<JsonElement> ResponseTask) CreateRequest(string method, object? @params = null)
        {
            var id = Interlocked.Increment(ref nextId);
            var tcs = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
            pending[id] = tcs;

            var request = new
            {
                jsonrpc = "2.0",
                id,
                method,
                @params
            };
            var json = JsonSerializer.Serialize(request, CamelCase);

            return (json, tcs.Task);
        }

        /// <summary>
        /// 创建通知（无 id，不期望响应）
        /// </summary>
        public string CreateNotification(string method, object? @params = null)
        {
            var notification = new
            {
                jsonrpc = "2.0",
                method,
                @params
            };
            return JsonSerializer.Serialize(notification, CamelCase);
        }

        /// <summary>
        /// 处理从 transport 收到的 JSON 消息，按类型分发
        /// - 有 id 且有 result/error → 响应，匹配到 pending TCS
        /// - 有 method 无 id → 通知（暂不处理，记日志）
        /// 
        /// 容错：npx 等包装器可能往 stdout 打印非 JSON 内容（安装进度等），直接忽略非 JSON 行，避免崩溃接收循环导致后续请求无人读取（超时）
        /// </summary>
        public void HandleMessage(string json)
        {
            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(json);
            }
            catch (JsonException)
            {
                Trace.TraceWarning($"收到非 JSON 行，忽略（可能来自 npx 等包装器）：{(json.Length > 200 ? json[..200] + "..." : json)}");
                return;
            }

            using (doc)
            {
                var root = doc.RootElement;

                // 有 id 且有 result/error → 响应
                if (root.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.Number)
                {
                    var id = idEl.GetInt32();
                    if (pending.TryRemove(id, out var tcs))
                    {
                        if (root.TryGetProperty("error", out var error))
                        {
                            var message = error.TryGetProperty("message", out var msgEl) ? msgEl.GetString() ?? "Unknown error"
                                                                                         : "Unknown error";
                            var code = error.TryGetProperty("code", out var codeEl) ? codeEl.GetInt32() : -1;
                            tcs.SetException(new JsonRpcException(code, message));
                        }
                        else if (root.TryGetProperty("result", out var result))
                        {
                            tcs.SetResult(result.Clone());
                        }
                        else
                        {
                            tcs.SetException(new JsonRpcException(-1, "响应缺少 result 和 error 字段"));
                        }
                    }
                    else
                    {
                        Trace.TraceWarning($"收到未知 id={id} 的 JSON-RPC 响应");
                    }
                }
                // 有 method 无 id → 通知
                else if (root.TryGetProperty("method", out _))
                {
                    // 本迭代不处理 Server → Client 通知（如 tools/list_changed）
                    Trace.TraceInformation($"收到 JSON-RPC 通知，暂不处理：{json}");
                }
            }
        }

        /// <summary>
        /// 取消所有等待中的请求（transport 关闭时调用）
        /// </summary>
        public void CancelAllPending()
        {
            foreach (var kvp in pending)
            {
                kvp.Value.TrySetException(new JsonRpcException(-1, "连接已关闭"));
            }
            pending.Clear();
        }
    }

    /// <summary>
    /// JSON-RPC 错误异常
    /// </summary>
    public sealed class JsonRpcException : Exception
    {
        public int Code { get; }
        public JsonRpcException(int code, string message) : base($"JSON-RPC 错误 [{code}]: {message}") => Code = code;
    }
}
