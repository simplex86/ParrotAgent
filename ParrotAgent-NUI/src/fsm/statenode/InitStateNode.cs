using ParrotAgent.Kenel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    internal class InitStateNode : StateNode
    {
        /// <summary>
        /// 
        /// </summary>
        private Sink sink;

        /// <summary>
        /// 
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="cancellationTokenSource"></param>
        public InitStateNode(StateMachine machine, Sink sink, CancellationTokenSource cancellationTokenSource) 
            : base(machine)
        {
            this.sink = sink;
            this.cancellationTokenSource = cancellationTokenSource;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override async Task Enter()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Initializing...");
            Console.ResetColor();

            sink.Output.OnChanged.Register(OnOutputChangedHandler);

            var entry = new AgentEntry(sink, cancellationTokenSource.Token);
            entry.Run();

            await Machine.Run("pending");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override async Task Exit()
        {
            sink.Output.OnChanged.Unregister(OnOutputChangedHandler);
            await Task.CompletedTask;
        }

        private void OnOutputChangedHandler(string content)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(content);
            Console.ResetColor();
        }
    }
}
