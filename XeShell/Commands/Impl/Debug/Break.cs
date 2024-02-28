using XeSharp.Debug;
using XeSharp.Device;
using XeSharp.Logger;

namespace XeShell.Commands.Impl
{
    [Command("break")]
    public class Break : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console) { }

        public ECommandResponse ExecuteRaw(string[] in_args, string in_command, XeConsole in_console)
        {
            if (in_args.Length <= 0 || in_args[0].ToLower() != "break")
                return ECommandResponse.Error;

            if (in_args.Length > 1 && in_args[1].ToLower() == "clearall")
            {
                XeDebugger.Detach(in_console);
                XeLogger.Log($"Debugger detached.");

                return ECommandResponse.RerouteToBase;
            }

            XeDebugger.Attach(in_console);
            XeLogger.Log($"Debugger attached.");

            return ECommandResponse.RerouteToBase;
        }

        public void Dispose() { }
    }
}
