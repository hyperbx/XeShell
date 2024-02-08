using XeSharp.Device;

namespace XeShell.Commands.Impl
{
    [Command("cls", "clear")]
    public class ClearScreen : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeDbgConsole in_console)
        {
            Console.Clear();
        }

        public bool ExecuteRaw(string[] in_args, XeDbgConsole in_console)
        {
            return false;
        }
    }
}
