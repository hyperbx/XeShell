using System.Text;
using XeSharp.Debug;
using XeSharp.Debug.Processor;
using XeSharp.Device;
using XeSharp.Helpers;
using XeSharp.Logger;

namespace XeShell.Commands.Impl
{
    [Command("poke", Inputs = [ typeof(string), typeof(string) ], OptionalInputs = [ typeof(string) ])]
    public class Poke : ICommand
    {
        public static Dictionary<uint, List<byte[]>> History = [];

        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console)
        {
            var source = (string)in_command.Inputs[0];
            var sourceDerefs = StringHelper.GetDereferenceCount(source);

            if (sourceDerefs > 0)
                source = StringHelper.TrimDereferences(source);

            var addr = 0U;
            var len = 0U;

            var param1 = (string)in_command.Inputs[1];
            var param2 = in_command.Inputs.Count > 2 ? (string)in_command.Inputs[2] : null;

            // Handle registers.
            using (var processor = new XeDebugger(in_console).GetProcessorToken())
            {
                if (processor.TryParseRegisterByName(source, out var out_type, out var out_index))
                {
                    // Poke register directly.
                    if (sourceDerefs <= 0)
                    {
                        if (out_type == ERegisterType.GPR)
                        {
                            processor.GPR[out_index] = MemoryHelper.ChangeType<ulong>(param1);
                        }
                        else if (out_type == ERegisterType.FPR)
                        {
                            processor.FPR[out_index] = MemoryHelper.ChangeType<double>(param1);
                        }

                        return;
                    }
                    else if (out_type == ERegisterType.GPR)
                    {
                        // Use "dereferenced" register value.
                        addr = (uint)processor.GPR[out_index];
                    }
                }
                else
                {
                    // Assume address.
                    addr = MemoryHelper.ChangeType<uint>(source);
                }
            }

            // Dereference this pointer.
            addr = in_console.Memory.DereferencePointer(addr, sourceDerefs);

            // Used for byte array input.
            byte[] data = [];

            // Get data length.
            switch (param1)
            {
                case "i8":      len = 1;                       break;
                case "u8":      len = 1;                       break;
                case "i16":     len = 2;                       break;
                case "u16":     len = 2;                       break;
                case "i32":     len = 4;                       break;
                case "u32":     len = 4;                       break;
                case "i64":     len = 8;                       break;
                case "u64":     len = 8;                       break;
                case "f32":     len = 4;                       break;
                case "f64":     len = 8;                       break;
                case "nop":     len = 4;                       break;
                case "string":  len = (uint)param2.Length;     break;
                case "wstring": len = (uint)param2.Length * 2; break;

                default:
                    data = MemoryHelper.HexStringToByteArray(param1);
                    len = (uint)data.Length;
                    break;
            }

            byte[] originalData = len == 0 ? [] : in_console.Memory.ReadBytes(addr, len);

            // TODO: support registers.
            if (History.ContainsKey(addr))
            {
                History[addr].Insert(0, originalData);
            }
            else
            {
                History.Add(addr, [originalData]);
            }

            // Write data.
            var response = param1 switch
            {
                "i8"      => in_console.Memory.Write(addr, MemoryHelper.ChangeType<sbyte>(param2)),
                "u8"      => in_console.Memory.Write(addr, MemoryHelper.ChangeType<byte>(param2)),
                "i16"     => in_console.Memory.Write(addr, MemoryHelper.ChangeType<short>(param2)),
                "u16"     => in_console.Memory.Write(addr, MemoryHelper.ChangeType<ushort>(param2)),
                "i32"     => in_console.Memory.Write(addr, MemoryHelper.ChangeType<int>(param2)),
                "u32"     => in_console.Memory.Write(addr, MemoryHelper.ChangeType<uint>(param2)),
                "i64"     => in_console.Memory.Write(addr, MemoryHelper.ChangeType<long>(param2)),
                "u64"     => in_console.Memory.Write(addr, MemoryHelper.ChangeType<ulong>(param2)),
                "f32"     => in_console.Memory.Write(addr, MemoryHelper.ChangeType<float>(param2)),
                "f64"     => in_console.Memory.Write(addr, MemoryHelper.ChangeType<double>(param2)),
                "nop"     => in_console.Memory.Write(addr, 0x60000000),
                "string"  => in_console.Memory.WriteBytes(addr, Encoding.UTF8.GetBytes(param2)),
                "wstring" => in_console.Memory.WriteBytes(addr, Encoding.BigEndianUnicode.GetBytes(param2)),
                _         => in_console.Memory.WriteBytes(addr, data),
            };

            if (response.Status.IsFailed())
            {
                XeLogger.Error(response.Status);
                return;
            }

            XeLogger.Log($"Successfully written {len} bytes to 0x{addr:X8}.\n");

            MemoryHelper.PrintBytes(in_console.Memory.ReadBytes(addr, len), addr);
        }

        public ECommandResponse ExecuteRaw(string[] in_args, string in_command, XeConsole in_console)
        {
            return ECommandResponse.Error;
        }

        public void Dispose()
        {
            History.Clear();
        }
    }
}
