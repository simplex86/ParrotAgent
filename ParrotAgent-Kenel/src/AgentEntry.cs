using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        private EventSink eventSink;
        /// <summary>
        /// 
        /// </summary>
        private CancellationToken cancellationToken;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        public AgentEntry(EventSink eventSink, CancellationToken cancellationToken)
        {
            this.chatProvider = new MockProvider();
            this.eventSink = eventSink;
            this.cancellationToken = cancellationToken;
        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Run()
        {
            try
            {
                var config = AgentConfigLoader.Load();
                {
                    var provider = config.Providers.FirstOrDefault(p => p.Name == config.ActiveProvider);
                    Schema.Init(provider);

                    eventSink.Output.Boardcast(new AgentBeginEvent()
                    {
                        Provider = provider.Name,
                        Protocol = provider.Protocol
                    });
                }
                chatProvider = ProviderFactory.CreateActive(config);

                var agent = new Agent(chatProvider, eventSink, cancellationToken);
                agent.Run();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
