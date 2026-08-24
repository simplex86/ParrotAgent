// See https://aka.ms/new-console-template for more information

using ParrotAgent.NUI;
using ParrotAgent.Tool;
using System;
using System.Text;

Console.Clear();

ToolModule.Load();

Console.OutputEncoding = Encoding.UTF8;
{
    var app = new App();
    await app.Run();
}
Console.ResetColor();