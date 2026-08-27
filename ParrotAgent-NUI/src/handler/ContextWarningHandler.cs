using System;
using System.Threading.Tasks;
using ParrotAgent.Kernel;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    [EventHandler]
    public class ContextWarningHandler : AEventHandler<ContextWarningEvent>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        protected override async Task Process(ContextWarningEvent evt)
        {
            await Task.CompletedTask;
        }
    }
}
