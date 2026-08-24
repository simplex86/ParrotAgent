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
            eventSink.Output.Register<ToolCallEvent>(OnToolCallHandler);
            eventSink.Output.Register<ToolResultEvent>(OnToolResultHandler);

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
            eventSink.Output.Unregister<ToolCallEvent>(OnToolCallHandler);
            eventSink.Output.Unregister<ToolResultEvent>(OnToolResultHandler);

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

            Machine.Run<PendingStateNode>().Wait();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        private void OnToolCallHandler(IEvent e)
        {
            var evt = (ToolCallEvent)e;

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"Invoke Function = {evt.Call.Name}, Arguments = {evt.Call.Args}");
            Console.ResetColor();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        private void OnToolResultHandler(IEvent e)
        {
            var evt = (ToolResultEvent)e;

            if (evt.Result.Success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Invoke Function = {evt.Call.Name}, Arguments = {evt.Call.Args}, Result = {evt.Result.Content}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Invoke Function = {evt.Call.Name}, Arguments = {evt.Call.Args}, Result = {evt.Result.Error}");
            }
            Console.ResetColor();
        }
    }
}
