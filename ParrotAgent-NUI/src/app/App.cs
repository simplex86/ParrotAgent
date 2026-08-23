using System;
using System.Threading;
using System.Threading.Tasks;
using ParrotAgent.Kenel;

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
            var eventSink = new EventSink();
            var cancellationTokenSource = new CancellationTokenSource();

            machine.Add("init", new InitStateNode(machine, eventSink, cancellationTokenSource));
            machine.Add("pending", new PendingStateNode(machine, eventSink));
            machine.Add("thinking", new ThinkingStateNode(machine, eventSink));
            machine.Add("close", new CloseStateNode(machine, cancellationTokenSource));

            await machine.Run("init");
        }
    }
}
