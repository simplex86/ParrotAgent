using System;
using System.Threading.Tasks;
using ParrotAgent.Kenel;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    [EventHandler]
    public class HitlHandler : AEventHandler<HitlEvent>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        protected override async Task Process(HitlEvent evt)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("The tool is about to be invoked.");
            Console.WriteLine($"  Name: {evt.ToolCall.Name}");
            Console.WriteLine($"  Args: {evt.ToolCall.Args}");
            Console.WriteLine("Do you allow to invoke?");
            Console.WriteLine("1. Allow");
            Console.WriteLine("2. Deny");
            Console.ResetColor();

            var option = Console.ReadLine();

            if (option == "1")
            {
                evt.TaskCompletionSource.SetResult(HitlOption.Allow);
            }
            else
            {
                evt.TaskCompletionSource.SetResult(HitlOption.Deny);
            }

            await Task.CompletedTask;
        }
    }
}
