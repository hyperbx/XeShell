using XeSharp.Device;
using XeShell.Services;

namespace XeShell.Commands.Impl
{
    [Command("xbdm", Inputs = [ typeof(string) ])]
    public class XBDM : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console)
        {
            new CommandService(in_console).SendCommand(string.Join(' ', in_command.Inputs), false);
        }

        public bool ExecuteRaw(string[] in_args, XeConsole in_console)
        {
            return false;
        }

        public void Dispose() { }
    }
}
