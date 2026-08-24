using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class ToolExecutor
    {
        private readonly ToolRegistry registry;
        private readonly TimeSpan timeout;
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 构造执行器。
        /// timeout：单次工具执行的最大时长，默认 30 秒。超时不杀工具任务（除非工具响应取消令牌），
        /// 只是不再等待结果——返回 ToolResult.Fail("工具执行超时")。
        /// </summary>
        public ToolExecutor(ToolRegistry registry, TimeSpan? timeout = null)
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
            this.timeout = timeout ?? DefaultTimeout;
        }

        /// <summary>
        /// 执行单个 ToolCall。
        /// 流程：查找工具 → 构造取消令牌 → 执行（带超时）→ 异常捕获 → 返回 ToolResult。
        /// 任何异常（包括超时、IO、权限、参数错误）都转为 ToolResult.Fail，不抛异常。
        /// 唯一例外：外部 cancellationToken 取消时透传 OperationCanceledException。
        /// </summary>
        public async Task<ToolResult> Execute(ToolCall call, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(call);
            // 外部取消优先——已取消的 token 立即抛 OCE，不进入工具查找 / 执行
            cancellationToken.ThrowIfCancellationRequested();

            // 1. 查找工具
            var tool = registry.Get(call.Name);
            if (tool is null)
                return ToolResult.Fail($"未注册工具：{call.Name}");

            // 2. 构造带超时的取消令牌：外部的 cancellationToken + 内部的超时取并集
            using var timeoutCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCancellationTokenSource.CancelAfter(timeout);

            var sw = Stopwatch.StartNew();

            // 3. 执行：把工具执行放到 Task.Run，避免工具同步阻塞调用线程
            Task<ToolResult> executeTask;
            try
            {
                var args = call.Args.ToJson();
                executeTask = Task.Run(() => tool.Execute(args, timeoutCancellationTokenSource.Token), timeoutCancellationTokenSource.Token);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // 外部取消（用户 Ctrl+C）——透传
                throw;
            }
            catch (Exception ex)
            {
                // Task.Run 同步抛（如工具 ExecuteAsync 内部 sync throw before first await）
                //_logger?.LogWarning(ex, "工具 {Name} 启动失败", call.Name);
                return ToolResult.Fail($"工具 {call.Name} 启动失败：{ex.Message}");
            }

            // 4. 等待工具完成或超时（Task.Delay 用外部 ct，外部取消时立即结束等待）
            var delayTask = Task.Delay(timeout, cancellationToken);
            var completed = await Task.WhenAny(executeTask, delayTask);

            // 优先检查外部取消：即使 delayTask 先完成（被 ct 取消），若是外部取消则透传 OCE
            // 这避免"外部取消 + delay 先完成"被误判为超时
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(cancellationToken);

            if (completed == executeTask)
            {
                // 工具任务完成（成功或抛异常）
                try
                {
                    var result = await executeTask;
                    //_logger?.LogInformation("工具 {Name} 执行完成，耗时 {Ms}ms，成功={Success}", call.Name, sw.ElapsedMilliseconds, result.Success);
                    return result;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw; // 外部取消透传
                }
                catch (OperationCanceledException)
                {
                    // 超时触发的取消（timeoutCts 已取消但外部 ct 未取消）
                    //_logger?.LogWarning("工具 {Name} 执行超时（{Timeout}s）", call.Name, _timeout.TotalSeconds);
                    return ToolResult.Fail($"工具 {call.Name} 执行超时（{timeout.TotalSeconds}s）");
                }
                catch (Exception ex)
                {
                    //_logger?.LogWarning(ex, "工具 {Name} 执行抛异常", call.Name);
                    return ToolResult.Fail($"工具 {call.Name} 执行失败：{ex.Message}");
                }
            }

            // Task.Delay 先完成且外部未取消——纯超时
            //_logger?.LogWarning("工具 {Name} 执行超时（{Timeout}s）", call.Name, _timeout.TotalSeconds);
            return ToolResult.Fail($"工具 {call.Name} 执行超时（{timeout.TotalSeconds}s）");
        }
    }
}
