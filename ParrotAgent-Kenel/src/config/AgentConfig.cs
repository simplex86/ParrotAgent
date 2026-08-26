using System;
using System.Collections.Generic;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 顶层配置
    /// 对应 .parrotagent.yaml 的根结构
    /// </summary>
    public sealed record AgentConfig
    {
        /// <summary>
        /// 当前激活的 Provider 名称
        /// 为 null 时回退到 providers[0].name
        /// </summary>
        public string? ActiveProvider { get; init; }

        /// <summary>
        /// Provider 列表
        /// 无配置文件时由 Loader 提供默认 mock 项
        /// </summary>
        public IList<ProviderConfig> Providers { get; init; } = Array.Empty<ProviderConfig>();
        /// <summary>
        /// 上下文管理配置
        /// null 时用默认值
        /// </summary>
        public ContextConfig? Context { get; init; }
    }

    /// <summary>
    /// Provider 配置
    /// </summary>
    public sealed record ProviderConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; init; } = string.Empty;
        /// <summary>
        /// 
        /// mock | openai | anthropic
        /// </summary>
        public string Protocol { get; init; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string Model { get; init; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string BaseUrl { get; init; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string ApiKey { get; init; } = string.Empty;
    }

    /// <summary>
    /// 上下文管理配置
    /// </summary>
    public sealed record ContextConfig
    {
        /// <summary>
        /// 上下文窗口 token 数
        /// </summary>
        public int ContextWindowTokens { get; init; }
        /// <summary>
        /// 警告阈值（占窗口比例）
        /// 默认 0.7
        /// </summary>
        public double? WarningFraction { get; init; }
        /// <summary>
        /// 触发摘要阈值（占窗口比例）
        /// 默认 0.9
        /// </summary>
        public double? TriggerFraction { get; init; }
        /// <summary>
        /// 单条工具结果截断阈值（字符数）
        /// 默认 50_000
        /// </summary>
        public int? PerResultThreshold { get; init; }
        /// <summary>
        /// 一轮内工具结果合计截断阈值（字符数）
        /// 默认 200_000
        /// </summary>
        public int? RoundTotalThreshold { get; init; }
        /// <summary>
        /// 截断后保留预览长度（字符数）
        /// 默认 2_000
        /// </summary>
        public int? PreviewLength { get; init; }
        /// <summary>
        /// 摘要时保留的最近消息数
        /// 默认 4
        /// </summary>
        public int? KeepRecentMessages { get; init; }
        /// <summary>
        /// 熔断器最大连续失败次数
        /// 默认 2
        /// </summary>
        public int? MaxCircuitFailures { get; init; }
        /// <summary>
        /// 是否启用自动压缩
        /// 默认 true；false 时仅截断不摘要
        /// </summary>
        public bool? EnableAutoCompress { get; init; }
    }
}
