using ParrotAgent.Kenel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    internal class App
    {
        /// <summary>
        /// 
        /// </summary>
        public App() 
        {
            
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            var machine = new StateMachine();
            var sink = new Sink();
            var cancellationTokenSource = new CancellationTokenSource();

            machine.Add("init", new InitStateNode(machine, sink, cancellationTokenSource));
            machine.Add("pending", new PendingStateNode(machine, sink.Input));
            machine.Add("thinking", new ThinkingStateNode(machine, sink.Output));
            machine.Add("close", new CloseStateNode(machine, cancellationTokenSource));

            await machine.Run("init");
        }
    }
}
