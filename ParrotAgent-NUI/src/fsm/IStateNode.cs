using System.Threading.Tasks;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 状态节点
    /// </summary>
    internal interface IStateNode
    {
        /// <summary>
        /// 
        /// </summary>
        StateMachine Machine { get; }

        /// <summary>
        /// 进入
        /// </summary>
        /// <returns></returns>
        Task Enter();

        /// <summary>
        /// 离开
        /// </summary>
        /// <returns></returns>
        Task Exit();
    }

    /// <summary>
    /// 
    /// </summary>
    internal class StateNode : IStateNode
    {
        /// <summary>
        /// 
        /// </summary>
        public StateMachine Machine { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="machine"></param>
        public StateNode(StateMachine machine) 
        {
            Machine = machine;
        }

        /// <summary>
        /// 进入
        /// </summary>
        /// <returns></returns>
        public virtual async Task Enter()
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 离开
        /// </summary>
        /// <returns></returns>
        public virtual async Task Exit()
        {
            await Task.CompletedTask;
        }
    }
}
