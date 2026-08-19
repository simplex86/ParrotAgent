using System;
using System.Threading.Tasks;
using ParrotAgent.Kenel;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    internal class PendingStateNode : IStateNode
    {
        /// <summary>
        /// 
        /// </summary>
        private StateMachine machine;
        /// <summary>
        /// 
        /// </summary>
        private SinkChannel inputChannel;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="inputChannel"></param>
        public PendingStateNode(StateMachine machine, SinkChannel inputChannel) 
        { 
            this.machine = machine;
            this.inputChannel = inputChannel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task Enter()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("> ");
            var input = Console.ReadLine();
            Console.ResetColor();

            if (input == null ||
                input == "/exit" ||
                input == "/quit")
            {
                await machine.Run("closing");
                return;
            }

            await machine.Run("thinking");
            inputChannel.Write(input);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task Exit()
        {
            await Task.CompletedTask;
        }
    }
}
