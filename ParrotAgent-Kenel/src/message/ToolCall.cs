using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// LLM 发起的工具调用。
    /// Input 为原始 JSON（保留协议细节，由 Provider 层解释）
    /// </summary>
    //public sealed record ToolCall(string Id, string Name, JsonElement Input);
}
