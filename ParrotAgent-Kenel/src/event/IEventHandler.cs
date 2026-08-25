using System;
using System.Threading.Tasks;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEventHandler
    {
        /// <summary>
        /// 
        /// </summary>
        Type EventType { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        Task Run(IEvent evt);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AEventHandler<T> : IEventHandler where T : struct, IEvent
    {
        /// <summary>
        /// 
        /// </summary>
        Type IEventHandler.EventType => typeof(T);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        async Task IEventHandler.Run(IEvent evt)
        {
            await Process((T)evt);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        protected abstract Task Process(T evt);
    }

    /// <summary>
    /// 
    /// </summary>
    public class EventHandlerAttribute : Attribute
    {

    }
}
