using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    internal class StateMachine
    {
        private Dictionary<string, IStateNode> states = new Dictionary<string, IStateNode>();
        private IStateNode? current = null;

        public void Add(string name, IStateNode state)
        {
            if (!states.ContainsKey(name))
            {
                states.Add(name, state);
            }
        }

        public async Task Run(string name)
        {
            if (current != null)
            {
                await current.Exit();
                current = null;
            }

            if (states.TryGetValue(name, out var state))
            {
                current = state;
                await current.Enter();
            }
        }
    }
}
