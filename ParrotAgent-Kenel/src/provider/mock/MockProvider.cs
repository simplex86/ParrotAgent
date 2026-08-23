using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 固定回显 Provider，用于在接入真实 LLM 前跑通管线。
    /// </summary>
    internal sealed class MockProvider : IProtocolProvider
    {
        /// <summary>
        /// 非流式聊天：给定用户输入，返回完整回复。
        /// </summary>
        public async Task<string> Chat(IReadOnlyList<IMessage> messages, JsonElement? tools, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return $"Mock: {messages[^1].Content}";
        }

        /// <summary>
        /// 流式聊天
        /// 不模拟逐字延迟，一次性产出完整回复。消费方（App）的 await foreach 逻辑与真实 Provider 一致，验证流式管线正确性。
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<Chunk> ChatStream(IReadOnlyList<IMessage> messages, JsonElement? tools, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var lastUser = messages.LastOrDefault(m => m.Role == MessageRole.User);
            var content = lastUser?.Content ?? string.Empty;

            yield return new Chunk.TextDelta($"Mock: {content}");
            yield return new Chunk.Done($"done");

            await Task.CompletedTask;
        }
    }
}
