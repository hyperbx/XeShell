using XeSharp.Device;
using XeSharp.Helpers;
using XeSharp.Logger;

namespace XeShell.Commands.Impl
{
    [Command("undo", OptionalInputs = [ typeof(uint) ])]
    public class Undo : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeDbgConsole in_console)
        {
            var addr = in_command.Inputs.Count > 0 ? (uint)in_command.Inputs[0] : 0;

            if (addr == 0 && Poke.History.Count > 0)
                addr = Poke.History.Keys.ElementAt(Poke.History.Count - 1);

            if (!Poke.History.ContainsKey(addr))
            {
                XeLogger.Log("Nothing to undo" + (addr == 0 ? "" : $" at 0x{addr:X}") + "...");
                return;
            }

            var undo = Poke.History[addr];
            var len  = (uint)undo.Length;

            in_console.WriteBytes(addr, undo);

            XeLogger.Log($"Undone {len} bytes at 0x{addr:X}...\n");

            Poke.History.Remove(addr);

            MemoryHelper.PrintBytes(in_console.ReadBytes(addr, len), addr);
        }

        public bool ExecuteRaw(string[] in_args, XeDbgConsole in_console)
        {
            return false;
        }
    }
}
