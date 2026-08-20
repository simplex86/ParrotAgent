using System;
using System.Linq;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 
    /// </summary>
    internal class ProviderFactory
    {
        /// <summary>
        /// 按 active_provider（回退 providers[0]）选中并创建
        /// </summary>
        public static IProtocolProvider CreateActive(AgentConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);
            if (config.Providers.Count == 0)
                throw new AgentConfigException("providers 不能为空");

            var name = config.ActiveProvider ?? config.Providers[0].Name;
            var pc = config.Providers.FirstOrDefault(p => p.Name == name)
                ?? throw new AgentConfigException($"active_provider '{name}' 未在 providers 中定义");

            return Create(pc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static IProtocolProvider Create(ProviderConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);
            return config.Protocol switch
            {
                "mock" => new MockProvider(),
                "openai" => new OpenAIProvider(config),
                "anthropic" => throw new ProviderNotImplementedException(config),
                _ => throw new ArgumentException($"不支持的协议: {config.Protocol} (provider={config.Name})")
            };
        }
    }

    /// <summary>
    /// 协议已识别但尚未实现（openai/anthropic）。迭代 3 接入真实 LLM 后消除。
    /// </summary>
    public sealed class ProviderNotImplementedException : NotSupportedException
    {
        public ProviderNotImplementedException(ProviderConfig config)
            : base($"Provider '{config.Name}' (protocol={config.Protocol}) 将在后续迭代实现，本迭代支持 mock/openai。")
        {

        }
    }
}
