using System;
using System.Collections.Generic;
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
        private SinkChannel outputChannel;
        private CancellationToken cancellationToken;

        public AgentLoop(IChatProvider chatProvider, SinkChannel outputChannel, CancellationToken cancellationToken)
        {
            this.chatProvider = chatProvider;
            this.outputChannel = outputChannel;
            this.cancellationToken = cancellationToken;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task Run(List<IMessage> messages)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    //var response = await chatProvider.Chat(messages, cancellationToken);
                    //outputChannel.Write(response);
                    await foreach (var token in chatProvider.ChatStream(messages, cancellationToken))
                    {
                        outputChannel.Write(token);
                    }
                    outputChannel.Complete();
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
