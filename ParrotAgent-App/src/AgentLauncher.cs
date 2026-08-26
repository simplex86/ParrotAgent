using System;
using System.Threading.Tasks;
using ParrotAgent.Kenel;

namespace ParrotAgent.App
{
    /// <summary>
    /// 
    /// </summary>
    internal class AgentLauncher
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task Launch()
        {
            try
            {
                Console.Clear();

                var pluginLoader = new PluginLoader();
                pluginLoader.Load();

                var types = Reflection.FindAll<IAgentApp, AgentAppAttribute>();
                if (types.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Launch Failed: Can not find the AgentApp entry");
                    Console.ResetColor();

                    return;
                }

                var app = Reflection.CreateInstance<IAgentApp>(types[0]);
                await app.Run();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Launch Failed: {ex.Message}");
                Console.WriteLine($"{ex.StackTrace}");
                Console.ResetColor();
            }
        }
    }
}
