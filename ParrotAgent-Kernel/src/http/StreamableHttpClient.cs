using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class StreamableHttpClient
    {
        private HttpClient http;

        /// <summary>
        /// 
        /// </summary>
        public AuthenticationHeaderValue Authorization
        {
            set
            {
                http.DefaultRequestHeaders.Authorization = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public StreamableHttpClient(string url, double timeout)
        {
            http = new HttpClient()
            {
                BaseAddress = new Uri(EnsureTrailingSlash(url)),
                Timeout = TimeSpan.FromMinutes(timeout)
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Post(string text, string uri, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await Request(text, uri, HttpMethod.Post, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<string> PostStream(string text, string uri, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = await Request(text, uri, HttpMethod.Post, cancellationToken);

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var parser = SseParser.Create(stream);

            await foreach (var sse in parser.EnumerateAsync(cancellationToken))
            {
                yield return sse.Data;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> Request(string text, string uri, HttpMethod method, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(method, uri)
            {
                Content = new StringContent(text, Encoding.UTF8, "application/json")
            };

            return await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }

        /// <summary>
        /// 确保 BaseUrl 以 / 结尾，使相对路径 chat/completions 能正确拼接到 base_url 之后。
        /// .NET URI 解析：无尾斜杠时 "https://host/v1" + "chat/completions" = "https://host/chat/completions"（丢失 /v1）。
        /// </summary>
        private static string EnsureTrailingSlash(string baseUrl) => baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/";
    }
}
