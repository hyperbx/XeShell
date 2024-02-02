using Spectre.Console;
using XeSharp.Device;
using XeSharp.Helpers;
using XeShell.Helpers;

namespace XeShell.Commands.Impl
{
    [Command("upload", "ul", Inputs = [ typeof(string), typeof(string) ], OptionalInputs = [ typeof(bool) ])]
    public class Upload : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeDbgConsole in_console)
        {
            var localPath = (string)in_command.Inputs[0];

            var data = ConsoleHelper.StatusCommon($"Reading file: \"{localPath}\"", ctx => File.ReadAllBytes(localPath));

            ConsoleHelper.ProgressCommon
            (
                ctx =>
                {
                    var task = ctx.AddTask("Uploading file...");

                    in_console.Client.WriteEvent += (s, e) =>
                    {
                        var bytesWrittenStr = FormatHelper.ByteLengthToDecimalString(e.BytesWritten);
                        var bytesTotalStr = FormatHelper.ByteLengthToDecimalString(e.BytesTotal);

                        task.Description($"Uploading file... ({bytesWrittenStr} / {bytesTotalStr})");
                        task.MaxValue(e.BytesTotal);
                        task.Value(e.BytesWritten);
                    };

                    in_console.FileSystem.Upload(data, (string)in_command.Inputs[1]);
                }
            );
        }

        public bool ExecuteRaw(string[] in_args, XeDbgConsole in_console)
        {
            return false;
        }
    }
}
