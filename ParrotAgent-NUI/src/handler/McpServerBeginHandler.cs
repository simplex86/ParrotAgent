using System;
using System.Threading.Tasks;
using ParrotAgent.Kernel;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    [EventHandler]
    public class McpServerBeginHandler : AEventHandler<McpServerBeginEvent>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        protected override async Task Process(McpServerBeginEvent evt)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"Begin to connect {evt.TotalCount} MCP Servers ...");
            Console.ResetColor();

            await Task.CompletedTask;
        }
    }
}
