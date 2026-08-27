using System;
using System.IO;
using System.Reflection;

namespace ParrotAgent.App
{
    /// <summary>
    /// 插件加载器
    /// </summary>
    internal class PluginLoader
    {
        /// <summary>
        /// 加载所有插件
        /// </summary>
        /// <returns></returns>
        public bool Load()
        {
            try
            {
                var lines = File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, ".plugins.manifest"));
                foreach (var line in lines)
                {
                    var filename = Path.Combine(AppContext.BaseDirectory, line);
                    Assembly.LoadFrom(filename);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Load Plugin Failed: {ex.Message}");
                Console.WriteLine($"{ex.StackTrace}");
                Console.ResetColor();

                return false;
            }

            return true;
        }
    }
}
