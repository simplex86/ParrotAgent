using System.Threading.Tasks;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 状态节点
    /// </summary>
    internal interface IStateNode
    {
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
}
