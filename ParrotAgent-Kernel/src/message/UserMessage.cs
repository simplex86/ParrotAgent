using System.Collections.Generic;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// 用户消息
    /// </summary>
    /// <param name="Content"></param>
    internal record UserMessage(string content) : IMessage
    {
        /// <summary>
        /// 
        /// </summary>
        public MessageRole Role { get; } = MessageRole.User;
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
