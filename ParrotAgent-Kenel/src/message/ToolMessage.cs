using System;
using System.Collections.Generic;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 工具消息
    /// </summary>
    /// <param name="Content"></param>
    internal record ToolMessage(string content, string id) : IMessage
    {
        /// <summary>
        /// 
        /// </summary>
        public MessageRole Role { get; } = MessageRole.Tool;
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
        public string? ToolCallId { get; init; } = id;
    }
}
