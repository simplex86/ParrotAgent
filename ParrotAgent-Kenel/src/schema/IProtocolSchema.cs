using System;
using System.Text.Json;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 
    /// </summary>
    internal interface IProtocolSchema
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
}
