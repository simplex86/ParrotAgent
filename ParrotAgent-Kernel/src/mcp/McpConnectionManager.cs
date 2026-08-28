using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// MCP 连接管理器：并行连接所有配置的 MCP server，管理生命周期
    /// 单个 server 连接失败不阻塞其他——记日志，跳过该 server
    ///
    /// 职责：
    /// 1. 启动时并行连接所有 server（Task.WhenAll）
    /// 2. 将成功连接的 server 的工具收集到 Adapters 列表
    /// 3. 关闭时并行关闭所有 server
    /// </summary>
    internal class McpConnectionManager : IAsyncDisposable
    {
        private EventDispatcher eventDispatcher;
        private readonly IReadOnlyList<McpServerConfig> configs;
        private readonly List<McpClient> clients = new();
        private readonly List<McpToolAdapter> adapters = new();

        /// <summary>
        /// 已连接的 MCP 客户端列表
        /// </summary>
        public IReadOnlyList<McpClient> Clients => clients;

        /// <summary>
        /// 已收集的 MCP 工具适配器列表（注册到 ToolRegistry）
        /// </summary>
        public IReadOnlyList<McpToolAdapter> Adapters => adapters;

        /// <summary>
        /// 已连接的 server 数量
        /// </summary>
        public int ConnectedCount => clients.Count;

        /// <summary>
        /// 已发现的 MCP 工具数量
        /// </summary>
        public int ToolCount => adapters.Count;

        /// <summary>
        /// 配置的 server 总数
        /// </summary>
        public int ConfiguredCount => configs.Count;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configs"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public McpConnectionManager(IReadOnlyList<McpServerConfig> configs, EventDispatcher eventDispatcher)
        {
            this.configs = configs ?? throw new ArgumentNullException(nameof(configs));
            this.eventDispatcher = eventDispatcher;
        }

        /// <summary>
        /// 并行连接所有 MCP server
        /// 连接成功后工具适配器收集到 Adapters 列表，由调用方注册到 ToolRegistry
        /// </summary>
        public async Task Connect(CancellationToken cancellationToken)
        {
            if (configs.Count == 0) return;

            Trace.TraceInformation($"开始连接 {configs.Count} 个 MCP server...");
            await eventDispatcher.Dispatch(new McpServerBeginEvent() { TotalCount = configs.Count });

            var connectTasks = configs.Select(config => Connect(config, cancellationToken));
            await Task.WhenAll(connectTasks);

            Trace.TraceInformation($"MCP 连接完成：{clients.Count}/{configs.Count} 个 server 就绪，{adapters.Count} 个工具已发现");
            await eventDispatcher.Dispatch(new McpServerEndEvent() { TotalCount = configs.Count, ConnectedCount = clients.Count });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task Connect(McpServerConfig config, CancellationToken cancellationToken)
        {
            try
            {
                // 创建 transport
                ITransport transport = config.Transport switch
                {
                    "stdio" => new StdioTransport(config),
                    "http" or "sse" => new StreamableHttpTransport(config),
                    _ => throw new ArgumentException($"不支持的 MCP transport：{config.Transport}")
                };

                // 创建 client 并连接
                var client = new McpClient(config.Name, transport);
                await client.Connect(cancellationToken);

                // 收集工具适配器（不在此处注册到 registry）
                foreach (var toolInfo in client.Tools)
                {
                    var adapter = new McpToolAdapter(client, toolInfo);
                    adapters.Add(adapter);
                }

                clients.Add(client);

                await eventDispatcher.Dispatch(new McpServerResultEvent() { Name = config.Name, Success = true, ToolCount = client.Tools.Count });
            }
            catch (Exception ex)
            {
                Trace.TraceError($"MCP Server [{config.Name}] 连接失败，跳过: {ex}");
                await eventDispatcher.Dispatch(new McpServerResultEvent() { Name = config.Name, Success = false });
            }
        }

        /// <summary>
        /// 并行关闭所有 MCP server
        /// </summary>
        public async Task Close(CancellationToken cancellationToken)
        {
            if (clients.Count == 0) return;

            Trace.TraceInformation($"正在关闭 {clients.Count} 个 MCP server...");

            var closeTasks = clients.Select(client => Close(client, cancellationToken));
            await Task.WhenAll(closeTasks);

            clients.Clear();
            adapters.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task Close(McpClient client, CancellationToken cancellationToken)
        {
            try
            {
                await client.Close(cancellationToken);
            }
            catch
            {
                // 关闭失败不影响其他 server
            }
        }

        /// <summary>
        /// 获取连接状态
        /// </summary>
        public string GetStatus()
        {
            if (configs.Count == 0) return "未配置";
            if (clients.Count == 0) return $"已配置 {configs.Count} 个，全部连接失败";
            if (clients.Count < configs.Count) return $"{clients.Count}/{configs.Count} 个已连接，{adapters.Count} 个工具";
            return $"{clients.Count} 个已连接，{adapters.Count} 个工具";
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
