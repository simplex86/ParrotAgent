using System;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// 配置加载/校验异常。
    /// 携带来源路径与（YAML 语法错误时的）行号/列号，供 Program 打印友好的定位信息。
    /// </summary>
    public sealed class AgentConfigException : Exception
    {
        public string? SourcePath { get; }
        public int? Line { get; }
        public int? Column { get; }

        public AgentConfigException(string message, string? sourcePath = null, int? line = null, int? column = null, Exception? inner = null)
            : base(message, inner)
        {
            SourcePath = sourcePath;
            Line = line;
            Column = column;
        }
    }
}
