using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// Streamable HTTP 传输
    /// </summary>
    internal class StreamableHttpTransport : ITransport
    {
        private readonly McpServerConfig config;
        private readonly HttpClient httpClient;
        private readonly string protocolVersion;
        private CancellationTokenSource? receiveCancellationTokenSource;
        private Channel<string>? receiveChannel;

        private string? sessionId;
        private bool isFirstRequestSent;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public StreamableHttpTransport(McpServerConfig config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.protocolVersion = McpMethods.ProtocolVersion;

            if (string.IsNullOrWhiteSpace(this.config.Url))
                throw new ArgumentException("HTTP transport 需要 url", nameof(config));

            httpClient = new HttpClient(new HttpClientHandler())
            {
                BaseAddress = new Uri(this.config.Url)
            };

            if (!string.IsNullOrWhiteSpace(this.config.ApiKey))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.config.ApiKey);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Connect(CancellationToken cancellationToken)
        {
            receiveCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            receiveChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions() {
                SingleReader = true,
                SingleWriter = true
            });

            // HTTP 传输无需预连接——首个 POST 即建立
            Trace.TraceInformation($"MCP Streamable HTTP Server [{config.Name}] 已就绪 ({config.Url})");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task Send(string json, CancellationToken cancellationToken)
        {
            if (receiveChannel is null) throw new InvalidOperationException("Transport 未连接");

            var request = new HttpRequestMessage(HttpMethod.Post, "/mcp")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            // Accept: 声明可接受 JSON 和 SSE 两种响应格式
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

            // Mcp-Session-Id: initialize 后的请求必须回传 server 分配的 session ID
            if (sessionId is not null)
                request.Headers.Add("Mcp-Session-Id", sessionId);

            // MCP-Protocol-Version: initialize 后的请求应携带协议版本号（spec 2025-03-26 要求）
            if (isFirstRequestSent)
                request.Headers.Add("MCP-Protocol-Version", protocolVersion);

            var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            // 捕获 Mcp-Session-Id（server 在 initialize 响应中返回，后续请求需回传）
            if (response.Headers.TryGetValues("Mcp-Session-Id", out var sessionIds))
                sessionId = sessionIds.FirstOrDefault();

            // 标记首个请求已发送（后续请求携带 MCP-Protocol-Version）
            isFirstRequestSent = true;

            var contentType = response.Content.Headers.ContentType?.MediaType;

            if (contentType == "text/event-stream")
            {
                // SSE 流响应：在后台解析 SSE 事件并写入 channel
                _ = Task.Run(() => ParseStream(response, receiveChannel.Writer, receiveCancellationTokenSource!.Token), receiveCancellationTokenSource!.Token);
            }
            else
            {
                // 单次 JSON 响应
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!string.IsNullOrWhiteSpace(body)) await receiveChannel.Writer.WriteAsync(body, cancellationToken);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<string?> Receive(CancellationToken cancellationToken)
        {
            if (receiveChannel is null) throw new InvalidOperationException("Transport 未连接");
            if (await receiveChannel.Reader.WaitToReadAsync(cancellationToken))
            {
                if (receiveChannel.Reader.TryRead(out var msg)) return msg;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Close(CancellationToken cancellationToken)
        {
            receiveCancellationTokenSource?.Cancel();
            receiveChannel?.Writer.TryComplete();
            Trace.TraceInformation($"MCP Streamable HTTP Server [{config.Name}] 已关闭");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 解析 SSE 流，提取 data 行写入 channel
        /// </summary>
        private async Task ParseStream(HttpResponseMessage response, ChannelWriter<string> writer, CancellationToken cancellationToken)
        {
            try
            {
                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var reader = new StreamReader(stream);
                string? currentData = null;

                while (!cancellationToken.IsCancellationRequested && !reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync(cancellationToken);
                    if (line is null) break;

                    if (line.StartsWith("data: "))
                    {
                        currentData = line["data: ".Length..];
                    }
                    else if (line.Length == 0 && currentData is not null)
                    {
                        // 空行 = 事件分隔符，发送累积的 data
                        await writer.WriteAsync(currentData, cancellationToken);
                        currentData = null;
                    }
                }

                // 流结束时如果有未发送的 data
                if (currentData is not null)
                    await writer.WriteAsync(currentData, cancellationToken);
            }
            catch (OperationCanceledException)
            { 
            
            }
            catch (Exception ex)
            {
                Trace.TraceInformation($"MCP Streamable HTTP SSE 流结束：{ex.Message}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ValueTask DisposeAsync()
        {
            receiveCancellationTokenSource?.Cancel();
            receiveCancellationTokenSource?.Dispose();
            httpClient.Dispose();

            return ValueTask.CompletedTask;
        }
    }
}
