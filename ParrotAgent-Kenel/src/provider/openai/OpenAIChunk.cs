using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ParrotAgent.Kenel
{
    // 
    namespace Protocol.OpenAI
    {
        /// <summary>
        /// 工具调用的函数部分（流式分块返回，需要拼接）
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Arguments"></param>
        internal record ToolCallFunctionChunk(
            [property: JsonPropertyName("name")] string? Name,
            // arguments 是流式拼接的 JSON 字符串片段，比如第一段返回 "{\"location\": "，第二段返回 "\"Beijing\"}"
            [property: JsonPropertyName("arguments")] string? Arguments
        );

        /// <summary>
        /// 单个工具调用的流式块（index 用于标识同一个 tool call 的不同分片）
        /// </summary>
        /// <param name="Index"></param>
        /// <param name="Id"></param>
        /// <param name="Type"></param>
        /// <param name="Function"></param>
        internal record ToolCallChunk(
            // 必选：同一个 tool call 的所有分片 index 相同
            [property: JsonPropertyName("index")] int? Index,
            // 工具调用 ID，仅在第一段出现
            [property: JsonPropertyName("id")] string? Id,
            // 固定为 "function"（OpenAI 规范）
            [property: JsonPropertyName("type")] string? Type,
            [property: JsonPropertyName("function")] ToolCallFunctionChunk? Function
        );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Role"></param>
        /// <param name="Content"></param>
        /// <param name="ToolCalls"></param>
        /// <param name="Refusal"></param>
        internal record Delta(
            [property: JsonPropertyName("role")] string? Role,
            [property: JsonPropertyName("content")] string? Content,
            // 核心：工具调用数组，可选
            [property: JsonPropertyName("tool_calls")] IReadOnlyList<ToolCallChunk>? ToolCalls,
            // 可选：拒绝生成的内容（比如安全过滤）
            [property: JsonPropertyName("refusal")] string? Refusal
        );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Index"></param>
        /// <param name="Delta"></param>
        /// <param name="FinishReason"></param>
        /// <param name="Logprobs"></param>
        internal record Choice(
            [property: JsonPropertyName("index")] int Index,
            [property: JsonPropertyName("delta")] Delta? Delta,
            // 当 finish_reason 为 "tool_calls" 时，说明工具调用返回完毕
            [property: JsonPropertyName("finish_reason")] string? FinishReason,
            [property: JsonPropertyName("logprobs")] JsonElement? Logprobs
        );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="PromptTokens"></param>
        /// <param name="CompletionTokens"></param>
        /// <param name="TotalTokens"></param>
        internal record Usage(
            [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
            [property: JsonPropertyName("completion_tokens")] int CompletionTokens,
            [property: JsonPropertyName("total_tokens")] int TotalTokens
        );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Object"></param>
        /// <param name="Created"></param>
        /// <param name="Model"></param>
        /// <param name="Choices"></param>
        /// <param name="Usage"></param>
        /// <param name="SystemFingerprint"></param>
        internal record ChatChunk(
            [property: JsonPropertyName("id")] string? Id,
            [property: JsonPropertyName("object")] string? Object,
            [property: JsonPropertyName("created")] long? Created,
            [property: JsonPropertyName("model")] string? Model,
            [property: JsonPropertyName("choices")] IReadOnlyList<Choice>? Choices,
            [property: JsonPropertyName("usage")] Usage? Usage,
            // 可选：系统指纹（部分 provider 支持）
            [property: JsonPropertyName("system_fingerprint")] string? SystemFingerprint
        );
    }
}
