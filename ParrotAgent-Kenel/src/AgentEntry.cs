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
        private Sink sink;
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
            this.sink = new Sink();
            this.cancellationToken = cancellationToken;
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <returns>返回sink对象</returns>
        public async Task<Sink> RunAsync()
        {
            var agent = new Agent(chatProvider, sink, cancellationToken);
            await agent.RunAsync();

            return sink;
        }
    }
}
