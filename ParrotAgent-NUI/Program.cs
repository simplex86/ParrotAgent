// See https://aka.ms/new-console-template for more information

using System.Text;
using ParrotAgent.NUI;

Console.OutputEncoding = Encoding.UTF8;

var app = new App();
await app.Run();

Console.ResetColor();