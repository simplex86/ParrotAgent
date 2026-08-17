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
    internal sealed class AgentLoop
    {
        private IChatProvider chatProvider;
        private CancellationToken cancellationToken;

        public AgentLoop(IChatProvider chatProvider, CancellationToken cancellationToken)
        {
            this.chatProvider = chatProvider;
            this.cancellationToken = cancellationToken;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task RunAsync(List<string> messages)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var response = await chatProvider.ChatAsync(messages[^1], cancellationToken);
                    Console.WriteLine(response);
                }
                catch (Exception ex)
                {

                }
                finally
                {

                }

                await Task.CompletedTask;
                return;
            }
        }
    }
}
