using System;
using System.Threading.Tasks;
using ParrotAgent.Kernel;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    [EventHandler]
    public class ToolResultHandler : AEventHandler<ToolResultEvent>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        protected override async Task Process(ToolResultEvent evt)
        {
            if (evt.Result.Success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Invoke Function = {evt.Call.Name}, Arguments = {evt.Call.Args}, Result = {(evt.Result.Content.Length < 30 ? evt.Result.Content : evt.Result.Content[..30] + "...")}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Invoke Function = {evt.Call.Name}, Arguments = {evt.Call.Args}, Error = {evt.Result.Error}");
            }
            Console.ResetColor();

            await Task.CompletedTask;
        }
    }
}
