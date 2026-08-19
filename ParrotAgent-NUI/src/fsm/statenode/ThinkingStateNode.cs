using System;
using System.Threading.Tasks;
using ParrotAgent.Kenel;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    internal class ThinkingStateNode : IStateNode
    {
        /// <summary>
        /// 
        /// </summary>
        private StateMachine machine;
        /// <summary>
        /// 
        /// </summary>
        private SinkChannel outputChannel;
        /// <summary>
        /// 
        /// </summary>
        private bool thinking = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="outputChannel"></param>
        public ThinkingStateNode(StateMachine machine, SinkChannel outputChannel)
        {
            this.machine = machine;
            this.outputChannel = outputChannel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task Enter()
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
        public async Task Exit()
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

            machine.Run("pending").Wait();
        }
    }
}
