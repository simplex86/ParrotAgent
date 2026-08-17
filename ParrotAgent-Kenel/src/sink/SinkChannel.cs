using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 
    /// </summary>
    public class SinkChannel
    {
        /// <summary>
        /// 
        /// </summary>
        private List<Func<string, Task>> actions = new List<Func<string, Task>>();

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
            foreach (var action in actions)
            {
                action.Invoke(text);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void Add(Func<string, Task> action)
        {
            if (action != null)
            {
                actions.Add(action);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void Remove(Func<string, Task> action)
        {
            if (action != null)
            {
                actions.Remove(action);
            }
        }
    }
}
