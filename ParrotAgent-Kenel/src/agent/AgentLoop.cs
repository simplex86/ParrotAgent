using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class AgentLoop
    {
        /// <summary>
        /// 
        /// </summary>
        private IProtocolProvider chatProvider;
        /// <summary>
        /// 
        /// </summary>
        private SinkChannel outputChannel;
        /// <summary>
        /// 
        /// </summary>
        private ToolRegistry toolRegistry;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chatProvider"></param>
        /// <param name="outputChannel"></param>
        /// <param name="cancellationToken"></param>
        public AgentLoop(IProtocolProvider chatProvider, ToolRegistry toolRegistry, SinkChannel outputChannel)
        {
            this.chatProvider = chatProvider;
            this.outputChannel = outputChannel;
            this.toolRegistry = toolRegistry;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public async Task Run(IReadOnlyList<IMessage> messages, bool stream, CancellationToken cancellationToken)
        {
            var tools = toolRegistry.Wire();

            while (!cancellationToken.IsCancellationRequested)
            {
                await Chat(messages, tools, stream, cancellationToken);
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        private async Task Chat(IReadOnlyList<IMessage> messages, JsonElement? tools, bool stream, CancellationToken cancellationToken)
        {
            try
            {
                if (stream)
                {
                    await foreach (var delta in chatProvider.ChatStream(messages, tools, cancellationToken))
                    {
                        outputChannel.Write(delta);
                    }
                }
                else
                {
                    var response = await chatProvider.Chat(messages, tools, cancellationToken);
                    outputChannel.Write(response);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                outputChannel.Complete();
            }

            
        }
    }
}
