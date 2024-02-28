using XeSharp.Debug;
using XeSharp.Device;
using XeSharp.Logger;

namespace XeShell.Commands.Impl
{
    [Command("xcpu", OptionalInputs = [ typeof(int) ])]
    public class XCPU : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console)
        {
            var threadID = in_command.Inputs.Count > 0
                ? (int)in_command.Inputs[0]
                : XeConsole.MainThreadID;

            XeLogger.Log(new XeDebugger(in_console).GetProcessor(threadID).GetRegisterInfo());
        }

        public ECommandResponse ExecuteRaw(string[] in_args, string in_command, XeConsole in_console)
        {
            return ECommandResponse.Error;
        }

        public void Dispose() { }
    }
}
