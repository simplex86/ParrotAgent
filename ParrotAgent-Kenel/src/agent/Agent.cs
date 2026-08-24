using System;
using System.Text;
using System.Threading;

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
        private CancellationToken cancellationToken;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chatProvider"></param>
        /// <param name="cancellationToken"></param>
        public Agent(IProtocolProvider chatProvider, ToolRegistry toolRegistry, EventSink eventSink, CancellationToken cancellationToken)
        {
            eventSink.Input.Register<UserPromptEvent>(OnUserPromptHandler);

            var toolExecutor = new ToolExecutor(toolRegistry);
            var batchExecutor = new BatchToolExecutor(toolExecutor, toolRegistry);

            this.agentLoop = new AgentLoop(chatProvider, toolRegistry, batchExecutor, eventSink);
            this.cancellationToken = cancellationToken;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void Run()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        private void OnUserPromptHandler(IEvent e)
        {
            var evt = (UserPromptEvent)e;
            try
            {
                conversation.AddUser(evt.Prompt);
                agentLoop.Run(conversation, true, cancellationToken).Wait();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
