using Spectre.Console;
using XeSharp.Device;
using XeSharp.Device.FileSystem;
using XeSharp.Logger;

namespace XeShell.Commands.Impl
{
    [Command("del", "rm", Inputs = [ typeof(string) ])]
    public class Delete : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console)
        {
            var path = (string)in_command.Inputs[0];

            if (!in_console.FileSystem.Exists(path))
            {
                XeLogger.Error($"Could Not Find {path}");
                return;
            }

            var node = in_console.FileSystem.GetNodeFromPath(path);

            if (node == null)
                return;

            if (node.Type == EXeFileSystemNodeType.Directory)
            {
                if (!AnsiConsole.Confirm($@"{node}\*, Are you sure?", false))
                    return;
            }

            node.Delete();
        }

        public ECommandResponse ExecuteRaw(string[] in_args, string in_command, XeConsole in_console)
        {
            return ECommandResponse.Error;
        }

        public void Dispose() { }
    }
}
