using System;
using System.Collections.Generic;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEventChannel
    {
        void Register<T>(Action<IEvent> action) where T : IEvent;

        void Boardcast<T>(T evt) where T : IEvent;

        void Unregister<T>(Action<IEvent> action) where T : IEvent;
    }

    /// <summary>
    /// 
    /// </summary>
    internal class EventChannel : IEventChannel
    {
        private Dictionary<Type, EventListener> listeners = new Dictionary<Type, EventListener>();

        public void Register<T>(Action<IEvent> action) where T : IEvent
        {
            if (action == null) return;

            if (!listeners.TryGetValue(typeof(T), out var listener))
            {
                listener = new EventListener();
                listeners.Add(typeof(T), listener);
            }
            listener.Register(action);
        }

        public void Boardcast<T>(T evt) where T : IEvent
        {
            if (listeners.TryGetValue(typeof(T), out var listener))
            {
                listener.Invoke(evt);
            }
        }

        public void Unregister<T>(Action<IEvent> action) where T : IEvent
        {
            if (action == null) return;

            if (listeners.TryGetValue(typeof(T), out var listener))
            {
                listener.Unregister(action);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class EventListener
    {
        private List<Action<IEvent>> actions = new List<Action<IEvent>>();

        public void Register(Action<IEvent> action)
        {
            actions.Add(action);
        }

        public void Invoke(IEvent evt)
        {
            foreach (var action in actions)
            {
                action.Invoke(evt);
            }
        }

        public void Unregister(Action<IEvent> action)
        {
            actions.Remove(action);
        }
    }
}
