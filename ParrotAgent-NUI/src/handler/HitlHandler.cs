using System;
using System.Threading.Tasks;
using ParrotAgent.Kernel;

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
            Console.WriteLine($"  Name: {evt.Call.Name}");
            Console.WriteLine($"  Args: {evt.Call.Args}");
            Console.WriteLine("Do you allow to invoke?");
            Console.WriteLine("  A. Allow");
            Console.WriteLine("  D. Deny");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Enter your choice: ");
            Console.ResetColor();

            var input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
            {
                evt.TaskCompletionSource.SetResult(HitlOption.Deny);
                AppContext.CancellationTokenSource.Cancel();
                return;
            }

            input = input.Trim().ToUpper();
            if (input == "A")
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
