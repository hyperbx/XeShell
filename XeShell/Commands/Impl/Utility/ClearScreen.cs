using XeSharp.Device;

namespace XeShell.Commands.Impl
{
    [Command("cls", "clear")]
    public class ClearScreen : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console)
        {
            Console.Clear();
        }

        public bool ExecuteRaw(string[] in_args, string in_command, XeConsole in_console)
        {
            return false;
        }

        public void Dispose() { }
    }
}
