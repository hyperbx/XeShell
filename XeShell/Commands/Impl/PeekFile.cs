using Spectre.Console;
using XeSharp.Device;
using XeSharp.Helpers;

namespace XeShell.Commands.Impl
{
    [Command("peekfile", "peekf", Inputs = [ typeof(string) ], OptionalInputs = [ typeof(int) ])]
    public class PeekFile : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeDbgConsole in_console)
        {
            var path = (string)in_command.Inputs[0];
            var file = in_console.FileSystem.GetNodeFromPath(path);
            var len = 64;

            if (in_command.Inputs.Count > 1)
                len = (int)in_command.Inputs[1];

            if (file == null)
                return;

            var data = AnsiConsole.Status().Start($"Downloading \"{path}\"... ({FormatHelper.ByteLengthToDecimalString(file.Size)})",
            //
                ctx =>
                {
                    ctx.Spinner(Spinner.Known.Line);

                    return file.Download(in_console).Data;
                }
            );

            Console.WriteLine($"Peeking {(len > file.Size ? file.Size : len)}/{file.Size} bytes from \"{path}\"...\n");

            MemoryHelper.PrintBytes(data.Take(len).ToArray());
        }

        public bool ExecuteRaw(string[] in_args, XeDbgConsole in_console)
        {
            return false;
        }
    }
}
