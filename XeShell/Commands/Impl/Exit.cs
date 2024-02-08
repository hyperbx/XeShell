using XeSharp.Device;

namespace XeShell.Commands.Impl
{
    [Command("exit")]
    public class Exit : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeDbgConsole in_console)
        {
            in_console.Client.Disconnect();
            Console.Clear();
            Environment.Exit(0);
        }

        public bool ExecuteRaw(string[] in_args, XeDbgConsole in_console)
        {
            return false;
        }
    }
}
