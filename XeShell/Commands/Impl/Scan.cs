using Spectre.Console;
using System.Text;
using XeSharp.Device;
using XeSharp.Device.Title;
using XeSharp.Helpers;
using XeShell.Helpers;

namespace XeShell.Commands.Impl
{
    [Command("scan", Inputs = [ typeof(string) ], OptionalInputs = [ typeof(string), typeof(string) ])]
    public class Scan : ICommand
    {
        private static XeModuleInfo _lastScannedModule;
        private static byte[] _memory;

        public void Execute(List<Command> in_commands, Command in_command, XeDbgConsole in_console)
        {
            var modules = in_console.GetModules();

            var param1 = (string)in_command.Inputs[0];
            var param2 = in_command.Inputs.Count > 1 ? (string)in_command.Inputs[1] : null;
            var param3 = in_command.Inputs.Count > 2 ? (string)in_command.Inputs[2] : null;

            if (!string.IsNullOrEmpty(param3) && !modules.ContainsKey(param3))
            {
                Console.WriteLine("The specified module is not resident in memory.");
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
                case "i8":      pattern = MemoryHelper.StructureToByteArray(MemoryHelper.ChangeType<sbyte>(param2));  break;
                case "u8":      pattern = MemoryHelper.StructureToByteArray(MemoryHelper.ChangeType<byte>(param2));   break;
                case "i16":     pattern = MemoryHelper.StructureToByteArray(MemoryHelper.ChangeType<short>(param2));  break;
                case "u16":     pattern = MemoryHelper.StructureToByteArray(MemoryHelper.ChangeType<ushort>(param2)); break;
                case "i32":     pattern = MemoryHelper.StructureToByteArray(MemoryHelper.ChangeType<int>(param2));    break;
                case "u32":     pattern = MemoryHelper.StructureToByteArray(MemoryHelper.ChangeType<uint>(param2));   break;
                case "i64":     pattern = MemoryHelper.StructureToByteArray(MemoryHelper.ChangeType<long>(param2));   break;
                case "u64":     pattern = MemoryHelper.StructureToByteArray(MemoryHelper.ChangeType<ulong>(param2));  break;
                case "f32":     pattern = MemoryHelper.StructureToByteArray(Convert.ToSingle(param2));                break;
                case "f64":     pattern = MemoryHelper.StructureToByteArray(Convert.ToDouble(param2));                break;
                case "string":  pattern = Encoding.UTF8.GetBytes(param2);                                             break;
                case "wstring": pattern = Encoding.BigEndianUnicode.GetBytes(param2);                                 break;
                
                default:
                    pattern = MemoryHelper.HexStringToByteArray(param1.Replace("?", "00"));
                    mask = FormatHelper.IDAPatternToCodeMask(param1);
                    isByteInput = true;
                    break;
            };

            if (string.IsNullOrEmpty(mask))
                mask = new string('x', pattern.Length);

            bool isUseCachedMemory = false;

            if (currentModule.Equals(_lastScannedModule))
            {
                if (_memory.Length > 0 && AnsiConsole.Confirm("Use cached memory?"))
                    isUseCachedMemory = true;
            }

            if (!isUseCachedMemory)
            {
                _memory = ConsoleHelper.ProgressCommon
                (
                    ctx =>
                    {
                        var task = ctx.AddTask("Downloading title image...");
                        var total = currentModule.ImageSize;

                        in_console.Client.ReadEvent += (s, e) =>
                        {
                            task.Description(
                                $"Downloading title image... " +
                                $"({FormatHelper.ByteLengthToDecimalString(e.BytesRead / 2)} / {FormatHelper.ByteLengthToDecimalString(total)})");

                            task.MaxValue(total);
                            task.Value(e.BytesRead / 2);
                        };

                        return in_console.ReadBytes(currentModule.BaseAddress, (int)total);
                    }
                );
            }

            var results = ConsoleHelper.StatusCommon($"Scanning for \"{(isByteInput ? param1 : MemoryHelper.ByteArrayToHexString(pattern))}\"...",
            //
                ctx =>
                {
                    return in_console.ScanSignature(_memory, pattern, mask, in_isFirstResult: false);
                }
            );

            _lastScannedModule = param3 == null
                ? modules.Last().Value
                : modules[param3];

            if (results.Count <= 0)
            {
                Console.WriteLine("Sequence not found.");
                return;
            }

            Console.WriteLine($"Found {results.Count} sequence{(results.Count == 1 ? ":" : "s:")}");

            foreach (var result in results)
                Console.WriteLine($"- 0x{result:X}");
        }

        public bool ExecuteRaw(string[] in_args, XeDbgConsole in_console)
        {
            return false;
        }
    }
}
