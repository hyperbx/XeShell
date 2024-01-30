using XeSharp.Device;

namespace XeShell.Commands.Impl
{
    [Command("clear", "cls")]
    public class ClearScreen : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeDbgConsole in_console)
        {
            Console.Clear();
        }
    }
}
