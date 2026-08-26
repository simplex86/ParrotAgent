using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// Agent启动入口
    /// </summary>
    public class AgentEntry
    {
        /// <summary>
        /// 
        /// </summary>
        private ToolRegistry toolRegistry;
        /// <summary>
        /// 
        /// </summary>
        private EventSink eventSink;
        /// <summary>
        /// 
        /// </summary>
        private CancellationToken cancellationToken;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        public AgentEntry(ToolRegistry toolRegistry, CancellationToken cancellationToken)
        {
            this.toolRegistry = toolRegistry;
            this.cancellationToken = cancellationToken;
        }

        /// <summary>
        /// 启动
        /// </summary>
        public async Task Run()
        {
            try
            {
                eventSink = new EventSink();
                eventSink.Collect();

                var config = AgentConfigLoader.Load();
                {
                    var provider = config.Providers.FirstOrDefault(p => p.Name == config.ActiveProvider);
                    eventSink.Broadcast(new AgentBeginEvent()
                    {
                        Provider = provider.Name,
                        Protocol = provider.Protocol,
                        ContextWindowTokens = config.Context.ContextWindowTokens,
                        ToolCount = toolRegistry.Count,
                    });

                    Schema.Init(provider);
                }

                var chatProvider = Provider.CreateActive(config);

                var contextConfig = config.Context ?? new ContextConfig();

                var truncateConfig = new TruncateConfig
                {
                    PerResultThreshold = contextConfig.PerResultThreshold ?? 50_000,
                    RoundTotalThreshold = contextConfig.RoundTotalThreshold ?? 200_000,
                    PreviewLength = contextConfig.PreviewLength ?? 2_000,
                };

                var compressor = new Compressor(chatProvider, 
                                                contextConfig.ContextWindowTokens, 
                                                truncateConfig, 
                                                contextConfig.WarningFraction ?? 0.7, 
                                                contextConfig.TriggerFraction ?? 0.9, 
                                                contextConfig.KeepRecentMessages ?? 4, 
                                                contextConfig.MaxCircuitFailures ?? 2, 
                                                contextConfig.EnableAutoCompress ?? true);

                var agent = new Agent(chatProvider, toolRegistry, eventSink, compressor, cancellationToken);
                await agent.Run();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
