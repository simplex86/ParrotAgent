using System;
using System.Threading.Tasks;
using ParrotAgent.Kenel;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    [EventHandler]
    public class AssistantCompletedHandler : AEventHandler<AssistantCompletedEvent>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        protected override async Task Process(AssistantCompletedEvent evt)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"Completed! Prompt Tokens = {evt.PromptTokens}, Total Tokens = {evt.TotalTokens}");
            Console.ResetColor();
        }
    }
}
