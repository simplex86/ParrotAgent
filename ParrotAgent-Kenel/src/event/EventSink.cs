using System;
using System.Collections.Generic;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 
    /// </summary>
    internal class EventSink
    {
        /// <summary>
        /// 
        /// </summary>
        private Dictionary<Type, List<IEventHandler>> listener = new Dictionary<Type, List<IEventHandler>>();

        /// <summary>
        /// 
        /// </summary>
        public void Collect()
        {
            if (listener.Count > 0)
                return;

            var types = Reflection.FindAll<IEventHandler, EventHandlerAttribute>();
            foreach (var type in types)
            {
                var handler = Reflection.CreateInstance<IEventHandler>(type);
                if (!listener.TryGetValue(handler.EventType, out var list))
                {
                    list = new List<IEventHandler>();
                    listener.Add(handler.EventType, list);
                }
                list.Add(handler);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="evt"></param>
        public void Broadcast<T>(T evt) where T : struct, IEvent
        {
            if (listener.TryGetValue(typeof(T), out var handlers))
            {
                foreach (var handler in handlers)
                {
                    handler.Run(evt);
                }
            }
        }
    }
}
