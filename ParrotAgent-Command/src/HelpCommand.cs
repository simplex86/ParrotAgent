using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParrotAgent.Kernel;

namespace ParrotAgent.Command
{
    /// <summary>
    /// /help：显示可用命令列表
    /// </summary>
    [Command]
    internal sealed class HelpCommand : ICommand
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly CommandRegistry registry;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="registry"></param>
        public HelpCommand(CommandRegistry registry)
        {
            this.registry = registry;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name => "help";
        /// <summary>
        /// 
        /// </summary>
        public string Description => "显示可用命令列表";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<CommandResult> Execute(CommandContext context)
        {
            var sb = new StringBuilder();
            sb.AppendLine("可用命令：");
            foreach (var cmd in registry.GetAll().OrderBy(c => c.Name))
                sb.AppendLine($"  /{Name}  {cmd.Description}");
            sb.AppendLine();
            sb.AppendLine("提示：输入消息与 AI 对话；/ 开头走命令。");

            return Task.FromResult(new CommandResult() { Handled = true, Output = sb.ToString() });
        }
    }
}
