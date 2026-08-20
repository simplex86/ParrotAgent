using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    internal class CloseStateNode : StateNode
    {
        /// <summary>
        /// 
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="cancellationTokenSource"></param>
        public CloseStateNode(StateMachine machine, CancellationTokenSource cancellationTokenSource)
            : base(machine)
        {
            this.cancellationTokenSource = cancellationTokenSource;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override async Task Enter()
        {
            cancellationTokenSource.Cancel();
            await Task.CompletedTask;
        }
    }
}
