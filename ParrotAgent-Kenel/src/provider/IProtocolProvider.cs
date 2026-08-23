using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 
    /// </summary>
    internal interface IProtocolProvider
    {
        /// <summary>
        /// 非流式聊天：给定用户输入，返回完整回复。
        /// </summary>
        Task<string> Chat(IReadOnlyList<IMessage> messages, JsonElement? tools, CancellationToken cancellationToken);

        /// <summary>
        /// 流式聊天
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        IAsyncEnumerable<Chunk> ChatStream(IReadOnlyList<IMessage> messages, JsonElement? tools, [EnumeratorCancellation] CancellationToken cancellationToken);
    }
}
