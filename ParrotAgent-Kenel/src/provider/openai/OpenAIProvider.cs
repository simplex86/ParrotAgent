using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kenel
{
    using Protocol.OpenAI;

    /// <summary>
    /// OpenAI 兼容协议 Provider。
    /// 通过 BaseUrl 覆盖 OpenAI 官方与 DeepSeek 等兼容服务。
    /// </summary>
    internal class OpenAIProvider : IProtocolProvider
    {
        /// <summary>
        /// 
        /// </summary>
        private ProviderConfig config;
        /// <summary>
        /// 
        /// </summary>
        private StreamableHttpClient http;

        /// <summary>
        /// 缓存同一个 tool call 的分片（key 是 ToolCallChunk.Index）
        /// </summary>
        private readonly Dictionary<int, ToolCallChunk> toolcalls = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public OpenAIProvider(ProviderConfig config)
        {
            this.config = config;
            this.http = new StreamableHttpClient(config.BaseUrl, 5)
            {
                Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey)
            };
        }

        /// <summary>
        /// 非流式聊天：给定用户输入，返回完整回复。
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="tools"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<string> Chat(IReadOnlyList<IMessage> messages,
                                       JsonElement? tools,
                                       CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var request = BuildRequestBody(messages, tools, false);
                using var response = await http.Send(request, "chat/completions", cancellationToken);

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(content);

                return doc.RootElement.GetProperty("choices")[0]
                                      .GetProperty("message")
                                      .GetProperty("content")
                                      .GetString() ?? string.Empty;
            }
            catch (Exception ex)
            {

            }

            return string.Empty;
        }

        /// <summary>
        /// 流式聊天
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="tools"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<Chunk> ChatStream(IReadOnlyList<IMessage> messages,
                                                         JsonElement? tools,
                                                         [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            toolcalls.Clear();

            var request = BuildRequestBody(messages, tools, true);
            using var response = await http.Send(request, "chat/completions", cancellationToken);

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var parser = SseParser.Create(stream);

            await foreach (var sse in parser.EnumerateAsync(cancellationToken))
            {
                var data = sse.Data;
                if (data == "[DONE]")
                {
                    yield return new Chunk.Done("done");
                    break;
                }

                ChatChunk? chunk;
                try
                {
                    chunk = JsonSerializer.Deserialize<ChatChunk>(data);
                }
                catch (JsonException)
                {
                    continue; // 跳过脏数据帧
                }

                if (chunk?.Choices is null || chunk.Choices.Count == 0) continue;
                var choice = chunk.Choices[0];
                var delta = choice.Delta;
                if (delta is null) continue;

                // 1. 处理普通文本内容
                if (!string.IsNullOrEmpty(delta.Content))
                {
                    yield return new Chunk.TextDelta(delta.Content);
                }

                // 2. 处理 tool_calls 分片
                if (delta.ToolCalls is not null)
                {
                    foreach (var toolcall in delta.ToolCalls)
                    {
                        if (!toolcall.Index.HasValue) continue;
                        var idx = toolcall.Index.Value;

                        // 缓存或更新 tool call 分片
                        if (toolcalls.TryGetValue(idx, out var existing))
                        {
                            // 拼接 arguments（核心！流式返回的 arguments 是分段的）
                            var existingArgs = existing.Function?.Arguments ?? "";
                            var newArgs = toolcall.Function?.Arguments ?? "";
                            var mergedArgs = existingArgs + newArgs;

                            toolcalls[idx] = existing with
                            {
                                Id = toolcall.Id ?? existing.Id,
                                Type = toolcall.Type ?? existing.Type,
                                Function = existing.Function with
                                {
                                    Name = toolcall.Function?.Name ?? existing.Function?.Name,
                                    Arguments = mergedArgs
                                }
                            };
                        }
                        else
                        {
                            toolcalls[idx] = toolcall;
                        }
                    }
                }

                // 3. 工具调用结束：返回完整的 tool calls
                if (choice.FinishReason == "tool_calls")
                {
                    var completedToolCalls = toolcalls.Values.Where(tc => tc.Id is not null && tc.Function?.Name is not null)
                                                             .Select(tc => new ToolCall(tc.Id!,
                                                                                        tc.Type ?? "function",
                                                                                        tc.Function!.Name!,
                                                                                        tc.Function.Arguments ?? "{}"))
                                                             .ToList();

                    if (completedToolCalls.Count > 0)
                    {
                        yield return new Chunk.ToolCallDelta(completedToolCalls);
                    }
                    toolcalls.Clear();
                }

                // 4. 普通内容结束
                if (choice.FinishReason == "stop")
                {
                    yield return new Chunk.Done("stop");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="tools"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        private string BuildRequestBody(IReadOnlyList<IMessage> messages,
                                        JsonElement? tools,
                                        bool stream)
        {
            var array = messages.Select(m => m.Wire());

            var root = new JsonObject
            {
                ["model"] = config.Model,
                ["messages"] = JsonNode.Parse(JsonSerializer.Serialize(array)),
                ["stream"] = stream
            };

            if (tools is { ValueKind: JsonValueKind.Array })
            {
                root["tools"] = JsonNode.Parse(tools.Value.GetRawText());
                root["tool_choice"] = "auto";
            }

            return root.ToJsonString();
        }
    }
}
