using System;
using System.Threading.Tasks;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 
    /// </summary>
    public class AgentEntry
    {
        public async Task RunAsync()
        {
            Console.WriteLine("Hello, Agent!");
            await Task.CompletedTask;
        }
    }
}
