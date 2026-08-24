using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class AgentLoop
    {
        /// <summary>
        /// 
        /// </summary>
        private IProtocolProvider chatProvider;
        /// <summary>
        /// 
        /// </summary>
        private EventSink eventSink;
        /// <summary>
        /// 
        /// </summary>
        private ToolRegistry toolRegistry;
        /// <summary>
        /// 
        /// </summary>
        private BatchToolExecutor toolExecutor;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chatProvider"></param>
        /// <param name="eventSink"></param>
        /// <param name="cancellationToken"></param>
        public AgentLoop(IProtocolProvider chatProvider, ToolRegistry toolRegistry, BatchToolExecutor toolExecutor, EventSink eventSink)
        {
            this.chatProvider = chatProvider;
            this.eventSink = eventSink;
            this.toolRegistry = toolRegistry;
            this.toolExecutor = toolExecutor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public async Task Run(Conversation conversation, bool stream, CancellationToken cancellationToken)
        {
            try
            {
                var tools = toolRegistry.Wire();

                for (int i = 0; i < 10; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var messages = conversation.ToProviderMessages();

                    StringBuilder reply = new StringBuilder();
                    IReadOnlyList<ToolCall>? functions = null;

                    await foreach (var chunk in chatProvider.ChatStream(messages, tools, cancellationToken))
                    {
                        switch (chunk)
                        {
                            case Chunk.TextDelta(var delta):
                                reply.Append(delta);
                                eventSink.Output.Broadcast(new AssistantDeltaEvent() { Delta = delta });
                                break;
                            case Chunk.ToolCalls(var toolcalls):
                                functions = toolcalls;
                                break;
                            case Chunk.Done:
                                break;
                        }
                    }

                    var content = reply.ToString();
                    if (functions == null || functions.Count == 0)
                    {
                        conversation.AddAssistant(content);
                    }
                    else
                    {
                        conversation.AddAssistant(content, functions);
                    }

                    // 无工具调用 → Agent 完成
                    if (functions == null || functions.Count == 0)
                    {
                        //eventSink.Output.Broadcast(new AssistantCompletedEvent());
                        return;
                    }

                    await OnExcuteToolCalls(conversation, functions, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {

            }
            catch (Exception ex)
            {

            }
            finally
            {
                eventSink.Output.Broadcast(new AssistantCompletedEvent());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toolcalls"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task OnExcuteToolCalls(Conversation conversation, IReadOnlyList<ToolCall> toolcalls, CancellationToken cancellationToken)
        {
            foreach (var call in toolcalls)
            {
                eventSink.Output.Broadcast(new ToolCallEvent() { Call = call });
            }

            var results = await toolExecutor.Execute(toolcalls, cancellationToken);

            for (int i=0; i<toolcalls.Count; i++)
            {
                var call = toolcalls[i];
                var result = results[i];

                var content = result.Success ? result.Content : $"错误：{result.Error}";
                conversation.AddTool(content, call.Id);

                eventSink.Output.Broadcast(new ToolResultEvent()
                {
                    Call = call,
                    Result = result,
                });
            }
        }
    }
}
