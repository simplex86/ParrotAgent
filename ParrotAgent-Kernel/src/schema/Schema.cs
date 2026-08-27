using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace ParrotAgent.Kernel
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
            var types = Reflection.FindAll<IProtocolSchema, ProtocalSchemaAttribute>();
            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<ProtocalSchemaAttribute>();
                if (attr?.Name == config.Protocol)
                {
                    schema = Reflection.CreateInstance<IProtocolSchema>(type);
                    return;
                }
            }
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
