using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ParrotAgent.Kenel;

namespace ParrotAgent.Protocol
{
    namespace OpenAI
    {
        /// <summary>
        /// 
        /// </summary>
        [ProtocalSchema("openai")]
        public class OpenAISchema : IProtocolSchema
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="message"></param>
            /// <returns></returns>
            public object Wire(IMessage message)
            {
                return message.Wire();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="tool"></param>
            /// <returns></returns>
            public JsonElement Wire(ITool tool)
            {
                var schema = new
                {
                    type = "function",
                    function = new
                    {
                        name = tool.Name,
                        description = tool.Description,
                        parameters = OpenAISchemaHelper.Wire(tool.Parameters)
                    }
                };
                return JsonSerializer.SerializeToElement(schema);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal static class OpenAISchemaEx
        {
            public static object Wire(this IMessage message)
            {
                if (message.Role == MessageRole.Assistant &&
                    message.ToolCalls is { Count: > 0 })
                {
                    return new
                    {
                        role = OpenAISchemaHelper.Wire(message.Role),
                        content = string.IsNullOrEmpty(message.Content) ? null : message.Content,
                        tool_calls = message.ToolCalls.Select(tc => new
                        {
                            id = tc.Id,
                            type = "function",
                            function = new
                            {
                                name = tc.Name,
                                arguments = tc.Args
                            }
                        }).ToArray()
                    };
                }

                if (message.Role == MessageRole.Tool &&
                    message.ToolCallId is not null)
                {
                    return new
                    {
                        role = OpenAISchemaHelper.Wire(message.Role),
                        content = message.Content,
                        tool_call_id = message.ToolCallId
                    };
                }

                return new
                {
                    role = OpenAISchemaHelper.Wire(message.Role),
                    content = message.Content
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal static class OpenAISchemaHelper
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="role"></param>
            /// <returns></returns>
            internal static string Wire(MessageRole role) => role switch
            {
                MessageRole.System => "system",
                MessageRole.User => "user",
                MessageRole.Assistant => "assistant",
                MessageRole.Tool => "tool",
                _ => "user"  // 未知角色兜底为 user，不抛异常（容错优先）
            };



            /// <summary>
            /// 基于 Parameters 列表构造 JSON Schema 的 parameters / input_schema 对象。
            /// {"type":"object","properties":{name:{"type":...,"description":...}},"required":[...]}
            /// </summary>
            internal static JsonElement Wire(IReadOnlyList<ToolParameter> parameters)
            {
                // 匿名对象 + Dictionary 混合：properties 是动态键的对象，无法用强类型匿名表达。
                // 用 Dictionary<string, object> 让 JsonSerializer 输出为 JSON 对象。
                var properties = new Dictionary<string, object>();
                foreach (var p in parameters)
                {
                    properties[p.Name] = new { type = p.Type, description = p.Description };
                }
                var required = parameters.Where(p => p.Required).Select(p => p.Name).ToArray();

                var schema = new
                {
                    type = "object",
                    properties,
                    required
                };

                return JsonSerializer.SerializeToElement(schema);
            }
        }
    }
}
