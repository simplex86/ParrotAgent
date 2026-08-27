using System;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// 命令分发器：判断输入是否为命令，若是则查找并执行。
    /// "/" 前缀 → 查 Registry → 执行 ICommand.ExecuteAsync
    /// 非 "/" 前缀 → 返回 CommandResult.NotHandled（回退到 AI）
    /// </summary>
    internal sealed class CommandExecutor
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly CommandRegistry registry;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="registry"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public CommandExecutor(CommandRegistry registry)
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<CommandResult> Execute(string line, CommandContext context, CancellationToken cancellationToken)
        {
            var parsed = CommandParser.Parse(line);
            if (parsed is null)
                return new CommandResult() { Handled = false };

            var (name, _) = parsed.Value;
            var command = registry.Get(name);
            if (command is null)
                return new CommandResult() { Handled = true, Output = $"未知命令: /{name}，输入 /help 查看可用命令" };

            var ctx = context with { RawInput = line, CancellationToken = cancellationToken };

            try
            {
                return await command.Execute(ctx);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new CommandResult() { Handled = true, Output = $"[!] 执行命令 /{name} 失败：{ex.Message}" };
            }
        }
    }
}
