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

        public bool ExecuteRaw(string[] in_args, XeDbgConsole in_console)
        {
            if (in_args.Length <= 0 || !in_args[0].EndsWith(':'))
                return false;

            if (!in_console.FileSystem.GetDrives(false).Any(x => x.Name.ToLower() == in_args[0].ToLower()))
                return false;

            in_console.FileSystem.ChangeDirectory(in_args[0]);

            return true;
        }
    }
}
