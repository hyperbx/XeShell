﻿using XeSharp.Device;
using XeSharp.Logger;

namespace XeShell.Commands.Impl
{
    [Command("mkdir", Inputs = [ typeof(string) ])]
    public class MakeDirectory : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeDbgConsole in_console)
        {
            var path = (string)in_command.Inputs[0];

            if (in_console.FileSystem.Exists(path))
            {
                XeLogger.Error($"A subdirectory or file {path} already exists.");
                return;
            }

            in_console.FileSystem.CreateDirectory(path);
        }

        public bool ExecuteRaw(string[] in_args, XeDbgConsole in_console)
        {
            return false;
        }
    }
}
