using ParrotAgent.Kenel;

namespace ParrotAgent.NUI
{
    internal class PendingState : IState
    {
        private StateMachine machine;
        private SinkChannel inputChannel;
        
        public PendingState(StateMachine machine, SinkChannel inputChannel) 
        { 
            this.machine = machine;
            this.inputChannel = inputChannel;
        }

        public async Task Enter()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("> ");
            var input = Console.ReadLine();
            Console.ResetColor();

            inputChannel.Write(input);
            await machine.Run("thinking");
        }

        public async Task Exit()
        {
            await Task.CompletedTask;
        }
    }
}
