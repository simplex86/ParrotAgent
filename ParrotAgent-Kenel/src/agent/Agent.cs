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
        public Agent(IProtocolProvider chatProvider, Sink sink, CancellationToken cancellationToken)
        {
            sink.Input.OnChanged.Register(OnSinkInputChangedHandler);
            sink.Output.OnChanged.Register(OnSinkOutputChangedHandler);
            sink.Output.OnCompleted.Register(OnSinkOutputCompletedHandler);

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
            this.agentLoop = new AgentLoop(chatProvider, toolRegistry, sink.Output);
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
        /// <param name="content"></param>
        /// <returns></returns>
        private void OnSinkInputChangedHandler(string content)
        {
            try
            {
                conversation.AddUser(content);
                agentLoop.Run(conversation.ToProviderMessages(), true, cancellationToken).Wait();
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private void OnSinkOutputChangedHandler(string content)
        {
            reply.Append(content);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private void OnSinkOutputCompletedHandler(string _)
        {
            var content = reply.ToString();
            reply.Clear();

            conversation.AddAssistant(content);
        }
    }
}
