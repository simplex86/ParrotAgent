using System;
using System.Collections.Generic;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// 对话记录
    /// </summary>
    internal class Conversation
    {
        /// <summary>
        /// 历史信息
        /// </summary>
        private List<IMessage> history = new List<IMessage>();

        /// <summary>
        /// 追加 user 消息
        /// </summary>
        public void AddUser(string content)
        {
            ArgumentNullException.ThrowIfNull(content);
            history.Add(new UserMessage(content));
        }

        /// <summary>
        /// 追加 assistant 消息（AI 的完整回复）
        /// </summary>
        public void AddAssistant(string content)
        {
            ArgumentNullException.ThrowIfNull(content);
            history.Add(new AssistantMessage(content));
        }

        /// <summary>
        /// 追加 assistant 消息（AI 的完整回复）
        /// </summary>
        public void AddAssistant(string content, IReadOnlyList<ToolCall> toolcalls)
        {
            ArgumentNullException.ThrowIfNull(content);
            ArgumentNullException.ThrowIfNull(toolcalls);
            history.Add(new AssistantMessage(content) { ToolCalls = toolcalls});
        }

        /// <summary>
        /// 追加 tool 消息
        /// </summary>
        public void AddTool(string content, string callId)
        {
            ArgumentNullException.ThrowIfNull(content);
            ArgumentNullException.ThrowIfNull(callId);
            history.Add(new ToolMessage(content, callId));
        }

        /// <summary>
        /// 替换全部消息
        /// 供 Summarizer.Summarize 调用
        /// </summary>
        public void ReplaceMessages(IReadOnlyList<IMessage> messages)
        {
            ArgumentNullException.ThrowIfNull(messages);
            history.Clear();
            history.AddRange(messages);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<IMessage> ToProviderMessages()
        {
            return history;
        }
    }
}
