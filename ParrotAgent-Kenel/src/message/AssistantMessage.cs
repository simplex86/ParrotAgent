using System;
using System.Collections.Generic;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// LLM消息
    /// </summary>
    /// <param name="Content"></param>
    internal record AssistantMessage(string content) : IMessage
    {
        /// <summary>
        /// 
        /// </summary>
        public MessageRole Role { get; } = MessageRole.Assistant;
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
