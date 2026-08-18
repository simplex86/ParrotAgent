using System;
using System.Threading;
using System.Threading.Tasks;
using ParrotAgent.Kenel;

namespace ParrotAgent.TUI
{
    /// <summary>
    /// 
    /// </summary>
    internal class App
    {
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// 
        /// </summary>
        public App() 
        {
            cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <returns></returns>
        public async Task RunAsync()
        {
            var entry = new AgentEntry(cancellationTokenSource.Token);
            var sink = await entry.Run();

            sink.Output.Add(OnSinkOutputHandler);

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                var input = Console.ReadLine();
                if (input == null ||
                    input == "/exit" ||
                    input == "/quit")
                {
                    break;
                }

                sink.Input.Write(input);
            }
        }

        /// <summary>
        /// 打印输出
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private async Task OnSinkOutputHandler(string text)
        {
            Console.WriteLine(text);
            await Task.CompletedTask;
        }
    }
}
