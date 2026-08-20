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
        private Sink sink;
        /// <summary>
        /// 
        /// </summary>
        private CancellationToken cancellationToken;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        public AgentEntry(Sink sink, CancellationToken cancellationToken)
        {
            this.chatProvider = new MockProvider();
            this.sink = sink;
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
                    sink.Output.Write($"Provider: {provider.Name}, Protocol: {provider.Protocol}");
                }
                chatProvider = ProviderFactory.CreateActive(config);

                var agent = new Agent(chatProvider, sink, cancellationToken);
                agent.Run();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
