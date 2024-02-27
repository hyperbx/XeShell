using ClassAnalyser.Analysis.RTTI;
using Spectre.Console;
using XeSharp.Debug.Analysis;
using XeSharp.Device;
using XeSharp.Helpers;
using XeSharp.Logger;
using XeShell.Helpers;
using XeShell.Services;

namespace XeShell.Commands.Impl
{
    [Command("analyse", "analyze", Inputs = [ typeof(string) ])]
    public class Analyse : ICommand
    {
        public static Dictionary<uint, string> RTTICache = [];

        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console)
        {
            if (!in_console.Memory.TryParseAddressFromToken((string)in_command.Inputs[0], out var out_addr))
            {
                XeLogger.Error("Failed to parse memory location.");
                return;
            }

            if (!in_console.Memory.IsAccessible(out_addr))
            {
                XeLogger.Error("Access denied.");
                return;
            }

            var factory = new RTTIFactory(new RTTIReaderXenon(in_console));
            var rtti    = factory.GetRuntimeInfoFromClass(out_addr);

            if (rtti == null)
            {
                XeLogger.Log($"Nothing to analyse at this location.");
                return;
            }

            var pVftable = in_console.Memory.Read<uint>(out_addr);

            XeLogger.Log($"Object at 0x{out_addr:X8} is a class with a vftable at 0x{pVftable:X8}.");

            if (RTTICache.TryGetValue(pVftable, out var out_rttiInfo))
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

            if (AnsiConsole.Confirm("Export to C++ headers?", false))
            {
                var isExportBaseClasses = AnsiConsole.Confirm("Export base classes?");
                var path = new PromptService() { IsHistorySaved = false }.Show("Path: ");

                if (path.IsNullOrEmptyOrWhiteSpace())
                {
                    XeLogger.Error("Export aborted.");
                    return;
                }

                ConsoleHelper.StatusCommon("Exporting classes...",
                    ctx => rtti.ExportHeaders(path, isExportBaseClasses, true));
            }
        }

        public bool ExecuteRaw(string[] in_args, string in_command, XeConsole in_console)
        {
            return false;
        }

        public void Dispose()
        {
            RTTICache.Clear();
        }
    }
}
