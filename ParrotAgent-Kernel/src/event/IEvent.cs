using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// 事件接口
    /// </summary>
    public interface IEvent
    {
    }

    /// <summary>
    /// Agent启动
    /// </summary>
    public struct AgentBeginEvent : IEvent
    {
        public string Provider;
        public string Protocol;
        public int? ContextWindowTokens;
    }

    /// <summary>
    /// Agent终止
    /// </summary>
    public struct AgentEndEvent : IEvent 
    {

    }

    /// <summary>
    /// 
    /// </summary>
    public struct McpServerBeginEvent : IEvent
    {
        public int TotalCount;
    }

    /// <summary>
    /// 
    /// </summary>
    public struct McpServerResultEvent : IEvent
    {
        public string Name;
        public bool Success;
        public int ToolCount;
    }

    /// <summary>
    /// 
    /// </summary>
    public struct McpServerEndEvent : IEvent
    {
        public int TotalCount;
        public int ConnectedCount;
    }

    /// <summary>
    /// User Prompt
    /// </summary>
    public struct UserPromptEvent : IEvent
    {
        public TaskCompletionSource<string> TaskCompletionSource;
    }

    /// <summary>
    /// Assistant 回复
    /// </summary>
    public struct AssistantDeltaEvent : IEvent
    {
        public string Delta;
    }

    /// <summary>
    /// Assistant 回复结束
    /// </summary>
    public struct AssistantCompletedEvent : IEvent
    {
        public int PromptTokens;
        public int TotalTokens;
    }

    /// <summary>
    /// 
    /// </summary>
    public struct ToolCallEvent : IEvent
    {
        public ToolCall Call;
    }

    /// <summary>
    /// 
    /// </summary>
    public struct ToolResultEvent : IEvent
    {
        public ToolCall Call;
        public ToolResult Result;
    }

    /// <summary>
    /// 
    /// </summary>
    public struct HitlEvent : IEvent
    {
        public ToolCall Call;
        public TaskCompletionSource<HitlOption> TaskCompletionSource;
    }

    /// <summary>
    /// 
    /// </summary>
    public struct ContextWarningEvent : IEvent
    {
        public string Message;
    }

    /// <summary>
    /// 
    /// </summary>
    public struct ContextCompressingEvent : IEvent
    {

    }

    /// <summary>
    /// 
    /// </summary>
    public struct ContextCompressedEvent : IEvent
    {
        public bool WasCompressed;
        public int MessagesCompressed;
        public int TokensSaved;
    }

    /// <summary>
    /// 
    /// </summary>
    public struct CommandResultEvent : IEvent
    {
        public CommandResult Result;
    }
}
