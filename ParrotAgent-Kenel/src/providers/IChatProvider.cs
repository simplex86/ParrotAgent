using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 
    /// </summary>
    public interface IChatProvider
    {
        /// <summary>
        /// 非流式聊天：给定用户输入，返回完整回复。
        /// </summary>
        Task<string> ChatAsync(string userInput, CancellationToken cancellationToken);
    }
}
