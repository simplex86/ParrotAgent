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
        /// assistant 消息携带的工具调用（仅 Role=Assistant 时可能非空）
        /// </summary>
        IReadOnlyList<ToolCall>? ToolCalls { get; init; }
        /// <summary>
        /// tool 角色消息关联的 tool_call_id
        /// OpenAI 要求 tool 消息必须带 tool_call_id 关联到触发它的 assistant tool_call
        /// </summary>
        string? ToolCallId { get; init; }
    }
}
