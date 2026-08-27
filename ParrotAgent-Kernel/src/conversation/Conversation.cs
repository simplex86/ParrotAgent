using System;
using System.Collections.Generic;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// 
    /// </summary>
    public interface IConversation
    {
        IReadOnlyList<IMessage> ToProviderMessages();
    }

    /// <summary>
    /// 对话记录
    /// </summary>
    internal class Conversation : IConversation
    {
        /// <summary>
        /// 历史信息
        /// </summary>
        private List<IMessage> messages = new List<IMessage>();

        /// <summary>
        /// 追加 user 消息
        /// </summary>
        public void AddUser(string content)
        {
            ArgumentNullException.ThrowIfNull(content);
            messages.Add(new UserMessage(content));
        }

        /// <summary>
        /// 追加 assistant 消息（AI 的完整回复）
        /// </summary>
        public void AddAssistant(string content)
        {
            ArgumentNullException.ThrowIfNull(content);
            messages.Add(new AssistantMessage(content));
        }

        /// <summary>
        /// 追加 assistant 消息（AI 的完整回复）
        /// </summary>
        public void AddAssistant(string content, IReadOnlyList<ToolCall> toolcalls)
        {
            ArgumentNullException.ThrowIfNull(content);
            ArgumentNullException.ThrowIfNull(toolcalls);
            messages.Add(new AssistantMessage(content) { ToolCalls = toolcalls});
        }

        /// <summary>
        /// 追加 tool 消息
        /// </summary>
        public void AddTool(string content, string callId)
        {
            ArgumentNullException.ThrowIfNull(content);
            ArgumentNullException.ThrowIfNull(callId);
            messages.Add(new ToolMessage(content, callId));
        }

        /// <summary>
        /// 替换全部消息
        /// 供 Summarizer.Summarize 调用
        /// </summary>
        public void ReplaceMessages(IReadOnlyList<IMessage> messages)
        {
            ArgumentNullException.ThrowIfNull(messages);
            this.messages.Clear();
            this.messages.AddRange(messages);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<IMessage> ToProviderMessages()
        {
            return messages;
        }
    }
}
