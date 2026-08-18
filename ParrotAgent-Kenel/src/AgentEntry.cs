using System;
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
        private IChatProvider chatProvider;
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
            this.chatProvider = new MockProvider();
            this.cancellationToken = cancellationToken;
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <returns>返回sink对象</returns>
        public async Task<Sink> Run()
        {
            var sink = new Sink();

            try
            {
                var config = AgentConfigLoader.Load();
                chatProvider = ProviderFactory.CreateActive(config);

                var agent = new Agent(chatProvider, sink, cancellationToken);
                await agent.Run();
            }
            catch (Exception ex)
            {

            }

            return sink;
        }
    }
}
