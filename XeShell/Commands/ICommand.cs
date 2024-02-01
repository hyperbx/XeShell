using XeSharp.Device;

namespace XeShell.Commands
{
    public interface ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeDbgConsole in_console);
        public bool ExecuteRaw(string[] in_args, XeDbgConsole in_console);
    }
}
