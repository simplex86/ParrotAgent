using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// LLM消息
    /// </summary>
    /// <param name="Content"></param>
    internal record AssistantMessage(string content) : IMessage
    {
        public MessageRole Role { get; } = MessageRole.Assistant;
        public string Content { get; } = content;
        public IReadOnlyList<ToolCall>? ToolCalls { get; init; }
    }
}
