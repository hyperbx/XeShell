using Spectre.Console;
using XeSharp.Device;
using XeShell.Helpers;

namespace XeShell.Commands.Impl
{
    [Command("download", "dl", Inputs = [ typeof(string), typeof(string) ])]
    public class Download : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console)
        {
            var serverPath = (string)in_command.Inputs[0];
            var localPath = (string)in_command.Inputs[1];

            var node = in_console.FileSystem.GetNodeFromPath(serverPath);

            var fileCount = node.GetTotalNodes(true, false);
            var fileIndex = 0;

            var status = "Downloading... ";

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
                            $"({e.BytesReadFormatted} / {e.BytesTotalFormatted})");

                        task.MaxValue(e.BytesTotal);
                        task.Value(e.BytesRead);
                    };

                    node.Download(localPath);
                }
            );
        }

        public ECommandResponse ExecuteRaw(string[] in_args, string in_command, XeConsole in_console)
        {
            return ECommandResponse.Error;
        }

        public void Dispose() { }
    }
}
