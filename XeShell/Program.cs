using Spectre.Console;
using XeSharp.Device;
using XeSharp.Helpers;
using XeSharp.Logger;
using XeSharp.Net.Sockets;
using XeShell.Commands;
using XeShell.Commands.Impl;
using XeShell.Exceptions;
using XeShell.Helpers;

namespace XeShell
{
    public class Program
    {
        private static List<string> _gracefulExitCommands =
        [
            "bye",
            "magicboot cold", // """graceful"""
            "shutdown"
        ];

        protected static XeDbgClient _client;
        protected static XeDbgConsole _console;

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
                XeLogger.Log($"\nFailed to establish a connection to \"{hostName}\"...");
                Thread.Sleep(2500); // Sleep to display error message.
                Main();
                return;
            }

            // TODO: set up command callbacks.
            Poke.History.Clear();

            Shell(true);
        }

        static void Welcome()
        {
            Console.Clear();
            Console.WriteLine($"XeShell [Version {Extensions.AssemblyExtensions.GetInformationalVersion()}]");
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
                        _client  = new XeDbgClient(in_hostName);
                        _console = new XeDbgConsole(_client, in_isFullFileSystemMap: false);

                        return true;
                    }
#if !DEBUG
                    catch
                    {
                        _client  = null;
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
                XeLogger.Log($"\nConnected to \"{_client.HostName}\".\n\n{_console.Info}");
            }

            Console.WriteLine();

            var prompt = Prompt.Show($"{_console.FileSystem.CurrentDirectory}>");

            if (prompt.IsNullOrEmptyOrWhiteSpace())
            {
                Shell();
                return;
            }

            // TODO: allow cancelling operations.
            try
            {
                // Intercept unimplemented XBDM commands with our own.
                if (!CommandProcessor.ExecuteArguments(prompt, _console))
                {
                    var response = _client.SendCommand(prompt, false);

                    if (response == null || !_client.IsConnected())
                    {
                        // TODO: make sure this works??
                        XeLogger.Error("Connection to the server has been lost...");
                    }
                    else
                    {
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
                                    throw new UnknownCommandException(prompt.Split(' ')[0]);

                                XeLogger.Error(isMessage ? response.Message : response.Status.ToString());
                            }
                            else if (isMessage)
                            {
                                XeLogger.Log(response.Message);
                            }
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
                Logger.Error($"An internal error occurred.\n{out_ex}");
            }
#endif

            // User disconnected gracefully.
            if (_gracefulExitCommands.Contains(prompt.ToLower()))
            {
                _client?.Dispose();
                Main();
                return;
            }

            Shell();
        }
    }
}
