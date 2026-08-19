using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParrotAgent.Kenel
{
    public class SinkEvent
    {
        /// <summary>
        /// 
        /// </summary>
        private List<Action<string>> actions = new List<Action<string>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void Register(Action<string> action)
        {
            actions.Add(action);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public void Invoke()
        {
            foreach (var action in actions)
            {
                action?.Invoke(null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public void Invoke(string text)
        {
            foreach (var action in actions)
            {
                action?.Invoke(text);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void Unregister(Action<string> action)
        {
            actions.Remove(action);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface ISinkChannel
    {
        /// <summary>
        /// 
        /// </summary>
        SinkEvent OnChanged { get; }

        /// <summary>
        /// 
        /// </summary>
        SinkEvent OnCompleted { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        void Write(string text);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        void Complete();
    }

    /// <summary>
    /// 
    /// </summary>
    public class SinkChannel : ISinkChannel
    {
        /// <summary>
        /// 
        /// </summary>
        public SinkEvent OnChanged { get; } = new SinkEvent();

        /// <summary>
        /// 
        /// </summary>
        public SinkEvent OnCompleted { get; } = new SinkEvent();

        /// <summary>
        /// 
        /// </summary>
        internal SinkChannel()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public void Write(string text)
        {
            OnChanged.Invoke(text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void Complete()
        {
            OnCompleted?.Invoke();
        }
    }
}
