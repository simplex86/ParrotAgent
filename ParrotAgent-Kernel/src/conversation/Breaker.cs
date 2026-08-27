using System;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// 通用熔断器：连续失败 maxFailures 次后打开，停止自动触发
    /// 成功或手动 Reset 后关闭
    /// 非线程安全（AgentLoop 单线程驱动）
    /// </summary>
    internal sealed class Breaker
    {
        private readonly int maxFailures;
        private int failureCount;
        private bool isOpen;

        public Breaker(int maxFailures = 2)
        {
            if (maxFailures < 1) throw new ArgumentOutOfRangeException(nameof(maxFailures));
            this.maxFailures = maxFailures;
        }

        public bool IsOpen => isOpen;
        public int FailureCount => failureCount;
        public int MaxFailures => maxFailures;

        /// <summary>
        /// 记录一次失败
        /// 达到阈值时打开熔断器
        /// </summary>
        public void RecordFailure()
        {
            failureCount++;
            if (failureCount >= maxFailures) isOpen = true;
        }

        /// <summary>
        /// 记录一次成功
        /// 清零计数，关闭熔断器
        /// </summary>
        public void RecordSuccess()
        {
            failureCount = 0;
            isOpen = false;
        }

        /// <summary>
        /// 手动重置（如 /compress 命令或程序重启）
        /// </summary>
        public void Reset()
        {
            failureCount = 0;
            isOpen = false;
        }
    }
}
