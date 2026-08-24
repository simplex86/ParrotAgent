using System;
using System.Collections.Generic;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// LLM 流式响应的协议中性单元。
    /// Provider 层把 OpenAI / Anthropic wire format 翻译成 Chunk，AgentLoop 只消费 Chunk，不感知协议细节。
    /// </summary>
    public abstract record Chunk
    {
        /// <summary>
        /// 文本增量
        /// LLM 产出的回复文本按片段到达，消费方拼接得到完整回复
        /// </summary>
        /// <param name="Content"></param>
        public record TextDelta(string Content) : Chunk;
        /// <summary>
        /// 工具调用增量
        /// OpenAI 流式中 tool_calls 按 index 分片到达：
        /// - 首片通常含 Id + Name（arguments 可能空或开始片段）
        /// - 后续片只含 ArgumentsFragment（arguments JSON 字符串的片段）
        /// - 同 index 的多片需累积：Id/Name 取首个非空，Arguments 拼接所有片段
        /// AgentLoop 按 Index 累积，流结束后 Build 成完整 ToolCall。
        /// </summary>
        /// <param name="Calls"></param>
        public record ToolCalls(IReadOnlyList<ToolCall> Calls) : Chunk;
        /// <summary>
        /// 流终止标记（OpenAI 的 data: [DONE]）
        /// 收到此 chunk 后 AgentLoop 停止本轮流式消费，进入 tool_calls 构建阶段
        /// </summary>
        /// <param name="Reason"></param>
        public record Done(string Reason) : Chunk;
    }

    /// <summary>
    /// 完整的工具调用对象（最终给业务用的）
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Type"></param>
    /// <param name="Name"></param>
    /// <param name="Args">完整 JSON，可直接反序列化为业务参数对象</param>
    public record ToolCall(
        string Id,
        //string Type,
        string Name,
        string Args
    );
}
