using XeSharp.Device;

namespace XeShell.Commands.Impl
{
    [Command("launch", Inputs = [ typeof(string) ])]
    public class Launch : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console)
        {
            in_console.Launch((string)in_command.Inputs[0], string.Join(' ', in_command.Inputs.Skip(1)));
        }

        public ECommandResponse ExecuteRaw(string[] in_args, string in_command, XeConsole in_console)
        {
            if (in_args.Length <= 0)
                return ECommandResponse.Error;

            if (!in_console.FileSystem.Exists(in_args[0]) || Path.GetExtension(in_args[0]) != ".xex")
                return ECommandResponse.Error;

            in_console.Launch(in_args[0], string.Join(' ', in_args.Skip(1)));

            return ECommandResponse.Success;
        }

        public void Dispose() { }
    }
}
