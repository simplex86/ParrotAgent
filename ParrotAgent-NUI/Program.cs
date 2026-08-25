// See https://aka.ms/new-console-template for more information

using System;
using System.Text;
using ParrotAgent.NUI;
using ParrotAgent.Protocol;
using ParrotAgent.Tool;

Console.Clear();

OpenAIModel.Load();
ToolModule.Load();

Console.OutputEncoding = Encoding.UTF8;
{
    var app = new App();
    await app.Run();
}
Console.ResetColor();