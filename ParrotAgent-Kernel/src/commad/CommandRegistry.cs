using System;
using System.Collections.Generic;
using System.Linq;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CommandRegistry
    {
        /// <summary>
        /// 
        /// </summary>
        public int Count => commands.Count;

        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<string, ICommand> commands = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 
        /// </summary>
        internal void Collect()
        {
            var types = Reflection.FindAll<ICommand, CommandAttribute>();
            foreach (var type in types)
            {
                var command = Reflection.CreateInstance<ICommand, CommandRegistry>(type, this);
                Register(command);
            }
        }

        /// <summary>
        /// 注册命令
        /// 重名抛 ArgumentException
        /// </summary>
        private void Register(ICommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            if (string.IsNullOrWhiteSpace(command.Name))
                throw new ArgumentException("命令名不能为空", nameof(command));
            if (!commands.TryAdd(command.Name, command))
                throw new ArgumentException($"命令名 '{command.Name}' 已注册", nameof(command));
        }

        /// <summary>
        /// 按名查找
        /// 未注册返回 null，调用方决定是否抛错。
        /// </summary>
        public ICommand? Get(string name) => commands.TryGetValue(name, out var tool) ? tool : null;

        /// <summary>
        /// 按名查找
        /// 未注册抛 ArgumentException
        /// </summary>
        public ICommand Require(string name) => Get(name) ?? throw new ArgumentException($"未注册命令：{name}");

        /// <summary>
        /// 所有已注册命令的快照（顺序不保证，按需排序）
        /// </summary>
        public IReadOnlyList<ICommand> GetAll() => [.. commands.Values.Distinct()];
    }
}
