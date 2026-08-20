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
        private SinkChannel outputChannel;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="outputChannel"></param>
        public ThinkingStateNode(StateMachine machine, SinkChannel outputChannel)
            : base(machine)
        {
            this.outputChannel = outputChannel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override async Task Enter()
        {
            outputChannel.OnChanged.Register(OnOutputChangedHandler);
            outputChannel.OnCompleted.Register(OnOutputCompletedHandler);

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
            outputChannel.OnChanged.Unregister(OnOutputChangedHandler);
            outputChannel.OnCompleted.Unregister(OnOutputCompletedHandler);

            await Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private void OnOutputChangedHandler(string text)
        {
            Console.Write(text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private void OnOutputCompletedHandler(string text)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Completed!");
            Console.ResetColor();

            Machine.Run("pending").Wait();
        }
    }
}
