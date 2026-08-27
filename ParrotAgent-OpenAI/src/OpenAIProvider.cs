using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using ParrotAgent.Kernel;

namespace ParrotAgent.Protocol
{
    using OpenAI;

    /// <summary>
    /// OpenAI 兼容协议 Provider。
    /// 通过 BaseUrl 覆盖 OpenAI 官方与 DeepSeek 等兼容服务。
    /// </summary>
    [ProtocolProvider("openai")]
    public class OpenAIProvider : IProtocolProvider
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

            var accumulator = new ToolCallAccumulator();

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

                        var function = toolcall.Function;
                        accumulator.Accumulate(idx, toolcall.Id, function?.Name, function?.Arguments);
                    }
                }

                // 3. 工具调用结束：返回完整的 tool calls
                if (choice.FinishReason == "tool_calls")
                {
                    var toolcalls = accumulator.Build();
                    yield return new Chunk.ToolCalls(toolcalls);
                }

                // 4. 普通内容结束
                if (choice.FinishReason == "stop")
                {
                    var usage = chunk.Usage;
                    if (usage is not null)
                    {
                        yield return new Chunk.Stop(usage.PromptTokens, usage.CompletionTokens, usage.TotalTokens);
                    }
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
