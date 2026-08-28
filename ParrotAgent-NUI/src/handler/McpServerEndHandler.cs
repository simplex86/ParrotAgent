using System;
using System.Threading.Tasks;
using ParrotAgent.Kernel;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    [EventHandler]
    public class McpServerEndHandler : AEventHandler<McpServerEndEvent>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        protected override async Task Process(McpServerEndEvent evt)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"Connected {evt.ConnectedCount}/{evt.TotalCount} MCP Servers ...");
            Console.ResetColor();

            await Task.CompletedTask;
        }
    }
}
