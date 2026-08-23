using System;
using System.Threading.Tasks;
using ParrotAgent.Kenel;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    internal class ThinkingStateNode : StateNode
    {
        /// <summary>
        /// 
        /// </summary>
        private EventSink eventSink;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="eventSink"></param>
        public ThinkingStateNode(StateMachine machine, EventSink eventSink)
            : base(machine)
        {
            this.eventSink = eventSink;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override async Task Enter()
        {
            eventSink.Output.Register<AssistantDeltaEvent>(OnAssistantDeltaHandler);
            eventSink.Output.Register<AssistantCompletedEvent>(OnAssistantCompletedHandler);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Thinking...");
            Console.ForegroundColor = ConsoleColor.Gray;

            await Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override async Task Exit()
        {
            eventSink.Output.Unregister<AssistantDeltaEvent>(OnAssistantDeltaHandler);
            eventSink.Output.Unregister<AssistantCompletedEvent>(OnAssistantCompletedHandler);

            await Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private void OnAssistantDeltaHandler(IEvent e)
        {
            var evt = (AssistantDeltaEvent)e;
            Console.Write(evt.Delta);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private void OnAssistantCompletedHandler(IEvent e)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Completed!");
            Console.ResetColor();

            Machine.Run("pending").Wait();
        }
    }
}
