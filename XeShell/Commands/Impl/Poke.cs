using System.Text;
using XeSharp.Device;
using XeSharp.Helpers;
using XeSharp.Logger;
using XeSharp.Net;

namespace XeShell.Commands.Impl
{
    [Command("poke", Inputs = [ typeof(uint), typeof(string) ], OptionalInputs = [ typeof(string) ])]
    public class Poke : ICommand
    {
        // TODO: maybe move this into XeSharp?
        public static Dictionary<uint, byte[]> History = [];

        public void Execute(List<Command> in_commands, Command in_command, XeDbgConsole in_console)
        {
            var addr = (uint)in_command.Inputs[0];
            var len = 0;

            var param1 = (string)in_command.Inputs[1];
            var param2 = in_command.Inputs.Count > 2 ? (string)in_command.Inputs[2] : null;

            var response = new XeDbgResponse();

            // Used for byte array input.
            byte[] data = [];

            // Get data length.
            switch (param1)
            {
                case "i8":      len = 1;                 break;
                case "u8":      len = 1;                 break;
                case "i16":     len = 2;                 break;
                case "u16":     len = 2;                 break;
                case "i32":     len = 4;                 break;
                case "u32":     len = 4;                 break;
                case "i64":     len = 8;                 break;
                case "u64":     len = 8;                 break;
                case "f32":     len = 4;                 break;
                case "f64":     len = 8;                 break;
                case "nop":     len = 4;                 break;
                case "string":  len = param2.Length;     break;
                case "wstring": len = param2.Length * 2; break;

                default:
                    data = MemoryHelper.HexStringToByteArray(param1);
                    len = data.Length;
                    break;
            }

            byte[] originalData = len == 0 ? [] : in_console.ReadBytes(addr, len);

            // Preserve data for undoing.
            // TODO: stackable undo history.
            if (History.TryGetValue(addr, out byte[]? out_originalData))
            {
                if (out_originalData.Length < len)
                {
                    var bytes = out_originalData.ToList();

                    // Append unmodified bytes to the current original bytes.
                    bytes.AddRange(in_console.ReadBytes((uint)(addr + out_originalData.Length), len - out_originalData.Length));

                    History[addr] = [.. bytes];
                }
                else if (out_originalData.Length == len)
                {
                    History[addr] = in_console.ReadBytes(addr, len);
                }
            }
            else
            {
                History.Add(addr, originalData);
            }

            // Write data.
            switch (param1)
            {
                case "i8":      response = in_console.Write(addr, Convert.ToSByte(param2));                         break;
                case "u8":      response = in_console.Write(addr, Convert.ToByte(param2));                          break;
                case "i16":     response = in_console.Write(addr, Convert.ToInt16(param2));                         break;
                case "u16":     response = in_console.Write(addr, Convert.ToUInt16(param2));                        break;
                case "i32":     response = in_console.Write(addr, Convert.ToInt32(param2));                         break;
                case "u32":     response = in_console.Write(addr, Convert.ToUInt32(param2));                        break;
                case "i64":     response = in_console.Write(addr, Convert.ToInt64(param2));                         break;
                case "u64":     response = in_console.Write(addr, Convert.ToUInt64(param2));                        break;
                case "f32":     response = in_console.Write(addr, Convert.ToSingle(param2));                        break;
                case "f64":     response = in_console.Write(addr, Convert.ToDouble(param2));                        break;
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

        public bool ExecuteRaw(string[] in_args, XeDbgConsole in_console)
        {
            return false;
        }
    }
}
