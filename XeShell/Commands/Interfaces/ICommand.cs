using XeSharp.Device;

namespace XeShell.Commands
{
    public interface ICommand : IDisposable
    {
        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console);
        public ECommandResponse ExecuteRaw(string[] in_args, string in_command, XeConsole in_console);
    }
}
