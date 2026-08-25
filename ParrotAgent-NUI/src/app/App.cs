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
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Initializing...");
            Console.ResetColor();

            var toolRegistry = new ToolRegistry();
            toolRegistry.Collect();

            var cancellationTokenSource = new CancellationTokenSource();

            var entry = new AgentEntry(toolRegistry, cancellationTokenSource.Token);
            await entry.Run();
        }
    }
}
