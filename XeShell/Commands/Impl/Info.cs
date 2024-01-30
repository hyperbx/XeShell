using XeSharp.Device;

namespace XeShell.Commands.Impl
{
    [Command("info", "info.", OptionalInputs = [typeof(string)])]
    public class Info : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeDbgConsole in_console)
        {
            bool isDirInfo = in_command.InvokedName == "info.";

            if (in_command.Inputs.Count <= 0 && !isDirInfo)
            {
                Console.WriteLine(in_console.Info.ToString());
                return;
            }

            var path = isDirInfo ? "." : (string)in_command.Inputs[0];

            var node = path == "."
                ? in_console.FileSystem.CurrentDirectory
                : in_console.FileSystem.GetNodeFromPath(path);

            if (node == null)
                return;

            Console.WriteLine(node.GetInfo());
        }
    }
}
