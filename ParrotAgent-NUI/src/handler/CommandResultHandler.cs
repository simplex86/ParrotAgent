using System;
using System.Threading.Tasks;
using ParrotAgent.Kernel;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    [EventHandler]
    public class CommandResultHandler : AEventHandler<CommandResultEvent>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        protected override async Task Process(CommandResultEvent evt)
        {
            Console.WriteLine(evt.Result.Output);
            await Task.CompletedTask;
        }
    }
}
