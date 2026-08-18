using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 顶层配置
    /// 对应 .parrotagent.yaml 的根结构
    /// </summary>
    public sealed record AgentConfig
    {
        /// <summary>
        /// 当前激活的 Provider 名称
        /// 为 null 时回退到 providers[0].name
        /// </summary>
        public string? ActiveProvider { get; init; }

        /// <summary>
        /// Provider 列表
        /// 无配置文件时由 Loader 提供默认 mock 项
        /// </summary>
        public IList<ProviderConfig> Providers { get; init; } = Array.Empty<ProviderConfig>();
    }

    /// <summary>
    /// Provider 配置
    /// </summary>
    public sealed record ProviderConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; init; } = string.Empty;
        /// <summary>
        /// 
        /// mock | openai | anthropic
        /// </summary>
        public string Protocol { get; init; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string Model { get; init; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string BaseUrl { get; init; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string ApiKey { get; init; } = string.Empty;
    }
}
