using XeSharp.Device;
using XeSharp.Logger;

namespace XeShell.Commands.Impl
{
    [Command("info", "info.", OptionalInputs = [ typeof(string) ])]
    public class Info : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console)
        {
            bool isDirInfo = in_command.InvokedName == "info.";

            if (in_command.Inputs.Count <= 0 && !isDirInfo)
            {
                XeLogger.Log(in_console.Info.ToString());
                return;
            }

            var path = isDirInfo ? "." : (string)in_command.Inputs[0];

            var node = path == "."
                ? in_console.FileSystem.CurrentDirectory
                : in_console.FileSystem.GetNodeFromPath(path);

            if (node == null)
                return;

            XeLogger.Log(node.GetInfo());
        }

        public bool ExecuteRaw(string[] in_args, XeConsole in_console)
        {
            return false;
        }

        public void Dispose() { }
    }
}
