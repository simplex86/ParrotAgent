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
            sink.Input.OnChanged.Register(OnSinkInputChangedHandler);

            sink.Output.OnChanged.Unregister(OnSinkOutputChangedHandler);
            sink.Output.OnCompleted.Unregister(OnSinkOutputCompletedHandler);

            await Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private void OnSinkInputChangedHandler(string input)
        {
            AddUser(input);
            agentLoop.Run(history).Wait();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private void OnSinkOutputChangedHandler(string output)
        {
            reply.Append(output);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private void OnSinkOutputCompletedHandler(string output)
        {
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
