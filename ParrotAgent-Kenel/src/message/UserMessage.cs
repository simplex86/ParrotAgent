using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 用户消息
    /// </summary>
    /// <param name="Content"></param>
    internal record UserMessage(string content) : IMessage
    {
        public MessageRole Role { get; } = MessageRole.User;
        public string Content { get; } = content;
        public IReadOnlyList<ToolCall>? ToolCalls { get; init; }
    }
}
