namespace ParrotAgent.Kenel
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
        public int ToolCount;
    }

    /// <summary>
    /// Agent终止
    /// </summary>
    public struct AgentEndEvent : IEvent 
    { 
    
    }

    /// <summary>
    /// User Prompt
    /// </summary>
    public struct UserPromptEvent : IEvent
    {
        public string Prompt;
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
}
