using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class BatchToolExecutor
    {
        /// <summary>
        /// 
        /// </summary>
        private ToolRegistry registry;
        /// <summary>
        /// 
        /// </summary>
        private readonly ToolExecutor executor;
        /// <summary>
        /// 
        /// </summary>
        private readonly IHitl hitl;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="hitl"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public BatchToolExecutor(ToolRegistry registry, IHitl hitl)
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
            this.executor = new ToolExecutor(registry);
            this.hitl = hitl ?? throw new ArgumentNullException(nameof(hitl));
        }

        /// <summary>
        /// 分批执行工具调用列表，返回与输入同序的结果列表。
        /// 流程：按 Category 分组 → Read 并发（分批限流）→ Write 串行 → 按原序合并。
        /// 任何工具失败不中断同批其他工具——失败原因作为 ToolResult.Fail 回灌给 LLM 自我修正。
        /// </summary>
        public async Task<IReadOnlyList<ToolResult>> Execute(IReadOnlyList<ToolCall> calls, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(calls);
            if (calls.Count == 0) return Array.Empty<ToolResult>();

            cancellationToken.ThrowIfCancellationRequested();

            var results = new List<ToolResult>(calls.Count);
            foreach (var call in calls)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var safety = GetSafety(call);
                if (safety != ToolSafety.Safe)
                {
                    var rhitl = await hitl.Request(call, cancellationToken);
                    if (rhitl.Option == HitlOption.Deny)
                    {
                        results.Add(ToolResult.Fail(rhitl.Reason ?? "用户拒绝执行"));
                        continue;
                    }
                }

                var rtool = await executor.Execute(call, cancellationToken);
                results.Add(rtool);
            }

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="call"></param>
        /// <returns></returns>
        private ToolSafety GetSafety(ToolCall call)
        {
            var tool = registry.Get(call.Name);
            return tool.GetType().GetCustomAttribute<ToolAttribute>().Safety;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="call"></param>
        /// <returns></returns>
        private ToolCategory GetCategory(ToolCall call)
        {
            var tool = registry.Get(call.Name);
            return tool.GetType().GetCustomAttribute<ToolAttribute>().Category;
        }
    }
}