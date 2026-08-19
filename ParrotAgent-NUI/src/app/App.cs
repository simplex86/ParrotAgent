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
        private StateMachine machine = new StateMachine();
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// 
        /// </summary>
        public App() 
        {
            cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            var entry = new AgentEntry(cancellationTokenSource.Token);
            var sink = await entry.Run();

            machine.Add("pending", new PendingStateNode(machine, sink.Input));
            machine.Add("thinking", new ThinkingStateNode(machine, sink.Output));
            machine.Add("closing", new ClosingStateNode());

            await machine.Run("pending");
        }
    }
}
