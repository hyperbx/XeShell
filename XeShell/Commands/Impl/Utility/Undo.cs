using XeSharp.Device;
using XeSharp.Helpers;
using XeSharp.Logger;

namespace XeShell.Commands.Impl
{
    [Command("undo", OptionalInputs = [ typeof(uint) ])]
    public class Undo : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console)
        {
            var addr = in_command.Inputs.Count > 0 ? (uint)in_command.Inputs[0] : 0;

            if (addr == 0 && Poke.History.Count > 0)
                addr = Poke.History.Keys.ElementAt(Poke.History.Count - 1);

            if (!Poke.History.ContainsKey(addr) || Poke.History[addr].Count <= 0)
            {
                XeLogger.Log("Nothing to undo" + (addr == 0 ? "" : $" at 0x{addr:X}") + "...");
                return;
            }

            var undo = Poke.History[addr][0];
            var len  = (uint)undo.Length;

            in_console.Memory.WriteBytes(addr, undo);

            XeLogger.Log($"Undone {len} bytes at 0x{addr:X}...\n");

            Poke.History[addr].RemoveAt(0);

            MemoryHelper.PrintBytes(in_console.Memory.ReadBytes(addr, len), addr);
        }

        public ECommandResponse ExecuteRaw(string[] in_args, string in_command, XeConsole in_console)
        {
            return ECommandResponse.Error;
        }

        public void Dispose() { }
    }
}
