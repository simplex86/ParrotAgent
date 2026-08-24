using System;
using System.Threading;
using System.Threading.Tasks;
using ParrotAgent.Kenel;
using ParrotAgent.Tool;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    internal class InitStateNode : StateNode
    {
        /// <summary>
        /// 
        /// </summary>
        private EventSink eventSink;

        /// <summary>
        /// 
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="cancellationTokenSource"></param>
        public InitStateNode(StateMachine machine, EventSink eventSink, CancellationTokenSource cancellationTokenSource) 
            : base(machine)
        {
            this.eventSink = eventSink;
            this.cancellationTokenSource = cancellationTokenSource;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override async Task Enter()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Initializing...");
            Console.ResetColor();

            eventSink.Output.Register<AgentBeginEvent>(OnAgentBeignHandler);

            var toolRegistry = new ToolRegistry();
            toolRegistry.Register(new GetOSTool());
            toolRegistry.Register(new RunCommandTool());

            var entry = new AgentEntry(toolRegistry, eventSink, cancellationTokenSource.Token);
            entry.Run();

            await Machine.Run<PendingStateNode>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override async Task Exit()
        {
            eventSink.Output.Unregister<AgentBeginEvent>(OnAgentBeignHandler);
            await Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        private void OnAgentBeignHandler(IEvent e)
        {
            var evt = (AgentBeginEvent)e;

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"Provider = {evt.Provider}, Protocol = {evt.Protocol}, Tool Count = {evt.ToolCount}");
            Console.ResetColor();
        }
    }
}
