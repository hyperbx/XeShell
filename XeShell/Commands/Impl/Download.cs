using XeSharp.Device;

namespace XeShell.Commands.Impl
{
    [Command("download", "dl", Inputs = [ typeof(string), typeof(string) ])]
    public class Download : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeDbgConsole in_console)
        {
            var serverPath = (string)in_command.Inputs[0];
            var localPath = (string)in_command.Inputs[1];
        }

        public bool ExecuteRaw(string[] in_args, XeDbgConsole in_console)
        {
            return false;
        }
    }
}
