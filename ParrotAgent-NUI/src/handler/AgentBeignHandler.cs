using System;
using System.Threading.Tasks;
using ParrotAgent.Kenel;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    [EventHandler]
    public class AgentBeignHandler : AEventHandler<AgentBeginEvent>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        protected override async Task Process(AgentBeginEvent evt)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"Provider = {evt.Provider}, Protocol = {evt.Protocol}, Tools = {evt.ToolCount}");
            Console.ResetColor();

            await Task.CompletedTask;
        }
    }
}
