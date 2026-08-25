using System;
using System.Threading.Tasks;
using ParrotAgent.Kenel;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    [EventHandler]
    public class AssistantDeltaHandler : AEventHandler<AssistantDeltaEvent>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        protected override async Task Process(AssistantDeltaEvent evt)
        {
            Console.Write(evt.Delta);
            await Task.CompletedTask;
        }
    }
}
