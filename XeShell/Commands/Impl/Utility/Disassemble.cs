using XeSharp.Debug;
using XeSharp.Device;
using XeSharp.Helpers;
using XeSharp.Logger;
using XeShell.Helpers;

namespace XeShell.Commands.Impl
{
    [Command("disasm", Inputs = [ typeof(string) ], OptionalInputs = [ typeof(uint) ])]
    public class Disassemble : ICommand
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

            var count = in_command.Inputs.Count > 1 ? (uint)in_command.Inputs[1] : 1;

            XeLogger.Log($"Disassembling {count} instructions from 0x{out_addr:X8}...\n");

            var instrs = ConsoleHelper.StatusCommon("Reading instructions...",
                ctx => XeDisassembler.Disassemble(in_console, out_addr, count));

            foreach (var instr in instrs)
            {
                XeLogger.Write(MemoryHelper.ByteArrayToHexString(instr.Bytes) + " ", EXeLogLevel.Utility);
                XeLogger.WriteLine(instr.ToString());
            }
        }

        public bool ExecuteRaw(string[] in_args, XeConsole in_console)
        {
            return false;
        }

        public void Dispose() { }
    }
}
