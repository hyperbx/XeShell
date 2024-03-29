﻿using XeSharp.Device;
using XeShell.Services;

namespace XeShell.Commands.Impl
{
    [Command("xbdm", Inputs = [ typeof(string) ])]
    public class XBDM : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console) { }

        public ECommandResponse ExecuteRaw(string[] in_args, string in_command, XeConsole in_console)
        {
            if (in_args.Length <= 0 || in_args[0].ToLower() != "xbdm")
                return ECommandResponse.Error;

            new CommandService(in_console).SendCommand(in_command[5..], false);

            return ECommandResponse.Success;
        }

        public void Dispose() { }
    }
}
