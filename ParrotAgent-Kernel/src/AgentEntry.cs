using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// Agent启动入口
    /// </summary>
    public class AgentEntry
    {
        /// <summary>
        /// 
        /// </summary>
        private CancellationToken cancellationToken;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        public AgentEntry(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
        }

        /// <summary>
        /// 启动
        /// </summary>
        public async Task Run()
        {
            McpConnectionManager? mcpManager = null;

            try
            {
                var toolRegistry = new ToolRegistry();
                toolRegistry.Collect();

                var commandRegistry = new CommandRegistry();
                commandRegistry.Collect();

                var eventDispatcher = new EventDispatcher();
                eventDispatcher.Collect();

                var config = AgentConfigLoader.Load();
                {
                    var providerConfig = config.Providers.FirstOrDefault(p => p.Name == config.ActiveProvider);
                    await eventDispatcher.Dispatch(new AgentBeginEvent() {
                        Provider = providerConfig.Name,
                        Protocol = providerConfig.Protocol,
                        ContextWindowTokens = config.Context.ContextWindowTokens,
                    });

                    Schema.Init(providerConfig);
                }

                var provider = Provider.CreateActive(config);
                var contextConfig = config.Context ?? new ContextConfig();
                var mcpConfig = config.Mcp ?? new McpConfig();
                var truncateConfig = new TruncateConfig
                {
                    PerResultThreshold = contextConfig.PerResultThreshold ?? 50_000,
                    RoundTotalThreshold = contextConfig.RoundTotalThreshold ?? 200_000,
                    PreviewLength = contextConfig.PreviewLength ?? 2_000,
                };

                var compressor = new Compressor(provider, 
                                                contextConfig.ContextWindowTokens, 
                                                truncateConfig, 
                                                contextConfig.WarningFraction ?? 0.7, 
                                                contextConfig.TriggerFraction ?? 0.9, 
                                                contextConfig.KeepRecentMessages ?? 4, 
                                                contextConfig.MaxCircuitFailures ?? 2, 
                                                contextConfig.EnableAutoCompress ?? true);

                if (mcpConfig.Enable ?? true)
                {
                    mcpManager = new McpConnectionManager([.. mcpConfig.Servers ?? Array.Empty<McpServerConfig>()], eventDispatcher);
                    await mcpManager.Connect(cancellationToken);

                    // 将MCP工具注册到ToolRegistry中
                    foreach (var adapter in mcpManager.Adapters)
                    {
                        try
                        {
                            toolRegistry.Register(adapter);
                        }
                        catch (ArgumentException ex)
                        {
                            Trace.TraceWarning($"MCP 工具 [{adapter.Name}] 注册失败（名称冲突）：{ex}");
                        }
                    }
                }

                var agent = new Agent(provider, 
                                      toolRegistry, 
                                      commandRegistry, 
                                      eventDispatcher, 
                                      compressor,  
                                      cancellationToken);
                await agent.Run();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
            finally
            {
                if (mcpManager is not null)
                {
                    await mcpManager.Close(cancellationToken);
                    await mcpManager.DisposeAsync();
                }
            }
        }
    }
}
