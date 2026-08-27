using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.ServerSentEvents;
using System.Text;
using System.Text.Json;
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
        /// <param name="requestUri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Send(string text, string requestUri, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await RequestAsync(text, requestUri, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="requestUri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> RequestAsync(string text, string requestUri, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
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
