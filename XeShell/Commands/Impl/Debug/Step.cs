using XeSharp.Debug;
using XeSharp.Device;
using XeSharp.Logger;

namespace XeShell.Commands.Impl
{
    [Command("step", OptionalInputs = [ typeof(int) ])]
    public class Step : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console)
        {
            var threadID = in_command.Inputs.Count > 0
                ? (int)in_command.Inputs[0]
                : XeConsole.MainThreadID;

            var debugger = new XeDebugger(in_console);

            if (!debugger.IsStopped())
            {
                XeLogger.Error("The thread must be stopped in order to step into the next instruction.");
                return;
            }

            XeLogger.Log($"Stepping into: 0x{debugger.StepInto(threadID).IAR:X8}");
        }

        public bool ExecuteRaw(string[] in_args, XeConsole in_console)
        {
            return false;
        }

        public void Dispose() { }
    }
}
