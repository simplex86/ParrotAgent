using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// 
    /// </summary>
    internal class PromptHitl : IHitl
    {
        /// <summary>
        /// 
        /// </summary>
        private EventDispatcher eventDispatcher;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventDispatcher"></param>
        public PromptHitl(EventDispatcher eventDispatcher)
        {
            this.eventDispatcher = eventDispatcher;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toolcall"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<HitlResult> Request(ToolCall toolcall, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return HitlResult.Deny("已取消");
            }

            var taskCompletionSource = new TaskCompletionSource<HitlOption>();
            await eventDispatcher.Dispatch(new HitlEvent() {
                Call = toolcall,
                TaskCompletionSource = taskCompletionSource 
            });

            var option = await taskCompletionSource.Task;

            if (option == HitlOption.Allow)
            {
                return HitlResult.Allow();
            }
            
            return HitlResult.Deny("用户拒绝");
        }
    }
}
