using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// OpenAI 兼容协议 Provider。
    /// 通过 BaseUrl 覆盖 OpenAI 官方与 DeepSeek 等兼容服务。
    /// </summary>
    internal class OpenAIProvider : IChatProvider
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
        public async Task<string> Chat(IReadOnlyList<IMessage> messages, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var request = BuildRequestBody(messages, false);
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
        /// 
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<string> ChatStream(IReadOnlyList<IMessage> messages, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = BuildRequestBody(messages, true);
            using var response = await http.Send(request, "chat/completions", cancellationToken);

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (line is null) break;  // 流结束

                // SSE 协议：空行是事件分隔符，跳过
                if (string.IsNullOrEmpty(line)) continue;
                // 其他前缀（event: / id: / 注释 :...）本迭代不处理，跳过
                if (!line.StartsWith("data: ")) continue;

                var data = line["data: ".Length..];
                if (data == "[DONE]") break;  // OpenAI 流终止标记

                // 解析 JSON，提取 choices[0].delta.content
                using var doc = JsonDocument.Parse(data);
                var choices = doc.RootElement.GetProperty("choices");
                if (choices.GetArrayLength() == 0) continue;

                var delta = choices[0].GetProperty("delta");
                if (delta.TryGetProperty("content", out var content))
                {
                    var text = content.GetString();
                    if (!string.IsNullOrEmpty(text)) yield return text;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        private string BuildRequestBody(IReadOnlyList<IMessage> messages, bool stream)
        {
            var array = messages.Select(m => new
            {
                role = m.Role switch
                {
                    MessageRole.System => "system",
                    MessageRole.User => "user",
                    MessageRole.Assistant => "assistant",
                    MessageRole.Tool => "tool",
                    _ => "user"
                },
                content = m.Content
            });

            var request = new
            {
                model = config.Model,
                messages = array,
                stream
            };

            return JsonSerializer.Serialize(request);
        }
    }
}
