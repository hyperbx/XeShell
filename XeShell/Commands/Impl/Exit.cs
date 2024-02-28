using XeSharp.Device;

namespace XeShell.Commands.Impl
{
    [Command("exit")]
    public class Exit : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console)
        {
            in_console.Client.Disconnect();
            Console.Clear();
            Environment.Exit(0);
        }

        public ECommandResponse ExecuteRaw(string[] in_args, string in_command, XeConsole in_console)
        {
            return ECommandResponse.Error;
        }

        public void Dispose() { }
    }
}
