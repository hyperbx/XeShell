using Spectre.Console;
using XeSharp.Device;
using XeSharp.Helpers;
using XeSharp.Logger;
using XeSharp.Net.Events;
using XeShell.Helpers;

namespace XeShell.Commands.Impl
{
    [Command("upload", "ul", Inputs = [ typeof(string), typeof(string) ])]
    public class Upload : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console)
        {
            var source = (string)in_command.Inputs[0];
            var dest   = (string)in_command.Inputs[1];

            void UploadFile(string in_source, string in_dest, bool in_isDirectory = false)
            {
                ConsoleHelper.ProgressCommon
                (
                    ctx =>
                    {
                        var task = ctx.AddTask("Uploading...");

                        var files = in_isDirectory
                            ? Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories).ToList()
                            : [in_source];

                        var totalSize = in_isDirectory
                            ? FileSystemHelper.GetDirectorySize(in_source)
                            : new FileInfo(in_source).Length;

                        task.MaxValue(totalSize);

                        var bytesWritten = 0U;
                        foreach (var file in files)
                        {
                            var relativePath = file.Remove(0, source.Length).Trim('\\');
                            var bytesTotal = 0U;

                            if (relativePath.IsNullOrEmptyOrWhiteSpace())
                                relativePath = Path.GetFileName(source);

                            in_console.Client.WriteEvent += WriteEvent;

                            void WriteEvent(object in_sender, ClientWriteEventArgs in_args)
                            {
                                task.Description($"Uploading \"{relativePath}\" ({in_args.BytesWrittenFormatted} / {in_args.BytesTotalFormatted})");
                                task.Value(bytesWritten + in_args.BytesWritten);

                                bytesTotal = in_args.BytesTotal;
                            }

                            if (in_isDirectory)
                                in_dest = Path.Combine(dest, relativePath);

                            in_console.FileSystem.Upload(file, in_dest);
                            in_console.Client.WriteEvent -= WriteEvent;

                            bytesWritten += bytesTotal;
                        }
                    }
                );
            }

            var destDrive = in_console.FileSystem.GetDrive(dest);

            if (!destDrive.IsSpaceAvailable(source))
            {
                XeLogger.Error($"Not enough space on volume {destDrive.Name[..^1]} for this upload.");
                return;
            }

            if (File.Exists(source))
            {
                UploadFile(source, dest);
                return;
            }

            if (!Directory.Exists(source))
            {
                XeLogger.Error($"Could Not Find {source}");
                return;
            }

            UploadFile(source, dest, in_isDirectory: true);
        }

        public ECommandResponse ExecuteRaw(string[] in_args, string in_command, XeConsole in_console)
        {
            return ECommandResponse.Error;
        }

        public void Dispose() { }
    }
}
