using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
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
        private ProviderConfig config;
        private StreamableHttpClient http;

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
                var response = await http.SendAsync(request, "chat/completions", cancellationToken);

                using var doc = JsonDocument.Parse(response);

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
