using System;
using System.Collections.Generic;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// 系统消息
    /// </summary>
    /// <param name="Content"></param>
    internal record SystemMessage(string content) : IMessage
    {
        /// <summary>
        /// 
        /// </summary>
        public MessageRole Role { get; } = MessageRole.System;
        /// <summary>
        /// 
        /// </summary>
        public string Content { get; } = content;
        /// <summary>
        /// 
        /// </summary>
        public IReadOnlyList<ToolCall>? ToolCalls { get; init; }
        /// <summary>
        /// 
        /// </summary>
        public string? ToolCallId { get; init; }
    }
}
