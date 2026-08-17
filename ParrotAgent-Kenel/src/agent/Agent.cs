using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 
    /// </summary>
    internal class Agent
    {
        private IChatProvider chatProvider;
        private Sink sink;
        private CancellationToken cancellationToken;
        private AgentLoop agentLoop = null;
        private List<string> history = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chatProvider"></param>
        /// <param name="cancellationToken"></param>
        public Agent(IChatProvider chatProvider, Sink sink, CancellationToken cancellationToken)
        {
            this.chatProvider = chatProvider;
            this.sink = sink;
            this.cancellationToken = cancellationToken;
            this.agentLoop = new AgentLoop(chatProvider, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task RunAsync()
        {
            sink.Input.Add(OnSinkInputHandler);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private async Task OnSinkInputHandler(string input)
        {
            history.Add(input);
            await agentLoop.RunAsync(history);
        }
    }
}
