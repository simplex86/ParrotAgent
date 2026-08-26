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
            try
            {
                var toolRegistry = new ToolRegistry();
                toolRegistry.Collect();

                var eventSink = new EventSink();
                eventSink.Collect();

                var config = AgentConfigLoader.Load();
                {
                    var providerConfig = config.Providers.FirstOrDefault(p => p.Name == config.ActiveProvider);
                    eventSink.Broadcast(new AgentBeginEvent()
                    {
                        Provider = providerConfig.Name,
                        Protocol = providerConfig.Protocol,
                        ContextWindowTokens = config.Context.ContextWindowTokens,
                        ToolCount = toolRegistry.Count,
                    });

                    Schema.Init(providerConfig);
                }

                var provider = Provider.CreateActive(config);
                var contextConfig = config.Context ?? new ContextConfig();

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

                var agent = new Agent(provider, toolRegistry, eventSink, compressor, cancellationToken);
                await agent.Run();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
