namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    internal class StateMachine
    {
        private Dictionary<string, IState> states = new Dictionary<string, IState>();
        private IState? current = null;

        public void Add(string name, IState state)
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
