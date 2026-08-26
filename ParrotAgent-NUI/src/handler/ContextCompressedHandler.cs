using System;
using System.Threading.Tasks;
using ParrotAgent.Kenel;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    [EventHandler]
    public class ContextCompressedHandler : AEventHandler<ContextCompressedEvent>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        protected override async Task Process(ContextCompressedEvent evt)
        {
            if (evt.WasCompressed)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Compression successful!");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Compression failed!");
                Console.ResetColor();
            }

            await Task.CompletedTask;
        }
    }
}
