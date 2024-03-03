using XeSharp.Debug;
using XeSharp.Debug.Processor;
using XeSharp.Device;
using XeSharp.Logger;

namespace XeShell.Commands.Impl
{
    [Command("xcpu", OptionalInputs = [ typeof(string), typeof(int) ])]
    public class XCPU : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console)
        {
            var registers = in_command.Inputs.Count > 0
                ? (string)in_command.Inputs[0]
                : "All";

            var threadID = in_command.Inputs.Count > 1
                ? (int)in_command.Inputs[1]
                : XeConsole.MainThreadID;

            if (!Enum.TryParse(typeof(ERegisterType), registers, true, out var out_registers))
            {
                XeLogger.Error("Invalid register display mode.");
                return;
            }

            XeLogger.Log(new XeDebugger(in_console).GetProcessor(threadID).GetRegisterInfo((ERegisterType)out_registers));
        }

        public ECommandResponse ExecuteRaw(string[] in_args, string in_command, XeConsole in_console)
        {
            return ECommandResponse.Error;
        }

        public void Dispose() { }
    }
}
