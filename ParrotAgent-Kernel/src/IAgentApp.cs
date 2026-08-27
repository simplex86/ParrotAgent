using System;
using System.Threading.Tasks;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAgentApp
    {
        Task Run();
    }

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AgentAppAttribute : Attribute
    {

    }
}
