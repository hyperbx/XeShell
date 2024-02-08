using XeSharp.Device;
using XeSharp.Device.FileSystem;
using XeSharp.Logger;

namespace XeShell.Commands.Impl
{
    [Command("dir", "ls", OptionalInputs = [ typeof(string) ])]
    public class ListDirectories : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeDbgConsole in_console)
        {
            var path = in_command.Inputs.Count > 0 ? (string)in_command.Inputs[0] : string.Empty;
            var isCurrentDir = string.IsNullOrEmpty(path);

            var dir = isCurrentDir
                ? in_console.FileSystem.CurrentDirectory
                : in_console.FileSystem.GetDirectoryFromPath(path);

            XeFileSystemDrive drive = null;

            if (dir == null)
                return;

            if (isCurrentDir)
                dir.Refresh();

            if (dir is XeFileSystemDrive || dir.Drive != null)
            {
                drive = dir.Drive ?? dir as XeFileSystemDrive;

                if (!string.IsNullOrEmpty(drive.FriendlyName))
                    XeLogger.Log($" Volume in drive {drive.Name[..^1]} is {drive.FriendlyName}\n");
            }

            if (!string.IsNullOrEmpty(dir.ToString()))
                XeLogger.Log($" Directory of {dir}\n");

            var fileCount = 0;
            var dirCount = 0;
            var totalFileSize = 0UL;

            if (dir?.Parent != null)
                PrintNavigationNodes(dir);

            if (dir != null && dir.Nodes.Any())
            {
                foreach (var node in dir.Nodes)
                {
                    switch (node.Type)
                    {
                        case EXeFileSystemNodeType.File:
                            totalFileSize += node.Size;
                            fileCount++;
                            break;

                        case EXeFileSystemNodeType.Directory:
                            dirCount++;
                            break;
                    }

                    PrintNode(node);
                }
            }

            XeLogger.Log($"\t       {fileCount} File(s) {string.Format("{0,14}", totalFileSize.ToString("N0"))} bytes");
            XeLogger.Log($"\t       {dirCount} Dir(s)" + (drive == null ? "" : $" {string.Format("{0,16}", drive.FreeSpace.ToString("N0"))} bytes free"));
        }

        public bool ExecuteRaw(string[] in_args, XeDbgConsole in_console)
        {
            return false;
        }

        private static void PrintNavigationNodes(XeFileSystemNode in_node)
        {
            var fmt = $"{in_node.DateModified:dd/MM/yyyy  hh:mm tt}\t<DIR>          .";

            Console.WriteLine(fmt);
            Console.WriteLine(fmt + '.');
        }

        private static void PrintNode(XeFileSystemNode in_node)
        {
            Console.Write(in_node.DateModified.ToString("dd/MM/yyyy  hh:mm tt"));

            if (in_node.Type == EXeFileSystemNodeType.Directory)
                Console.Write("\t<DIR>          ");

            if (in_node.Type == EXeFileSystemNodeType.File)
            {
                Console.WriteLine(string.Format("{0,18} {1}", in_node.Size.ToString("N0"), in_node.Name));
            }
            else
            {
                Console.WriteLine(in_node.Name);
            }
        }
    }
}
