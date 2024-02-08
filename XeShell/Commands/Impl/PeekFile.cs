using XeSharp.Device;
using XeSharp.Helpers;
using XeSharp.Logger;
using XeShell.Helpers;

namespace XeShell.Commands.Impl
{
    [Command("peekfile", "peekf", Inputs = [ typeof(string) ], OptionalInputs = [ typeof(uint) ])]
    public class PeekFile : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeDbgConsole in_console)
        {
            var path = (string)in_command.Inputs[0];
            var file = in_console.FileSystem.GetNodeFromPath(path);
            var len = 64U;

            if (in_command.Inputs.Count > 1)
                len = (uint)in_command.Inputs[1];

            if (file == null)
                return;

            var data = ConsoleHelper.StatusCommon(
                $"Downloading \"{path}\"... ({FormatHelper.ByteLengthToDecimalString(file.Size)})",
                ctx => file.Download().Data);

            XeLogger.Log($"Peeking {(len > file.Size ? file.Size : len)}/{file.Size} bytes from \"{path}\"...\n");

            MemoryHelper.PrintBytes(data.Take((int)len).ToArray());
        }

        public bool ExecuteRaw(string[] in_args, XeDbgConsole in_console)
        {
            return false;
        }
    }
}
