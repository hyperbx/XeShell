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
        protected static XeDbgConsole _console;

#if !DEBUG
        private const string _programName = "XeShell";
#else
        private const string _programName = "XeShell Debug";
#endif

        private static List<string> _gracefulExitCommands =
        [
            "bye",
            "magicboot cold", // """graceful"""
            "shutdown"
        ];

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
                        _console = new XeDbgConsole(in_hostName, in_isFullFileSystemMap: false);
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

            var prompt   = new PromptService();
            var autofill = new AutofillService(_console, prompt);

            var input = prompt.Show($"{_console.FileSystem.CurrentDirectory}>");

            if (input.IsNullOrEmptyOrWhiteSpace())
            {
                Shell();
                return;
            }

            try
            {
                Console.CancelKeyPress += Console_CancelKeyPress;

                if (!_console.Client.Ping())
                {
                    Welcome();
                    Error("\nConnection to the server has been lost...");
                    return;
                }

                // Intercept unimplemented XBDM commands with our own.
                if (!CommandProcessor.ExecuteArguments(input, _console))
                {
                    var response = _console.Client.SendCommand(input, false)
                        ?? throw new HttpIOException(HttpRequestError.InvalidResponse, "The server response returned null.");

                    if (response.Results?.Length > 0)
                    {
                        foreach (var result in response.Results)
                            XeLogger.Log(result);
                    }
                    else
                    {
                        bool isMessage = !string.IsNullOrEmpty(response.Message);

                        if (response.Status.IsFailed())
                        {
                            if (response.Status.ToHResult() == XeSharp.Net.EXeDbgStatusCode.XBDM_INVALIDCMD)
                                throw new UnknownCommandException(input.Split(' ')[0]);

                            XeLogger.Error(isMessage ? response.Message : response.Status.ToString());
                        }
                        else if (isMessage)
                        {
                            XeLogger.Log(response.Message);
                        }
                    }
                }
            }
            catch (UnknownCommandException out_ex)
            {
                XeLogger.Error(out_ex.Message);
            }
#if !DEBUG
            catch (Exception out_ex)
            {
                XeLogger.Error($"An internal error occurred.\n{out_ex}");
            }
#endif
            finally
            {
                Console.CancelKeyPress -= Console_CancelKeyPress;
            }

            // User disconnected gracefully.
            if (_gracefulExitCommands.Contains(input.ToLower()))
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
