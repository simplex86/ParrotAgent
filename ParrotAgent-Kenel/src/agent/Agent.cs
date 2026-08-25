using System;
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
        /// <summary>
        /// 
        /// </summary>
        private AgentLoop agentLoop;
        /// <summary>
        /// 
        /// </summary>
        private Conversation conversation = new Conversation();
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
        /// <param name="chatProvider"></param>
        /// <param name="cancellationToken"></param>
        public Agent(IProtocolProvider chatProvider, ToolRegistry toolRegistry, EventSink eventSink, CancellationToken cancellationToken)
        {
            var executor = new ToolExecutor(toolRegistry);
            var hitl = new PromptHitl(eventSink);
            var batchExecutor = new BatchToolExecutor(executor, hitl);

            this.agentLoop = new AgentLoop(chatProvider, toolRegistry, batchExecutor, eventSink);
            this.eventSink = eventSink;
            this.cancellationToken = cancellationToken;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var taskCompletionSource = new TaskCompletionSource<string>();
                eventSink.Broadcast(new UserPromptEvent() { TaskCompletionSource = taskCompletionSource });
                var prompt = await taskCompletionSource.Task;
                try
                {
                    conversation.AddUser(prompt);
                    await agentLoop.Run(conversation, true, cancellationToken);
                }
                catch (Exception ex)
                {

                }
            }

            eventSink.Broadcast(new AgentEndEvent());
        }
    }
}
