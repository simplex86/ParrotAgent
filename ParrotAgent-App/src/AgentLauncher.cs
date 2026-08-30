using System;
using System.Threading.Tasks;
using ParrotAgent.Kernel;

namespace ParrotAgent.App
{
    /// <summary>
    /// 启动器
    /// </summary>
    internal class AgentLauncher
    {
        /// <summary>
        /// 启动
        /// </summary>
        /// <returns></returns>
        public async Task Launch()
        {
            try
            {
                Console.Clear();
                Console.Title = "Parrot Agent v0.7.25";

                var pluginLoader = new PluginLoader();
                if (!pluginLoader.Load()) return;

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
