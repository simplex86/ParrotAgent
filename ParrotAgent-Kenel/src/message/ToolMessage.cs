using System;
using System.Collections.Generic;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 工具消息
    /// </summary>
    /// <param name="Content"></param>
    internal record ToolMessage(string content) : IMessage
    {
        public MessageRole Role { get; } = MessageRole.Tool;
        public string Content { get; } = content;
        public IReadOnlyList<ToolCall>? ToolCalls { get; init; }
    }
}
