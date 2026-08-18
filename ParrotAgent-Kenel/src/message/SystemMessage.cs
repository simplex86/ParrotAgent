using System;
using System.Collections.Generic;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 系统消息
    /// </summary>
    /// <param name="Content"></param>
    internal record SystemMessage(string content) : IMessage
    {
        public MessageRole Role { get; } = MessageRole.System;
        public string Content { get; } = content;
        public IReadOnlyList<ToolCall>? ToolCalls { get; init; }
    }
}
