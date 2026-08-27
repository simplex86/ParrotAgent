using System;
using System.Threading.Tasks;
using ParrotAgent.Kernel;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    [EventHandler]
    public class UserPromptHandler : AEventHandler<UserPromptEvent>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        protected override async Task Process(UserPromptEvent evt)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("> ");
            var input = Console.ReadLine();
            Console.ResetColor();

            evt.TaskCompletionSource.SetResult(input);

            await Task.CompletedTask;
        }
    }
}
