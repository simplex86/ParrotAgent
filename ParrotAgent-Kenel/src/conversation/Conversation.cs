using System;
using System.Collections.Generic;

namespace ParrotAgent.Kenel
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
        /// 估算的全部历史 token 数（字符数 / 3 近似）。
        /// </summary>
        public int EstimatedTokens => TokenEstimator.Estimate(history);

        /// <summary>
        /// 追加 user 消息。
        /// </summary>
        public void AddUser(string content)
        {
            ArgumentNullException.ThrowIfNull(content);
            history.Add(new UserMessage(content));
        }

        /// <summary>
        /// 追加 assistant 消息（AI 的完整回复）。
        /// </summary>
        public void AddAssistant(string content)
        {
            ArgumentNullException.ThrowIfNull(content);
            history.Add(new AssistantMessage(content));
        }

        /// <summary>
        /// 追加 assistant 消息（AI 的完整回复）。
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
        /// 
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<IMessage> ToProviderMessages()
        {
            return history;
        }
    }
}
