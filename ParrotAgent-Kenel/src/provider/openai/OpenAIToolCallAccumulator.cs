using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParrotAgent.Kenel
{
    // 
    namespace Protocol.OpenAI
    {
        /// <summary>
        /// 工具调用累积器：按 index 拼接 OpenAI 流式 tool_calls 分片。
        /// 协议无关——任何按 index 分片的 tool_calls（OpenAI/兼容协议）都适用。
        /// </summary>
        internal sealed class ToolCallAccumulator
        {
            /// <summary>
            /// 
            /// </summary>
            private sealed class AccumulatorEntry
            {
                public string? Id;
                public string? Name;
                public readonly StringBuilder Args = new();
            }

            /// <summary>
            /// 
            /// </summary>
            private readonly Dictionary<int, AccumulatorEntry> entries = new();

            /// <summary>
            /// 
            /// </summary>
            public bool IsEmpty => entries.Count == 0;

            /// <summary>
            /// 累积一个分片。
            /// Id/Name 取首个非空值，Arguments 拼接所有片段。
            /// </summary>
            public void Accumulate(int index, string? id, string? name, string? args)
            {
                if (!entries.TryGetValue(index, out var entry))
                {
                    entry = new AccumulatorEntry();
                    entries[index] = entry;
                }
                if (id   is not null) entry.Id = id;
                if (name is not null) entry.Name = name;
                if (args is not null) entry.Args.Append(args);
            }

            /// <summary>
            /// 构建完整 ToolCall 列表（按 index 升序）。
            /// Arguments 字符串解析为 JsonElement；空或非法 JSON 用空对象兜底。
            /// </summary>
            public IReadOnlyList<ToolCall> Build()
            {
                var result = new List<ToolCall>(entries.Count);
                foreach (var kv in entries.OrderBy(x => x.Key))
                {
                    var entry = kv.Value;
                    result.Add(new ToolCall(Id: entry.Id ?? $"call_{kv.Key}", Name: entry.Name ?? string.Empty, Args: entry.Args.Length == 0 ? "{}" : entry.Args.ToString()));
                }
                return result;
            }
        }
    }
}
