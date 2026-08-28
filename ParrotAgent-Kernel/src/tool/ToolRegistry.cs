using System;
using System.Collections.Generic;
using System.Linq;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// 工具注册中心：按名查找 + 批量 schema 转换。
    /// AgentLoop在调用 LLM 前注入 ToolRegistry.ToOpenAISchemas()，让 LLM 知道有哪些工具可用。
    /// </summary>
    internal sealed class ToolRegistry
    {
        /// <summary>
        /// 
        /// </summary>
        public int Count => tools.Count;

        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<string, ITool> tools = new(StringComparer.Ordinal);

        /// <summary>
        /// 
        /// </summary>
        public void Collect()
        {
            var types = Reflection.FindAll<ITool, ToolAttribute>();
            foreach (var type in types)
            {
                var tool = Reflection.CreateInstance<ITool>(type);
                Register(tool);
            }
        }

        /// <summary>
        /// 注册工具。重名抛 ArgumentException（工具名应跨工具唯一）。
        /// </summary>
        public void Register(ITool tool)
        {
            ArgumentNullException.ThrowIfNull(tool);
            if (string.IsNullOrWhiteSpace(tool.Name))
                throw new ArgumentException("工具名不能为空", nameof(tool));
            if (!tools.TryAdd(tool.Name, tool))
                throw new ArgumentException($"工具名 '{tool.Name}' 已注册", nameof(tool));
        }

        /// <summary>
        /// 按名查找。未注册返回 null，调用方决定是否抛错。
        /// </summary>
        public ITool? Get(string name) => tools.TryGetValue(name, out var tool) ? tool : null;

        /// <summary>
        /// 按名查找。未注册抛 ArgumentException（用于"工具必须存在"的场景）。
        /// </summary>
        public ITool Require(string name) => Get(name) ?? throw new ArgumentException($"未注册工具：{name}");

        /// <summary>
        /// 所有已注册工具的快照（顺序不保证，按需排序）。
        /// </summary>
        public IReadOnlyList<ITool> GetAll() => tools.Values.ToArray();
    }
}
