using Spectre.Console;
using XeSharp.Device;
using XeSharp.Helpers;
using XeSharp.Logger;
using XeShell.Commands;
using XeShell.Exceptions;
using XeShell.Helpers;
using XeShell.Services;

namespace XeShell
{
    public class Program
    {
        protected static XeConsole _console;

#if !DEBUG
        private const string _programName = "XeShell";
#else
        private const string _programName = "XeShell Debug";
#endif

        static void Main(string[] in_args = null)
        {
            Welcome();

            string hostName;

            if (in_args == null || in_args.Length <= 0)
            {
                Console.WriteLine();
                hostName = AnsiConsole.Prompt(new TextPrompt<string>("Host name or IP address:"));
            }
            else
            {
                hostName = in_args[0];
            }

            if (!Connect(hostName))
            {
                Error($"\nFailed to establish a connection to \"{hostName}\"...");
                return;
            }

            PromptService.History.Clear();
            CommandProcessor.DisposeCommands();

            Shell(true);
        }

        static void Error(string in_message, int in_timeout = 2500)
        {
            XeLogger.Error(in_message);
            Thread.Sleep(in_timeout);
            Main();
        }

        static void Welcome()
        {
            Console.Clear();
            Console.WriteLine($"{_programName} [Version {Extensions.AssemblyExtensions.GetInformationalVersion()}]");
        }

        static bool Connect(string in_hostName)
        {
            Welcome();

            return ConsoleHelper.StatusCommon($"Connecting to \"{in_hostName}\"...",
            //
                ctx =>
                {
#if !DEBUG
                    try
#endif
                    {
                        _console = new XeConsole(in_hostName, in_isFullFileSystemMap: false);
                        return true;
                    }
#if !DEBUG
                    catch
                    {
                        _console = null;
                        return false;
                    }
#endif
                }
            );
        }

        static void Shell(bool in_isInitial = false)
        {
            if (in_isInitial)
            {
                Welcome();
                XeLogger.Log($"\nConnected to \"{_console.Client.HostName}\".\n\n{_console.Info}");
            }

            Console.WriteLine();

            if (!_console.Client.Ping())
            {
                Welcome();
                Error("\nConnection to the server has been lost...");
                return;
            }

            var response = new CommandService(_console).PromptCommand($"{_console.FileSystem.CurrentDirectory}>");

            if (response == EPromptResponse.ExitCallback)
            {
                _console.Client?.Dispose();
                Main();
                return;
            }

            Shell();
        }

        private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _console.Client.Cancel();
        }
    }
}
