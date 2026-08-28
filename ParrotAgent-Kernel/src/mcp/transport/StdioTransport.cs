using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParrotAgent.Kernel
{
    /// <summary>
    /// Stdio 传输：通过子进程的 stdin/stdout 通信，stderr 独立收集日志
    /// 子进程在 Connect 启动，Close 关闭
    ///
    /// 关键设计：
    /// - stdin 写入 JSON-RPC 消息（每条一行）
    /// - stdout 读取 JSON-RPC 响应（每条一行）
    /// - stderr 独立线程读取，输出到日志（不混入 JSON-RPC 通道）
    /// </summary>
    internal sealed class StdioTransport : ITransport
    {
        /// <summary>
        /// 
        /// </summary>
        private McpServerConfig config;
        private Process? process;
        private StreamReader? stdout;
        private StreamWriter? stdin;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public StdioTransport(McpServerConfig config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Task Connect(CancellationToken cancellationToken)
        {
            var (fileName, args, resolvedDir) = ResolveCommand(config.Command, config.Args ?? string.Empty);

            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args ?? string.Empty,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = config.WorkingDir ?? string.Empty,
                StandardInputEncoding = new UTF8Encoding(false), // stdin 编码必须无 BOM
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };

            // 设置环境变量
            if (config.Env is not null)
            {
                foreach (var (key, value) in config.Env)
                    startInfo.Environment[key] = value;
            }

            // 如果命令从 fallback 目录找到（不在 PATH 中），将该目录加入子进程 PATH，
            // 使 npx.cmd 等批处理能找到 node.exe 等依赖。
            if (resolvedDir is not null)
            {
                var currentPath = startInfo.Environment.TryGetValue("PATH", out var p) ? p : "";
                startInfo.Environment["PATH"] = resolvedDir + Path.PathSeparator + currentPath;
            }

            process = Process.Start(startInfo) ?? throw new InvalidOperationException($"无法启动 MCP Server：{config.Command}");
            Trace.TraceInformation($"MCP Stdio Server [{config.Name}] 已启动 (PID={process.Id})");

            stdout = process.StandardOutput;
            stdin = process.StandardInput;
            stdin.AutoFlush = true;

            // stderr 独立线程收集（不污染 JSON-RPC 通道）
            Task.Run(async () => {
                try
                {
                    while (!process.StandardError.EndOfStream)
                    {
                        var line = await process.StandardError.ReadLineAsync(cancellationToken);
                        if (line is not null) Trace.TraceInformation($"MCP Server [{config.Name}] stderr: {line}");
                    }
                }
                catch (OperationCanceledException)
                {
                
                }
                catch (Exception ex)
                {
                    Trace.TraceInformation($"MCP Server [{config.Name}] stderr 读取结束：{ex.Message}");
                }
            }, cancellationToken);

            return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task Send(string json, CancellationToken cancellationToken)
        {
            if (stdin is null) throw new InvalidOperationException("Transport 未连接");
            await stdin.WriteLineAsync(json.AsMemory(), cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<string?> Receive(CancellationToken cancellationToken)
        {
            if (stdout is null) 
                throw new InvalidOperationException("Transport 未连接");

            try
            {
                return await stdout.ReadLineAsync(cancellationToken);
            }
            catch (IOException)
            {
                return null;  // 进程已退出
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Close(CancellationToken cancellationToken)
        {
            if (process is null || process.HasExited) return Task.CompletedTask;

            Trace.TraceInformation($"MCP Stdio server [{config.Name}] 正在关闭");

            // 关闭 stdin 触发 server 优雅退出
            try { stdin?.Close(); } catch { }

            // 等待进程退出（最多 3 秒）
            if (!process.WaitForExit(3000))
            {
                Trace.TraceWarning($"MCP server [{config.Name}] 未在 3 秒内退出，强制终止");
                try { process.Kill(entireProcessTree: true); } catch { }
            }

            Trace.TraceInformation($"MCP Stdio server [{config.Name}] 已关闭 (exit={process.ExitCode})");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 解析命令路径。Windows 上 npx/node 等实际是 .cmd 批处理文件，
        /// UseShellExecute=false 时 CreateProcess 不能直接执行 .cmd/.bat，
        /// 需通过 cmd.exe /c 包装，并使用完整路径。
        /// 返回 (fileName, args, resolvedDir)：resolvedDir 为命令所在目录（fallback 找到时需加入子进程 PATH）。
        /// Unix 上直接返回原命令（依赖 PATH 查找）。
        /// </summary>
        private static (string fileName, string args, string? resolvedDir) ResolveCommand(string? command, string args)
        {
            if (string.IsNullOrWhiteSpace(command)) return (command ?? string.Empty, args, null);

            // Unix 直接返回（execvp 负责 PATH 查找）
            if (!OperatingSystem.IsWindows()) return (command, args, null);

            // 已含扩展名：检查是否为 .cmd/.bat
            if (Path.HasExtension(command))
            {
                var ext = Path.GetExtension(command).ToLowerInvariant();
                if (ext == ".cmd" || ext == ".bat")
                {
                    var (fullpath, dir) = FindExecutable(command);
                    return (fullpath is not null) ? ("cmd.exe", $"/c \"{fullpath}\" {args}", dir)
                                                  : ("cmd.exe", $"/c \"{command}\" {args}", null);
                }
                return (command, args, null);
            }

            // 无扩展名：尝试 .cmd / .bat / .exe
            foreach (var ext in new[] { ".cmd", ".bat", ".exe" })
            {
                var withExt = command + ext;
                var (fullpath, dir) = FindExecutable(withExt);
                if (fullpath is not null)
                {
                    return (ext is ".cmd" or ".bat") ? ("cmd.exe", $"/c \"{fullpath}\" {args}", dir)
                                                     : (fullpath, args, dir);
                }
            }

            // 找不到则返回原命令，让 Process.Start 抛出原始异常
            return (command, args, null);  
        }

        /// <summary>
        /// 在 PATH 和常见安装目录中查找可执行文件
        /// 返回 (完整路径, 所在目录)
        /// 目录在 fallback 查找时非 null，PATH 查找时为 null（已在 PATH 中）
        /// </summary>
        private static (string? fullpath, string? dir) FindExecutable(string fileName)
        {
            // 如果已经是完整路径，直接检查
            if (Path.IsPathRooted(fileName))
                return (File.Exists(fileName) ? fileName : null, null);

            // 1. 在 PATH 中查找（返回 dir=null 表示已在 PATH 中，无需额外处理）
            var path = Environment.GetEnvironmentVariable("PATH");
            if (path is not null)
            {
                foreach (var dir in path.Split(Path.PathSeparator))
                {
                    if (string.IsNullOrWhiteSpace(dir)) continue;
                    try
                    {
                        var full = Path.Combine(dir, fileName);
                        if (File.Exists(full)) return (full, null);
                    }
                    catch { }
                }
            }

            // 2. 在常见 Node.js 安装目录中查找（fallback）
            // Windows 上 Node.js 可能安装但未加入进程 PATH（终端在安装前已打开等）
            var fallbacks = new[] {
                @"C:\Program Files\nodejs",
                @"C:\Program Files (x86)\nodejs",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "nodejs"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npm"),
            };

            foreach (var dir in fallbacks)
            {
                try
                {
                    var full = Path.Combine(dir, fileName);
                    if (File.Exists(full)) return (full, dir);
                }
                catch 
                { 
                }
            }

            return (null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ValueTask DisposeAsync()
        {
            try { process?.Dispose(); } catch { }
            return ValueTask.CompletedTask;
        }
    }
}
