using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// 
    /// </summary>
    internal class Agent
    {
        /// <summary>
        /// 
        /// </summary>
        private AgentLoop agentLoop;
        /// <summary>
        /// 
        /// </summary>
        private Conversation conversation = null;
        /// <summary>
        /// 
        /// </summary>
        private Compressor compressor;
        /// <summary>
        /// 
        /// </summary>
        private CommandExecutor commandExecutor;
        /// <summary>
        /// 
        /// </summary>
        private EventDispatcher eventDispatcher;
        /// <summary>
        /// 
        /// </summary>
        private CancellationToken cancellationToken;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chatProvider"></param>
        /// <param name="toolRegistry"></param>
        /// <param name="commandRegistry"></param>
        /// <param name="eventDispatcher"></param>
        /// <param name="compressor"></param>
        /// <param name="cancellationToken"></param>
        public Agent(IProtocolProvider chatProvider, 
                     ToolRegistry toolRegistry, 
                     CommandRegistry commandRegistry, 
                     EventDispatcher eventDispatcher, 
                     Compressor compressor, 
                     CancellationToken cancellationToken)
        {
            this.compressor = compressor;
            this.agentLoop = new AgentLoop(chatProvider, toolRegistry, eventDispatcher, compressor);
            this.conversation = new Conversation();
            this.commandExecutor = new CommandExecutor(commandRegistry);
            this.eventDispatcher = eventDispatcher;
            this.cancellationToken = cancellationToken;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var taskCompletionSource = new TaskCompletionSource<string>();
                    await eventDispatcher.Dispatch(new UserPromptEvent() { TaskCompletionSource = taskCompletionSource });
                    var input = await taskCompletionSource.Task;

                    var commandResult = await ExecuteCommand(input);
                    if (commandResult.Handled)
                    {
                        if (!string.IsNullOrEmpty(commandResult.Output))
                        {
                            await eventDispatcher.Dispatch(new CommandResultEvent() { Result = commandResult });
                        }
                        continue; // 是命令且已经被处理，不再灌给LLM
                    }

                    conversation.AddUser(input);
                    await agentLoop.Run(conversation, true, cancellationToken);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private async Task<CommandResult> ExecuteCommand(string input)
        {
            var context = new CommandContext(conversation, compressor, cancellationToken);
            return await commandExecutor.Execute(input, context, cancellationToken);
        }
    }
}
