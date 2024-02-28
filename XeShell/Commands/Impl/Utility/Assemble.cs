using Keystone;
using XeSharp.Debug;
using XeSharp.Device;
using XeSharp.Helpers;
using XeSharp.Logger;
using XeShell.Helpers;

namespace XeShell.Commands.Impl
{
    [Command("asm", Inputs = [ typeof(string), typeof(string) ])]
    public class Assemble : ICommand
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

            var asm = (string)in_command.Inputs[1];
            byte[] instrs;

            try
            {
                instrs = ConsoleHelper.StatusCommon("Assembling...",
                    ctx => XeAssembler.Assemble(asm, out_addr));
            }
            catch (KeystoneException)
            {
                XeLogger.Error("Failed to assemble instruction(s).\n" +
                    "Keystone uses AT&T naming conventions for PowerPC, prepend registers with the '%' character.");

                return;
            }

            ConsoleHelper.StatusCommon("Writing code...",
                ctx => in_console.Memory.WriteBytes(out_addr, instrs));

            XeLogger.Log($"Successfully written instruction(s) to 0x{out_addr:X8}.\n");

            MemoryHelper.PrintBytes(in_console.Memory.ReadBytes(out_addr, (uint)instrs.Length), out_addr);
        }

        public ECommandResponse ExecuteRaw(string[] in_args, string in_command, XeConsole in_console)
        {
            return ECommandResponse.Error;
        }

        public void Dispose() { }
    }
}
