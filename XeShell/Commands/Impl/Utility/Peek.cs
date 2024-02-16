using XeSharp.Debug;
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
            var source = (string)in_command.Inputs[0];

            var addr = 0U;
            var len = 64U;

            using (var debugger = new XeDebugger(in_console))
            {
                var register = source.ToLower();
                var processor = debugger.GetProcessor();

                switch (register)
                {
                    case "iar": addr = processor.IAR;       break;
                    case "lr":  addr = processor.LR;        break;
                    case "ctr": addr = (uint)processor.CTR; break;

                    default:
                    {
                        if (processor.TryParseGPRByName(register, out var out_gpr))
                        {
                            addr = (uint)out_gpr;
                            break;
                        }

                        // Assume address.
                        addr = MemoryHelper.ChangeType<uint>(source);

                        break;
                    }
                }
            }

            if (in_command.Inputs.Count > 1)
                len = (uint)in_command.Inputs[1];

            var result = in_console.ReadBytes(addr, len);

            if (result.Length <= 0)
            {
                XeLogger.Error("Access denied.");
                return;
            }

            XeLogger.Log($"Peeking {len} bytes at 0x{addr:X}...\n");
            MemoryHelper.PrintBytes(result, addr);
        }

        public bool ExecuteRaw(string[] in_args, XeConsole in_console)
        {
            return false;
        }

        public void Dispose() { }
    }
}
