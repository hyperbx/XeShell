using Spectre.Console;
using XeSharp.Device;
using XeSharp.Net.Sockets;
using XeShell.Commands;
using XeShell.Commands.Impl;
using XeShell.Exceptions;

namespace XeShell
{
    internal class Program
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
                Console.WriteLine($"\nFailed to establish a connection to \"{hostName}\"...");
                Thread.Sleep(2500); // Sleep to display error message.
                Main();
                return;
            }

            // TODO: use message processor in ICommand for handling static members?
            Poke.History.Clear();

            Prompt(true);
        }

        static void Welcome()
        {
            Console.Clear();
            Console.WriteLine($"XeShell [Version {Extensions.AssemblyExtensions.GetInformationalVersion()}]");
        }

        static bool Connect(string in_hostName)
        {
            Welcome();

            return AnsiConsole.Status().Start($"Connecting to \"{in_hostName}\"...",
            //
                ctx =>
                {
                    ctx.Spinner(Spinner.Known.Line);

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
                        return false;
                    }
#endif
                }
            );
        }

        static void Prompt(bool in_isInitial = false)
        {
            if (in_isInitial)
            {
                Welcome();
                Console.WriteLine($"\nConnected to \"{_client.HostName}\".\n\n{_console.Info}\n");
            }

            /* TODO: use Stack<T> and restore previous prompts on arrow
               keys (might require own implementation of the prompt). */
            var prompt = AnsiConsole.Prompt(new TextPrompt<string>($"{_console.FileSystem.CurrentDirectory}>"));

            try
            {
                // Intercept unimplemented XBDM commands with our own.
                if (!CommandProcessor.ExecuteArguments(prompt, _console))
                {
                    var response = _client.SendCommand(prompt, false);

                    if (response == null || !_client.IsConnected())
                    {
                        Console.WriteLine("Connection to the server has been lost...");
                    }
                    else
                    {
                        if (response.Results?.Length > 0)
                        {
                            foreach (var result in response.Results)
                                Console.WriteLine(result);
                        }
                        else
                        {
                            bool isMessage = !string.IsNullOrEmpty(response.Message);

                            if (response.Status.IsFailed())
                            {
                                if (response.Status.ToHResult() == XeSharp.Net.EXeDbgStatusCode.XBDM_INVALIDCMD)
                                    throw new UnknownCommandException(prompt.Split(' ')[0]);

                                Console.WriteLine(isMessage ? response.Message : response.Status.ToString());
                            }
                            else if (isMessage)
                            {
                                Console.WriteLine(response.Message);
                            }
                        }
                    }
                }
            }
            catch (UnknownCommandException out_ex)
            {
                Console.WriteLine(out_ex.Message);
            }

            // User disconnected gracefully.
            if (_gracefulExitCommands.Contains(prompt.ToLower()))
            {
                _client?.Dispose();
                Main();
                return;
            }

            Console.WriteLine();

            Prompt();
        }
    }
}
