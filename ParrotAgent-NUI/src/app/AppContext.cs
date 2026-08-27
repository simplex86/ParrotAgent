using System;
using System.Threading;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    internal static class AppContext
    {
        /// <summary>
        /// 
        /// </summary>
        public static CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
    }
}
