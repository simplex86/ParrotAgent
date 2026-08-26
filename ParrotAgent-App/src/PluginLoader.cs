using System;
using System.Reflection;

namespace ParrotAgent.App
{
    internal class PluginLoader
    {
#if DEBUG
        private const string rootpath = "../bin/Debug/net8.0/";
#else
        private const string rootpath = "./";
#endif

        public bool Load()
        {
            try
            {
                Assembly.LoadFrom($"{rootpath}ParrotAgent-NUI.dll");
                Assembly.LoadFrom($"{rootpath}ParrotAgent-OpenAI.dll");
                Assembly.LoadFrom($"{rootpath}ParrotAgent-Tool.dll");
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
