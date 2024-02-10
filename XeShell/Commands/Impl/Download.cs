using Spectre.Console;
using XeSharp.Device;
using XeSharp.Helpers;
using XeShell.Helpers;

namespace XeShell.Commands.Impl
{
    [Command("download", "dl", Inputs = [ typeof(string), typeof(string) ])]
    public class Download : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeDbgConsole in_console)
        {
            var serverPath = (string)in_command.Inputs[0];
            var localPath = (string)in_command.Inputs[1];

            var node = in_console.FileSystem.GetNodeFromPath(serverPath);

            var fileCount = node.GetTotalNodes(true, false);
            var fileIndex = 0;

            var status = "Downloading file" + (fileCount == 0 ? "..." : "s...") + " ";

            ConsoleHelper.ProgressCommon
            (
                ctx =>
                {
                    var task = ctx.AddTask(status);

                    in_console.Client.ReadEvent += (s, e) =>
                    {
                        /* TODO: figure out why this sometimes surpasses the file count.
                                 It seems to be related to passing directory paths to the
                                 Download function and XBDM returning an empty bin response. */
                        if (e.IsBegin)
                            fileIndex = (int)Math.Min(fileCount, fileIndex + 1);

                        task.Description(
                            $"{status}" + (fileCount == 0 ? "" : $"({fileIndex} / {fileCount}) ") +
                            $"({FormatHelper.ByteLengthToDecimalString(e.BytesRead)} / {FormatHelper.ByteLengthToDecimalString(e.BytesTotal)})");

                        task.MaxValue(e.BytesTotal);
                        task.Value(e.BytesRead);
                    };

                    node.Download(localPath);
                }
            );
        }

        public bool ExecuteRaw(string[] in_args, XeDbgConsole in_console)
        {
            return false;
        }

        public void Dispose() { }
    }
}
