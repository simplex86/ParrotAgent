using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class AgentLoop
    {
        /// <summary>
        /// 
        /// </summary>
        private IProtocolProvider provider;
        /// <summary>
        /// 
        /// </summary>
        private EventDispatcher eventDispatcher;
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
        private Compressor compressor;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="toolRegistry"></param>
        /// <param name="eventDispatcher"></param>
        /// <param name="compressor"></param>
        public AgentLoop(IProtocolProvider provider, ToolRegistry toolRegistry, EventDispatcher eventDispatcher, Compressor compressor)
        {
            this.provider = provider;
            this.toolRegistry = toolRegistry;
            this.eventDispatcher = eventDispatcher;
            this.compressor = compressor;

            var hitl = new PromptHitl(eventDispatcher);
            this.toolExecutor = new BatchToolExecutor(toolRegistry, hitl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public async Task Run(Conversation conversation, bool stream, CancellationToken cancellationToken)
        {
            var uPromptTokens = 0;
            var uTotalTokens = 0;

            try
            {
                var tools = toolRegistry.Wire();

                for (int i = 0; i < 30; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // 每轮 LLM 调用前，尝试压缩
                    if (compressor is not null)
                    {
                        var compression = await compressor.CheckCompressable(conversation, cancellationToken);
                        switch (compression)
                        {
                            case Compression.None: 
                                break;
                            case Compression.Warning(var message, var breakerOpen):
                                await eventDispatcher.Dispatch(new ContextWarningEvent() { Message = message });
                                break;
                            case Compression.Compress:
                                await eventDispatcher.Dispatch(new ContextCompressingEvent());
                                var result = await compressor.Compress(conversation, cancellationToken);
                                await eventDispatcher.Dispatch(new ContextCompressedEvent() {
                                    WasCompressed = result.WasCompressed,
                                    MessagesCompressed = result.MessagesCompressed,
                                    TokensSaved = result.EstimatedTokensSaved
                                });
                                break;
                            default:
                                break;
                        }
                    }

                    StringBuilder reply = new StringBuilder();
                    IReadOnlyList<ToolCall>? functions = null;

                    var messages = conversation.ToProviderMessages();
                    await foreach (var chunk in provider.ChatStream(messages, tools, cancellationToken))
                    {
                        switch (chunk)
                        {
                            case Chunk.TextDelta(var delta):
                                reply.Append(delta);
                                await eventDispatcher.Dispatch(new AssistantDeltaEvent() { Delta = delta });
                                break;
                            case Chunk.ToolCalls(var toolcalls):
                                functions = toolcalls;
                                break;
                            case Chunk.Stop(var promptTokens, var completionTokens, var totalTokens):
                                uPromptTokens = promptTokens;
                                uTotalTokens  = totalTokens;
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
                Trace.TraceError(ex.Message);
            }
            finally
            {
                await eventDispatcher.Dispatch(new AssistantCompletedEvent() { 
                    PromptTokens = uPromptTokens, 
                    TotalTokens = uTotalTokens 
                });
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
                await eventDispatcher.Dispatch(new ToolCallEvent() { Call = call });
            }

            var results = await toolExecutor.Execute(toolcalls, cancellationToken);

            string[] truncatedContents;
            IReadOnlyList<TruncationInfo> truncatedInfos = [];

            // 在工具结果入历史之前，尝试截断
            if (compressor is not null)
            {
                var (tc, ti) = compressor.TruncateBatch([.. results.Select(r => r.Success ? r.Content : string.Empty)],
                                                        [.. toolcalls.Select(c => c.Name)]);
                truncatedContents = tc;
                truncatedInfos = ti;
            }
            else
            {
                truncatedContents = [.. results.Select(r => r.Content)];
            }

            for (int i=0; i<toolcalls.Count; i++)
            {
                var call = toolcalls[i];
                var result = results[i];

                var content = result.Success ? truncatedContents[i] : $"错误：{result.Error}";
                conversation.AddTool(content, call.Id);

                await eventDispatcher.Dispatch(new ToolResultEvent() {
                    Call = call,
                    Result = result,
                });
            }
        }
    }
}
