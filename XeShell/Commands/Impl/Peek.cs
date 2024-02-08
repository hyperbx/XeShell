using XeSharp.Device;
using XeSharp.Helpers;
using XeSharp.Logger;

namespace XeShell.Commands.Impl
{
    [Command("peek", Inputs = [ typeof(uint) ], OptionalInputs = [ typeof(int) ])]
    public class Peek : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeDbgConsole in_console)
        {
            var addr = (uint)in_command.Inputs[0];
            var len = 64;

            if (in_command.Inputs.Count > 1)
                len = (int)in_command.Inputs[1];

            var result = in_console.ReadBytes(addr, len);

            if (result.Length <= 0)
            {
                XeLogger.Error("Access denied.");
                return;
            }

            XeLogger.Log($"Peeking {len} bytes at 0x{addr:X}...\n");
            MemoryHelper.PrintBytes(result, addr);
        }

        public bool ExecuteRaw(string[] in_args, XeDbgConsole in_console)
        {
            return false;
        }
    }
}
