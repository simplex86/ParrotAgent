using System;
using System.Threading.Tasks;
using ParrotAgent.Kenel;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    [EventHandler]
    public class ContextCompressingHandler : AEventHandler<ContextCompressingEvent>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        protected override async Task Process(ContextCompressingEvent evt)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Compressing...");
            Console.ResetColor();

            await Task.CompletedTask;
        }
    }
}
