using XeSharp.Device;

namespace XeShell.Commands
{
    public interface ICommand
    {
        void Execute(List<Command> in_commands, Command in_command, XeDbgConsole in_console);
    }
}
