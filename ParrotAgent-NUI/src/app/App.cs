using System;
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

            machine.Add("pending", new PendingState(machine, sink.Input));
            machine.Add("thinking", new ThinkingState(machine, sink.Output));

            await machine.Run("pending");
        }
    }
}
