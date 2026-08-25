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
        private IProtocolProvider chatProvider;
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
            this.chatProvider = new MockProvider();
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
                        ToolCount = toolRegistry.Count,
                    });

                    Schema.Init(provider);
                }
                chatProvider = Provider.CreateActive(config);

                var agent = new Agent(chatProvider, toolRegistry, eventSink, cancellationToken);
                await agent.Run();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
