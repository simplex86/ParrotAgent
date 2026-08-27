using System;
using System.Text;
using System.Threading.Tasks;
using ParrotAgent.Kernel;

namespace ParrotAgent.NUI
{

    /// <summary>
    /// 
    /// </summary>
    [AgentApp]
    public class App : IAgentApp
    {
        /// <summary>
        /// 启动
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Initializing...");
            Console.ResetColor();

            Console.OutputEncoding = Encoding.UTF8;
            {
                var entry = new AgentEntry(AppData.CancellationTokenSource.Token);
                await entry.Run();
            }
            Console.ResetColor();
        }
    }
}
