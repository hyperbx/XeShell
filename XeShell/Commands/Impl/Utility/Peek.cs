using XeSharp.Device;
using XeSharp.Helpers;
using XeSharp.Logger;

namespace XeShell.Commands.Impl
{
    [Command("peek", Inputs = [ typeof(string) ], OptionalInputs = [ typeof(uint) ])]
    public class Peek : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console)
        {
            if (!in_console.Memory.TryParseAddressFromToken((string)in_command.Inputs[0], out var out_addr))
            {
                XeLogger.Error($"Failed to parse memory location.");
                return;
            }

            if (!in_console.Memory.IsAccessible(out_addr))
            {
                XeLogger.Error($"Access denied.");
                return;
            }

            var len = in_command.Inputs.Count > 1 ? (uint)in_command.Inputs[1] : 64U;

            XeLogger.Log($"Peeking {len} bytes from 0x{out_addr:X8}...\n");

            MemoryHelper.PrintBytes(in_console.Memory.ReadBytes(out_addr, len), out_addr);
        }

        public ECommandResponse ExecuteRaw(string[] in_args, string in_command, XeConsole in_console)
        {
            return ECommandResponse.Error;
        }

        public void Dispose() { }
    }
}
