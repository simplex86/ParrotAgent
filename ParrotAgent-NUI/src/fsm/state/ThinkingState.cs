using ParrotAgent.Kenel;

namespace ParrotAgent.NUI
{
    internal class ThinkingState : IState
    {
        private StateMachine machine;
        private SinkChannel outputChannel;
        private bool thinking = false;

        public ThinkingState(StateMachine machine, SinkChannel outputChannel)
        {
            this.machine = machine;
            this.outputChannel = outputChannel;
        }

        public async Task Enter()
        {
            outputChannel.Add(OnOutputHandler);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Thinking...");
            Console.ResetColor();

            thinking = true;
            await Task.Run(async () => { while (thinking); await machine.Run("pending"); });
        }

        public async Task Exit()
        {
            outputChannel.Remove(OnOutputHandler);
        }

        private async Task OnOutputHandler(string text)
        {
            Console.WriteLine(text);
            thinking = false;
        }
    }
}
