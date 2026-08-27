using System;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// 命令执行上下文：封装命令执行时需要的所有依赖
    /// </summary>
    public sealed record CommandContext(IConversation Conversation,
                                        ICompressor? Compressor,
                                        CancellationToken CancellationToken)
    {
        /// <summary>
        /// 当前 Provider 配置（/status 用）
        /// 必填
        /// </summary>
        public ProviderConfig ProviderConfig { get; init; } = null!;

        /// <summary>
        /// 当前 AgentConfig（/status 用）
        /// 必填
        /// </summary>
        public AgentConfig AgentConfig { get; init; } = null!;

        /// <summary>
        /// 项目指令加载概要（/status 显示）
        /// </summary>
        public string? InstructionSummary { get; init; }

        /// <summary>
        /// 原始输入行（含 / 前缀，便于错误提示引用与参数解析）
        /// </summary>
        public string RawInput { get; init; } = string.Empty;
    }

    /// <summary>
    /// 命令执行结果
    /// </summary>
    public sealed record CommandResult
    {
        /// <summary>
        /// 命令是否被处理（true=已处理，false=未识别/未处理，回退到 AI）
        /// </summary>
        public bool Handled { get; init; }
        /// <summary>
        /// 命令输出文本
        /// </summary>
        public string? Output { get; init; }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// 
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 命令描述（/help 展示用，简短一句话）
        /// </summary>
        string Description { get; }
        /// <summary>
        /// 执行命令。返回 CommandResult。
        /// 命令不应抛异常——错误信息通过 CommandResult.Output 返回。
        /// </summary>
        Task<CommandResult> Execute(CommandContext context);
    }

    /// <summary>
    /// 
    /// </summary>
    public class CommandAttribute : Attribute
    {

    }
}
