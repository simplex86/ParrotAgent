using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 压缩结果
    /// </summary>
    internal sealed record CompressionResult
    {
        public bool WasCompressed { get; init; }
        public int MessagesCompressed { get; init; }
        public int EstimatedTokensSaved { get; init; }
        public string? Message { get; init; }
        public bool BreakerOpen { get; init; }
    }

    /// <summary>
    /// 
    /// </summary>
    internal abstract record Compression
    {
        internal record None() : Compression;
        internal record Warning(string? Message, bool BreakerOpen) : Compression
        {
            public Warning() : this(string.Empty, false) { }
        };
        internal record Compress() : Compression;
    }

    /// <summary>
    /// Token 压缩器
    /// </summary>
    internal sealed class Compressor
    {
        private readonly Truncator truncator;
        private readonly Summarizer summarizer;
        private readonly bool enableAutoCompress;
        private bool warningEmitted;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="contextWindowTokens"></param>
        /// <param name="truncateConfig"></param>
        /// <param name="warningFraction"></param>
        /// <param name="triggerFraction"></param>
        /// <param name="keepRecent"></param>
        /// <param name="maxCircuitFailures"></param>
        /// <param name="enableAutoCompress"></param>
        /// <param name="projectRoot"></param>
        public Compressor(IProtocolProvider provider,
                          int contextWindowTokens,
                          TruncateConfig? truncateConfig = null,
                          double warningFraction = 0.7,
                          double triggerFraction = 0.9,
                          int keepRecent = 4,
                          int maxCircuitFailures = 2,
                          bool enableAutoCompress = true,
                          string? projectRoot = null)
        {
            truncator = new Truncator(truncateConfig, projectRoot);
            summarizer = new Summarizer(provider,
                                                   contextWindowTokens,
                                                   warningFraction,
                                                   triggerFraction,
                                                   keepRecent,
                                                   maxCircuitFailures);
            this.enableAutoCompress = enableAutoCompress;
        }

        /// <summary>
        /// 截断
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="toolnames"></param>
        /// <returns></returns>

        public (string[] TruncatedContents, IReadOnlyList<TruncationInfo> Infos) TruncateBatch(IReadOnlyList<string> contents, IReadOnlyList<string> toolnames)
            => truncator.TruncateBatch(contents, toolnames);

        public int ContextWindow => summarizer.ContextWindow;
        public int WarningThreshold => summarizer.WarningThreshold;
        public int TriggerThreshold => summarizer.TriggerThreshold;
        public bool BreakerOpen => summarizer.BreakerOpen;
        public int BreakerFailures => summarizer.BreakerFailures;

        /// <summary>
        /// 重置熔断器
        /// </summary>
        public void ResetBreaker() => summarizer.ResetBreaker();

        /// <summary>
        /// 重置警告标志（/clear 或压缩成功后调用）
        /// </summary>
        public void ResetWarning() => warningEmitted = false;

        /// <summary>
        /// 检查压缩，在每轮 LLM 调用前调用
        /// 1. enableAutoCompress=false → 直接返回
        /// 2. token > 警告阈值 → 发警告（仅一次）
        /// 3. 熔断器 open → 跳过
        /// 4. token > 触发阈值 → 触发摘要
        /// </summary>
        public async Task<Compression> CheckCompressable(Conversation conversation, CancellationToken cancellationToken)
        {
            // enable_auto_compress: false → 跳过
            if (!enableAutoCompress)
                return new Compression.None();

            var warning = new Compression.Warning();
            var messages = conversation.ToProviderMessages();

            // 1. 警告检查（仅一次）
            if (!warningEmitted && summarizer.NeedsWarning(messages))
            {
                warning = warning with
                {
                    Message = "上下文即将不足，建议保存当前会话并开启新对话"
                };
            }

            // 2. 熔断器检查
            if (summarizer.BreakerOpen)
            {
                warning = warning with
                {
                    BreakerOpen = true,
                    Message = "自动压缩已禁用（摘要连续失败），请手动 /compress 或开启新会话"
                };
                return warning;
            }

            // 3. 触发摘要
            return summarizer.NeedsCompression(messages) ? new Compression.Compress()
                                                         : warning;
        }

        /// <summary>
        /// 执行压缩，在每轮 LLM 调用前调用
        /// 1. enableAutoCompress=false → 直接返回
        /// 2. token > 警告阈值 → 发警告（仅一次）
        /// 3. 熔断器 open → 跳过
        /// 4. token > 触发阈值 → 触发摘要
        /// </summary>
        public async Task<CompressionResult> Compress(Conversation conversation, CancellationToken cancellationToken)
        {
            var summary = await summarizer.Summarize(conversation, cancellationToken);
            if (!summary.Success)
            {
                var result = new CompressionResult() { WasCompressed = false };
                // 摘要失败（熔断器已递增）
                if (summarizer.BreakerOpen)
                {
                    result = result with
                    {
                        Message = "自动压缩已禁用（摘要连续失败 2 次），请手动 /compress 或开启新会话",
                        BreakerOpen = true,
                    };
                }
                return result;
            }

            // 摘要成功 → 压缩后 token 降下来，重置警告
            warningEmitted = false;

            return new CompressionResult
            {
                WasCompressed = true,
                MessagesCompressed = summary.MessagesCompressed,
                EstimatedTokensSaved = summary.EstimatedTokensSaved
            };
        }
    }
}
