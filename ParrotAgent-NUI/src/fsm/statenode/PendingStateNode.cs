using System;
using System.Threading.Tasks;
using ParrotAgent.Kenel;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    internal class PendingStateNode : StateNode
    {
        /// <summary>
        /// 
        /// </summary>
        private EventSink eventSink;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="inputChannel"></param>
        public PendingStateNode(StateMachine machine, EventSink eventSink) 
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
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("> ");
            var input = Console.ReadLine();
            Console.ResetColor();

            if (input == null    ||
                input == "/exit" ||
                input == "/quit")
            {
                await Machine.Run<CloseStateNode>();
                return;
            }

            await Machine.Run<ThinkingStateNode>();
            eventSink.Input.Broadcast(new UserPromptEvent()
            {
                Prompt = input
            });
        }
    }
}
