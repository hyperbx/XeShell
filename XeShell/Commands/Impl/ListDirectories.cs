﻿using XeSharp.Device;
using XeSharp.Device.FileSystem;

namespace XeShell.Commands.Impl
{
    [Command("dir", OptionalInputs = [ typeof(string) ])]
    public class ListDirectories : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeDbgConsole in_console)
        {
            var path = in_command.Inputs.Count > 0 ? (string)in_command.Inputs[0] : string.Empty;

            var dir = string.IsNullOrEmpty(path)
                ? in_console.FileSystem.CurrentDirectory
                : in_console.FileSystem.GetDirectoryFromPath(path);

            if (dir == null)
                return;

            if (dir is XeFileSystemDrive || dir.Drive != null)
            {
                var drive = dir.Drive ?? dir as XeFileSystemDrive;

                if (!string.IsNullOrEmpty(drive.FriendlyName))
                    Console.WriteLine($" Volume in drive {drive.Name[..^1]} is {drive.FriendlyName}\n");
            }

            if (!string.IsNullOrEmpty(dir.ToString()))
                Console.WriteLine($" Directory of {dir}\n");

            var fileCount = 0;
            var dirCount = 0;
            var totalFileSize = 0L;

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

            Console.WriteLine($"\t       {fileCount} File(s) {string.Format("{0,14}", totalFileSize.ToString("N0"))} bytes");
            Console.WriteLine($"\t       {dirCount} Dir(s)");
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