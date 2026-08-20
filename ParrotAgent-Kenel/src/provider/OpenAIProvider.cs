using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ParrotAgent.Kenel
{
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
        public async IAsyncEnumerable<string> ChatStream(IReadOnlyList<IMessage> messages, 
                                                         JsonElement? tools, 
                                                         [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = BuildRequestBody(messages, tools, true);
            using var response = await http.Send(request, "chat/completions", cancellationToken);

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var parser = SseParser.Create(stream);

            await foreach (var sse in parser.EnumerateAsync(cancellationToken))
            {
                var data = sse.Data;

                // 1. 处理流结束标记
                if (data == "[DONE]")
                    break;

                // 2. 反序列化（ framing 由 SseParser 完成，这里只管 JSON）
                ChatChunk? chunk;
                try
                {
                    chunk = JsonSerializer.Deserialize<ChatChunk>(data);
                }
                catch (JsonException)
                {
                    continue;// 心跳/脏数据帧：跳过，不中断整个流
                }

                if (chunk?.Choices is null || chunk.Choices.Length == 0) 
                    continue;

                var content = chunk.Choices[0].Delta?.Content;
                if (!string.IsNullOrEmpty(content )) 
                    yield return content ;
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
