using System;
using System.Reflection;

namespace ParrotAgent.App
{
    /// <summary>
    /// 插件加载器
    /// </summary>
    internal class PluginLoader
    {
#if DEBUG
        private const string rootpath = "../bin/Debug/net8.0/";
#else
        private const string rootpath = "./";
#endif

        /// <summary>
        /// 加载所有插件
        /// </summary>
        /// <returns></returns>
        public bool Load()
        {
            return Load($"ParrotAgent-OpenAI.dll", out var _) &&
                   Load($"ParrotAgent-Tool.dll",   out var _) &&
                   Load($"ParrotAgent-NUI.dll",    out var _) ;
        }

        /// <summary>
        /// 加载指定的插件
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private bool Load(string filename, out Assembly? assembly)
        {

            try
            {
                assembly = Assembly.LoadFrom($"{rootpath}{filename}");
            }
            catch (Exception ex)
            {
                assembly = null;

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
