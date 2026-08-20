using System;
using System.Text.Json.Serialization;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Role"></param>
    /// <param name="Content"></param>
    public record Delta(
        [property: JsonPropertyName("role")] string? Role,
        [property: JsonPropertyName("content")] string? Content
    );

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Index"></param>
    /// <param name="Delta"></param>
    /// <param name="FinishReason"></param>
    public record Choice(
        [property: JsonPropertyName("index")] int Index,
        [property: JsonPropertyName("delta")] Delta? Delta,
        [property: JsonPropertyName("finish_reason")] string? FinishReason
    );

    /// <summary>
    /// 
    /// </summary>
    /// <param name="PromptTokens"></param>
    /// <param name="CompletionTokens"></param>
    /// <param name="TotalTokens"></param>
    public record Usage(
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
    /// /// <param name="Usage"></param>
    public record ChatChunk(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("object")] string? Object,
        [property: JsonPropertyName("created")] long? Created,
        [property: JsonPropertyName("model")] string? Model,
        [property: JsonPropertyName("choices")] Choice[]? Choices,
        [property: JsonPropertyName("usage")] Usage? Usage
    );
}
