using System;
using System.Collections.Generic;
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
        private Sink sink;
        private AgentLoop agentLoop = null;
        private List<IMessage> history = new List<IMessage>();
        private StringBuilder reply = new StringBuilder();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chatProvider"></param>
        /// <param name="cancellationToken"></param>
        public Agent(IChatProvider chatProvider, Sink sink, CancellationToken cancellationToken)
        {
            this.sink = sink;
            this.agentLoop = new AgentLoop(chatProvider, sink.Output, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            sink.Input.Add(OnSinkInputHandler);
            sink.Output.Add(OnSinkOutputHandler);

            await Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private async Task OnSinkInputHandler(string input)
        {
            AddUser(input);
            await agentLoop.Run(history);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private async Task OnSinkOutputHandler(string output)
        {
            reply.Append(output);
            AddAssistant(reply.ToString());

            reply.Clear();
        }

        /// <summary>
        /// 追加 user 消息。
        /// </summary>
        public void AddUser(string content)
        {
            ArgumentNullException.ThrowIfNull(content);
            history.Add(new UserMessage(content));
        }

        /// <summary>
        /// 追加 assistant 消息（AI 的完整回复）。
        /// </summary>
        public void AddAssistant(string content)
        {
            ArgumentNullException.ThrowIfNull(content);
            history.Add(new AssistantMessage(content));
        }
    }
}
