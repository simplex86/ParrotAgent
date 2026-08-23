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
        private CancellationToken cancellationToken;
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
        private StringBuilder reply = new StringBuilder();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chatProvider"></param>
        /// <param name="cancellationToken"></param>
        public Agent(IProtocolProvider chatProvider, EventSink eventSink, CancellationToken cancellationToken)
        {
            eventSink.Input.Register<UserPromptEvent>(OnUserPromptHandler);
            eventSink.Output.Register<AssistantDeltaEvent>(OnAssistantDeltaHandler);
            eventSink.Output.Register<AssistantCompletedEvent>(OnAssistantCompletedHandler);

            var toolRegistry = new ToolRegistry();
            {
                var types = Reflection.FindAll<ITool, ToolAttribute>();
                foreach (var type in types)
                {
                    var tool = Reflection.CreateInstance<ITool>(type);
                    toolRegistry.Register(tool);
                }
            }

            this.cancellationToken = cancellationToken;
            this.agentLoop = new AgentLoop(chatProvider, toolRegistry, eventSink);
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
                agentLoop.Run(conversation.ToProviderMessages(), true, cancellationToken).Wait();
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        private void OnAssistantDeltaHandler(IEvent e)
        {
            var evt = (AssistantDeltaEvent)e;
            reply.Append(evt.Delta);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        private void OnAssistantCompletedHandler(IEvent e)
        {
            var evt = (AssistantCompletedEvent)e;

            var content = reply.ToString();
            reply.Clear();

            conversation.AddAssistant(content);
        }
    }
}
