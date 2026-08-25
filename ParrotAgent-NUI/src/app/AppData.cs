using System;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.NUI
{
    /// <summary>
    /// 
    /// </summary>
    internal static class AppData
    {
        /// <summary>
        /// 
        /// </summary>
        public static CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
    }
}
