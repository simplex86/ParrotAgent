using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 固定回显 Provider，用于在接入真实 LLM 前跑通管线。
    /// </summary>
    internal sealed class MockProvider : IChatProvider
    {
        /// <summary>
        /// 非流式聊天：给定用户输入，返回完整回复。
        /// </summary>
        public async Task<string> Chat(IReadOnlyList<IMessage> messages, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return $"Mock: {messages[^1].Content}";
        }
    }
}
