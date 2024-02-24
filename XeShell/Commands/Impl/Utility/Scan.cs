using System.Text;
using XeSharp.Device;
using XeSharp.Helpers;
using XeSharp.Logger;
using XeShell.Helpers;

namespace XeShell.Commands.Impl
{
    [Command("scan", Inputs = [ typeof(string) ], OptionalInputs = [ typeof(string), typeof(string) ])]
    public class Scan : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console)
        {
            var modules = in_console.GetModules();

            var param1 = (string)in_command.Inputs[0];
            var param2 = in_command.Inputs.Count > 1 ? (string)in_command.Inputs[1] : null;
            var param3 = in_command.Inputs.Count > 2 ? (string)in_command.Inputs[2] : null;

            if (!string.IsNullOrEmpty(param3) && !modules.ContainsKey(param3))
            {
                XeLogger.Error("The specified module is not resident in memory.");
                return;
            }

            var currentModule = string.IsNullOrEmpty(param3)
                ? modules.Last().Value
                : modules[param3];

            byte[] pattern = [];
            var mask = string.Empty;
            var isByteInput = false;

            // Get scan pattern.
            switch (param1)
            {
                case "i8":      pattern = MemoryHelper.UnmanagedTypeToByteArray(MemoryHelper.ChangeType<sbyte>(param2));  break;
                case "u8":      pattern = MemoryHelper.UnmanagedTypeToByteArray(MemoryHelper.ChangeType<byte>(param2));   break;
                case "i16":     pattern = MemoryHelper.UnmanagedTypeToByteArray(MemoryHelper.ChangeType<short>(param2));  break;
                case "u16":     pattern = MemoryHelper.UnmanagedTypeToByteArray(MemoryHelper.ChangeType<ushort>(param2)); break;
                case "i32":     pattern = MemoryHelper.UnmanagedTypeToByteArray(MemoryHelper.ChangeType<int>(param2));    break;
                case "u32":     pattern = MemoryHelper.UnmanagedTypeToByteArray(MemoryHelper.ChangeType<uint>(param2));   break;
                case "i64":     pattern = MemoryHelper.UnmanagedTypeToByteArray(MemoryHelper.ChangeType<long>(param2));   break;
                case "u64":     pattern = MemoryHelper.UnmanagedTypeToByteArray(MemoryHelper.ChangeType<ulong>(param2));  break;
                case "f32":     pattern = MemoryHelper.UnmanagedTypeToByteArray(Convert.ToSingle(param2));                break;
                case "f64":     pattern = MemoryHelper.UnmanagedTypeToByteArray(Convert.ToDouble(param2));                break;
                case "string":  pattern = Encoding.UTF8.GetBytes(param2);                                                 break;
                case "wstring": pattern = Encoding.BigEndianUnicode.GetBytes(param2);                                     break;
                
                default:
                    pattern = MemoryHelper.HexStringToByteArray(param1.Replace("?", "00"));
                    mask = FormatHelper.IDAPatternToCodeMask(param1);
                    isByteInput = true;
                    break;
            };

            if (string.IsNullOrEmpty(mask))
                mask = new string('x', pattern.Length);

            // TODO: display progress.
            var results = ConsoleHelper.StatusCommon($"Scanning for \"{(isByteInput ? param1 : MemoryHelper.ByteArrayToHexString(pattern))}\"...",
            //
                ctx =>
                {
                    return in_console.Memory.ScanSignature(pattern, mask, in_isFirstResult: false);
                }
            );

            if (results.Count <= 0)
            {
                XeLogger.Log("Sequence not found.");
                return;
            }

            XeLogger.Log($"Found {results.Count} sequence{(results.Count == 1 ? ":" : "s:")}");

            foreach (var result in results)
                XeLogger.Log($"- 0x{result:X}");
        }

        public bool ExecuteRaw(string[] in_args, XeConsole in_console)
        {
            return false;
        }

        public void Dispose() { }
    }
}
