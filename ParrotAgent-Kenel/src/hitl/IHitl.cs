using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 
    /// </summary>
    public enum HitlOption
    {
        /// <summary>
        /// 允许
        /// </summary>
        Allow,
        /// <summary>
        /// 拒绝
        /// </summary>
        Deny
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Option"></param>
    /// <param name="Reason"></param>
    public sealed record HitlResult(HitlOption Option, string? Reason)
    {
        /// <summary>
        /// 允许
        /// </summary>
        public static HitlResult Allow() => new(HitlOption.Allow, null);

        /// <summary>
        /// 拒绝
        /// </summary>
        public static HitlResult Deny(string reason) => new(HitlOption.Deny, reason);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IHitl
    {
        Task<HitlResult> Request(ToolCall call, CancellationToken cancellationToken);
    }
}
