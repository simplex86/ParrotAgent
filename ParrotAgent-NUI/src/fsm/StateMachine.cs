using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    internal class StateMachine
    {
        private Dictionary<Type, IStateNode> states = new Dictionary<Type, IStateNode>();
        private IStateNode? current = null;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="state"></param>
        public void Add<T>(T state) where T : IStateNode
        {
            var type = typeof(T);
            if (!states.ContainsKey(type))
            {
                states.Add(type, state);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task Run<T>() where T : IStateNode
        {
            var type = typeof(T);

            if (current != null)
            {
                await current.Exit();
                current = null;
            }

            if (states.TryGetValue(type, out var state))
            {
                current = state;
                await current.Enter();
            }
        }
    }
}
