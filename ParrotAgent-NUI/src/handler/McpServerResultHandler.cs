using System;
using System.Threading.Tasks;
using ParrotAgent.Kernel;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    [EventHandler]
    public class McpServerResultHandler : AEventHandler<McpServerResultEvent>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        protected override async Task Process(McpServerResultEvent evt)
        {
            if (evt.Success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Connect to [{evt.Name}] MCP server successfully! Fetch {evt.ToolCount} Tools");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Connect to [{evt.Name}] MCP server failed");
            }
            Console.ResetColor();

            await Task.CompletedTask;
        }
    }
}
