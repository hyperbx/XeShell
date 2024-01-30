using XeSharp.Device;

namespace XeShell.Commands.Impl
{
    [Command("cd", "cd..", OptionalInputs = [ typeof(string) ])]
    public class ChangeDirectory : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeDbgConsole in_console)
        {
            var dir = in_command.Inputs.Count > 0 ? (string)in_command.Inputs[0] : null;

            var lastDir = in_console.FileSystem.CurrentDirectory;
            var currentDir = in_console.FileSystem.ChangeDirectory(in_command.InvokedName == "cd.." ? ".." : dir);

            if (currentDir == lastDir)
                Console.WriteLine("The system cannot find the path specified.");
        }
    }
}
