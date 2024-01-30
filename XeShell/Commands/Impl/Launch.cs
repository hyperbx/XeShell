using XeSharp.Device;

namespace XeShell.Commands.Impl
{
    [Command("launch", Inputs = [ typeof(string) ])]
    public class Launch : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeDbgConsole in_console)
        {
            var path = (string)in_command.Inputs[0];
            var file = in_console.FileSystem.GetNodeFromPath(path);

            /* Checking the signature would be nice, so you can
               at least use binaries that have renamed extensions,
               but downloading that much data to check three bytes
               is not worth it at all. */
            if (Path.GetExtension(path) != ".xex")
            {
                Console.WriteLine("File must be executable.");
                return;
            }

            in_console.Client.SendCommand($"magicboot title=\"{file}\"");
        }
    }
}
