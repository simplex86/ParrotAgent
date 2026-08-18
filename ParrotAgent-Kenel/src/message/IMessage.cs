using System;
using System.Collections.Generic;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 
    /// </summary>
    public enum MessageRole
    {
        /// <summary>
        /// 系统
        /// </summary>
        System,
        /// <summary>
        /// 用户
        /// </summary>
        User,
        /// <summary>
        /// LLM
        /// </summary>
        Assistant,
        /// <summary>
        /// 工具
        /// </summary>
        Tool
    }

    /// <summary>
    /// 协议中性的消息。
    /// ToolCalls 仅 assistant 消息可能非空。
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// 
        /// </summary>
        MessageRole Role { get; }
        /// <summary>
        /// 
        /// </summary>
        string Content { get; }
        /// <summary>
        /// 
        /// </summary>
        IReadOnlyList<ToolCall>? ToolCalls { get; init; }
    }
}
