using XeSharp.Debug;
using XeSharp.Debug.RTTI;
using XeSharp.Device;
using XeSharp.Helpers;
using XeSharp.Logger;
using XeShell.Helpers;

namespace XeShell.Commands.Impl
{
    [Command("peek", Inputs = [ typeof(string) ], OptionalInputs = [ typeof(uint) ])]
    public class Peek : ICommand
    {
        public static Dictionary<uint, string> RTTICache = [];

        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console)
        {
            var source = (string)in_command.Inputs[0];
            var sourceDerefs = StringHelper.GetDereferenceCount(source);

            if (sourceDerefs > 0)
                source = StringHelper.TrimDereferences(source);

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

            // Dereference this pointer.
            addr = in_console.Memory.DereferencePointer(addr, sourceDerefs);

            var result = in_console.Memory.ReadBytes(addr, len);

            if (result.Length <= 0)
            {
                XeLogger.Error("Access denied.");
                return;
            }

            XeLogger.Log($"Peeking {len} bytes at 0x{addr:X}...\n");

            var rtti = RTTIFactory.GetRuntimeInfoFromClass(in_console, addr);

            if (rtti != null)
            {
                var pVftable = in_console.Memory.Read<uint>(addr);

                XeLogger.Log($"Object at 0x{addr:X8} is a class with a vftable at 0x{pVftable:X8}.");

                if (RTTICache.TryGetValue(pVftable, out string out_rttiInfo))
                {
                    XeLogger.Utility($"\n{out_rttiInfo}");
                }
                else
                {
                    var info = ConsoleHelper.StatusCommon("Getting runtime type information...",
                        ctx => rtti.GetClassInfo());

                    RTTICache.Add(pVftable, info);

                    XeLogger.Utility($"\n{info}");
                }
            }

            MemoryHelper.PrintBytes(result, addr);
        }

        public bool ExecuteRaw(string[] in_args, XeConsole in_console)
        {
            return false;
        }

        public void Dispose()
        {
            RTTICache.Clear();
        }
    }
}
