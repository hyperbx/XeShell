using System.Text;
using XeSharp.Debug;
using XeSharp.Device;
using XeSharp.Helpers;
using XeSharp.Logger;
using XeSharp.Net;

namespace XeShell.Commands.Impl
{
    [Command("poke", Inputs = [ typeof(string), typeof(string) ], OptionalInputs = [ typeof(string) ])]
    public class Poke : ICommand
    {
        public static Dictionary<uint, List<byte[]>> History = [];

        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console)
        {
            var source = (string)in_command.Inputs[0];
            var isRegister = source.ToLower().StartsWith("gpr") ||
                source.ToLower().StartsWith("fpr");

            var addr = 0U;
            var len = 0U;

            var param1 = (string)in_command.Inputs[1];
            var param2 = in_command.Inputs.Count > 2 ? (string)in_command.Inputs[2] : null;

            var response = new XeResponse();

            // Handle registers.
            if (isRegister)
            {
                using var processor = new XeDebugger(in_console).GetProcessorToken();

                if (!processor.TryParseRegisterIndexByName(source, out var out_index))
                    return;

                if (source.ToLower().StartsWith("gpr"))
                {
                    processor.GPR[out_index] = MemoryHelper.ChangeType<ulong>(param1);
                }
                else if (source.ToLower().StartsWith("fpr"))
                {
                    processor.FPR[out_index] = MemoryHelper.ChangeType<double>(param1);
                }

                return;
            }
            else
            {
                // Assume address.
                addr = MemoryHelper.ChangeType<uint>(source);
            }

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

            byte[] originalData = len == 0 ? [] : in_console.ReadBytes(addr, len);

            if (History.ContainsKey(addr))
            {
                History[addr].Insert(0, originalData);
            }
            else
            {
                History.Add(addr, [originalData]);
            }

            // Write data.
            switch (param1)
            {
                case "i8":      response = in_console.Write(addr, MemoryHelper.ChangeType<sbyte>(param2));          break;
                case "u8":      response = in_console.Write(addr, MemoryHelper.ChangeType<byte>(param2));           break;
                case "i16":     response = in_console.Write(addr, MemoryHelper.ChangeType<short>(param2));          break;
                case "u16":     response = in_console.Write(addr, MemoryHelper.ChangeType<ushort>(param2));         break;
                case "i32":     response = in_console.Write(addr, MemoryHelper.ChangeType<int>(param2));            break;
                case "u32":     response = in_console.Write(addr, MemoryHelper.ChangeType<uint>(param2));           break;
                case "i64":     response = in_console.Write(addr, MemoryHelper.ChangeType<long>(param2));           break;
                case "u64":     response = in_console.Write(addr, MemoryHelper.ChangeType<ulong>(param2));          break;
                case "f32":     response = in_console.Write(addr, MemoryHelper.ChangeType<float>(param2));          break;
                case "f64":     response = in_console.Write(addr, MemoryHelper.ChangeType<double>(param2));         break;
                case "nop":     response = in_console.Write(addr, 0x60000000);                                      break;
                case "string":  response = in_console.WriteBytes(addr, Encoding.UTF8.GetBytes(param2));             break;
                case "wstring": response = in_console.WriteBytes(addr, Encoding.BigEndianUnicode.GetBytes(param2)); break;
                default:        response = in_console.WriteBytes(addr, data);                                       break;
            }

            if (response.Status.IsFailed())
            {
                XeLogger.Error(response.Status);
                return;
            }

            XeLogger.Log($"Successfully written {len} bytes to 0x{addr:X}.\n");

            MemoryHelper.PrintBytes(in_console.ReadBytes(addr, len), addr);
        }

        public bool ExecuteRaw(string[] in_args, XeConsole in_console)
        {
            return false;
        }

        public void Dispose()
        {
            History.Clear();
        }
    }
}
