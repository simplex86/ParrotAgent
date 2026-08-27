using System;
using System.Text.Json;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProtocolSchema
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        object Wire(IMessage message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        JsonElement Wire(ITool tool);
    }

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ProtocalSchemaAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public ProtocalSchemaAttribute(string name)
        {
            Name = name;
        }
    }
}
