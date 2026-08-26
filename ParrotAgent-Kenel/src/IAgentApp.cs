using System;
using System.Threading.Tasks;

namespace ParrotAgent.Kenel
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
    public class AgentAppAttribute : Attribute
    {

    }
}
