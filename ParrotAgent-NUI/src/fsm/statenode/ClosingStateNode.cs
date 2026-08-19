using System.Threading.Tasks;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    internal class ClosingStateNode : IStateNode
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task Enter()
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task Exit()
        {
            await Task.CompletedTask;
        }
    }
}
