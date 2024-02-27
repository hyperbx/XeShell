using XeSharp.Debug;
using XeSharp.Device;
using XeSharp.Logger;

namespace XeShell.Commands.Impl
{
    [Command("detach")]
    public class Detach : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console)
        {
            XeDebugger.Detach(in_console);
            XeLogger.Log($"Debugger detached.");
        }

        public bool ExecuteRaw(string[] in_args, string in_command, XeConsole in_console)
        {
            return false;
        }

        public void Dispose() { }
    }
}
