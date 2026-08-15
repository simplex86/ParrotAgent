using System.Threading.Tasks;
using ParrotAgent.Kenel;

namespace ParrotAgent.TUI
{
    /// <summary>
    /// 
    /// </summary>
    internal class App
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task RunAsync()
        {
            var entry = new AgentEntry();
            await entry.RunAsync();
        }
    }
}
