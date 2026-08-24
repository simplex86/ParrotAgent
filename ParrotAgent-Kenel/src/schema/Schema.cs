using System.Linq;
using System.Text.Json;

namespace ParrotAgent.Kenel
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Schema
    {
        /// <summary>
        /// 
        /// </summary>
        private static IProtocolSchema schema;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public static void Init(ProviderConfig config)
        {
            switch (config.Protocol)
            {
                case "openai":
                    schema = new OpenAISchema();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static object Wire(this IMessage message)
        {
            return schema.Wire(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="registry"></param>
        /// <returns></returns>
        public static JsonElement? Wire(this ToolRegistry registry)
        {
            var tools = registry.GetAll();
            return tools.Count > 0 ? JsonSerializer.SerializeToElement(tools.Select(t => schema.Wire(t)).ToArray())
                                   : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static JsonElement ToJson(this string text)
        {
            return JsonSerializer.Deserialize<JsonElement>(text);
        }
    }
}
