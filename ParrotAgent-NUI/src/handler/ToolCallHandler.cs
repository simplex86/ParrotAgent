using System;
using System.Threading.Tasks;
using ParrotAgent.Kenel;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    [EventHandler]
    public class ToolCallHandler : AEventHandler<ToolCallEvent>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        protected override async Task Process(ToolCallEvent evt)
        {
            await Task.CompletedTask;
        }
    }
}
