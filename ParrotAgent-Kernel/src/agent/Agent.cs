using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kernel
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
        public Agent(IProtocolProvider chatProvider, ToolRegistry toolRegistry, EventSink eventSink, Compressor compressor, CancellationToken cancellationToken)
        {
            var tooExecutor = new ToolExecutor(toolRegistry);
            var hitl = new PromptHitl(eventSink);
            var batchExecutor = new BatchToolExecutor(toolRegistry, tooExecutor, hitl);

            this.agentLoop = new AgentLoop(chatProvider, toolRegistry, batchExecutor, eventSink, compressor);
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
                try
                {
                    var taskCompletionSource = new TaskCompletionSource<string>();
                    eventSink.Broadcast(new UserPromptEvent() { TaskCompletionSource = taskCompletionSource });
                    var prompt = await taskCompletionSource.Task;

                    conversation.AddUser(prompt);
                    await agentLoop.Run(conversation, true, cancellationToken);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                }
            }
        }
    }
}
