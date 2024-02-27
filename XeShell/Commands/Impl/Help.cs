using XeSharp.Device;
using XeSharp.Logger;

namespace XeShell.Commands.Impl
{
    [Command("help", "?")]
    public class Help : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeConsole in_console)
        {
            static void PrintDefinitions(string in_category, Dictionary<string, (string Description, string Usage, string Deprecated)> in_defs)
            {
                XeLogger.Log($"\n{in_category};");

                foreach (var entry in in_defs)
                {
                    var isUsage = !string.IsNullOrEmpty(entry.Value.Usage);
                    var isDeprecated = !string.IsNullOrEmpty(entry.Value.Deprecated);

                    XeLogger.Log($"{(isUsage || isDeprecated ? "┬" : "-")} {entry.Key}: " +
                    entry.Value.Description.Replace("\n", $"\n│{new string(' ', entry.Key.Length + 3)}"));

                    static void CreateBranch(string in_name, string in_value, ConsoleColor in_colour, bool in_isLast = true)
                    {
                        var branch = in_isLast
                            ? '└'
                            : '├';

                        // Line drawing for readability.
                        var line = branch + "── ";

                        // Whitespace for line feeds.
                        var lf = (in_isLast ? " " : "│") +
                            new string(' ', line.Length + in_name.Length + 1);

                        var oldColour = Console.ForegroundColor;

                        Console.ForegroundColor = in_colour;
                        Console.Write(line);
                        Console.WriteLine($"{in_name}: {in_value.Replace("\n", $"\r\n{lf}")}");
                        Console.ForegroundColor = oldColour;
                    }

                    if (isUsage)
                        CreateBranch("Usage", entry.Value.Usage, ConsoleColor.Green, !isDeprecated);

                    if (isDeprecated)
                        CreateBranch("Deprecated", entry.Value.Deprecated, ConsoleColor.Red);
                }
            }

            PrintDefinitions("XeShell commands", _xeShellDefinitions);
            PrintDefinitions("Available XBDM commands", _xbdmDefinitions);

            if (in_console.Client.Info.IsFreebootXBDM)
                PrintDefinitions("Freeboot XBDM commands", _freebootXbdmDefinitions);
        }

        public bool ExecuteRaw(string[] in_args, XeConsole in_console)
        {
            return false;
        }

        public void Dispose() { }

        private Dictionary<string, (string Description, string Usage, string Deprecated)> _xeShellDefinitions = new()
        {
            { "analyse/analyze", ("analyses a memory address containing a class for RTTI.", "analyse [address/register]", "") },
            { "asm",             ("assembles PowerPC instructions to bytecode at the specified memory address.", "asm [address/register] [asm]", "") },
            { "attach",          ("attaches the debugger to the console.", "", "") },
            { "cd/cd..",         ("changes to the specified directory.", "cd [path]\nDrives can be changed into without invoking the command.", "") },
            { "cls/clear",       ("clears the command prompt.", "", "") },
            { "del/rm",          ("deletes a remote file or directory.", "del [remote path]", "") },
            { "detach",          ("detaches the debugger from the console.", "", "") },
            { "dir/ls",          ("lists the current or specified directory's contents.", "dir [opt: path]", "") },
            { "disasm",          ("disassembles PowerPC instructions at the specified memory address.", "disasm [address/register] [opt: amount]", "") },
            { "download/dl",     ("downloads a remote file.", "download [remote source] [local destination]", "") },
            { "exit",            ("exits the command prompt.", "", "") },
            { "help/?",          ("displays help information.", "", "") },
            { "info/info.",      ("displays information about the connected console or specified filesystem node.", "info [opt: file/directory]", "") },
            { "launch",          ("launches the specified executable binary.", "launch [path]\n*.xex files can be invoked directly by name.", "") },
            { "mkdir",           ("creates a remote directory.", "mkdir [path]", "") },
            { "peek",            ("reads data from a memory address and prints it.", "peek [address/register] [opt: amount]", "") },
            { "peekfile/peekf",  ("reads data from a file and prints it.", "peekfile [path] [opt: amount]", "") },
            { "poke",            ("writes data to a virtual address.", "poke [address/register] [opt: i8/u8/i16/u16/i32/u32/i64/u64/nop/string/wstring] [data]", "") },
            { "scan",            ("scans memory for data.", "scan [opt: i8/u8/i16/u16/i32/u32/i64/u64/string/wstring] [data] [opt: module name]", "") },
            { "step",            ("steps to the next instruction from the current breakpoint.", "step [opt: thread ID]", "") },
            { "undo",            ("undoes memory changes via the poke command (only works per session).", "undo [opt: address]", "") },
            { "upload/ul",       ("uploads a local file.", "upload [local source] [remote destination]", "") },
            { "xbdm",            ("sends a command to XBDM, bypassing XeShell overrides.", "xbdm [command]", "") },
            { "xcpu",            ("dumps the current CPU state.", "xcpu [opt: thread ID]", "") }
        };

        private Dictionary<string, (string Description, string Usage, string Deprecated)> _xbdmDefinitions = new()
        {
            { "altaddr",           ("gets the title IP address.", "", "") },
            { "break",             ("sets a breakpoint at the specified address.", "break addr=[address]", "") },
            { "bye",               ("ends the connection.", "", "") },
            { "consolefeatures",   ("gets the console features.", "", "use the info command.") },
            { "consoletype",       ("gets the console type.", "", "use the info command.") },
            { "continue",          ("continues a thread.", "continue thread=[id]", "") },
            { "dbgname",           ("gets the console name.", "", "use the info command.") },
            { "debugger",          ("signals that a debugger has been attached or detached.", "", "use the attach and detach commands.") },
            { "delete",            ("deletes a file or directory.", "delete name=[path] [opt: dir]", "use the del command.") },
            { "dirlist",           ("gets a list of items in a folder.", "dirlist name=[path]", "use the dir command.") },
            { "dmversion",         ("gets the debugger version.", "", "use the info command.") },
            { "drivefreespace",    ("gets some stats about the drive's volume size.", "", "use the dir or info commands.") },
            { "drivelist",         ("gets the list of connected drives.", "", "use the dir command.") },
            { "drivemap",          ("sets if FLASH:\\ is visible in the drive browser.", "", "use the dir command.") },
            { "dumpmode",          ("configures the crash dump mode.", "", "") },
            { "dvdeject",          ("ejects the DVD drive.", "", "") },
            { "getconsoleid",      ("gets the console ID.", "", "use the info command.") },
            { "getcontext",        ("gets a thread's context.", "", "use the xcpu command.") },
            { "getfile",           ("downloads a remote file.", "", "use the download command.") },
            { "getfileattributes", ("gets a file's attributes.", "", "") },
            { "getmem",            ("reads memory at a virtual address and returns its data.", "getmem addr=[address] length=[amount]", "use the peek command.") },
            { "getmemex",          ("gets memory in data.", "", "use the peek command.") },
            { "getpid",            ("gets the process ID.", "", "") },
            { "go",                ("continues all threads.", "", "") },
            { "isdebugger",        ("gets info about the current debugger.", "", "") },
            { "isstopped",         ("returns information about a stopped thread.", "", "") },
            { "magicboot",         ("changes the currently running title and/or reboots the console.", "", "use the launch command.") },
            { "mkdir",             ("creates a folder.", "", "XeShell overrides this command.") },
            { "modsections",       ("lists the current module's sections.", "", "") },
            { "modules",           ("returns a list of all the loaded modules.", "", "") },
            { "nostopon",          ("changes what we stop on.", "", "") },
            { "notify",            ("creates a notification channel.", "", "") },
            { "notifyat",          ("manages a notification channel.", "", "") },
            { "rename",            ("renames a file or directory.", "", "") },
            { "resume",            ("resumes a thread.", "", "") },
            { "screenshot",        ("takes a screenshot.", "", "") },
            { "sendfile",          ("uploads a local file.", "", "use the upload command.") },
            { "sendvfile",         ("uploads multiple local files.", "", "use the upload command.") },
            { "setcontext",        ("sets a thread's context.", "", "") },
            { "setfileattributes", ("sets a file's attributes.", "", "") },
            { "setmem",            ("writes a byte array to a virtual address.", "setmem addr=[address] data=[bytes to write]", "use the poke command.") },
            { "setsystime",        ("sets the system time.", "", "") },
            { "stop",              ("stops all threads.", "", "") },
            { "stopon",            ("changes what we stop on.", "", "") },
            { "suspend",           ("suspends a thread.", "", "") },
            { "systeminfo",        ("gets system information.", "", "use the info command.") },
            { "systime",           ("sets the system time.", "", "") },
            { "threadinfo",        ("returns info about a thread.", "", "") },
            { "threads",           ("gets a list of running threads.", "", "") },
            { "xbeinfo",           ("gets information on the running executable.", "", "") },
            { "xexfield",          ("gets an XEX field from a loaded module.", "xexfield module=[name] field=[id]", "only accepts returning the entrypoint field (0x00010100) with Freeboot XBDM.") },
            { "walkmem",           ("lists memory and protection pages.", "", "") }
        };

        private Dictionary<string, (string Description, string Usage, string Deprecated)> _freebootXbdmDefinitions = new()
        {
            { "getcpukey",    ("gets the CPU key.", "", "") },
            { "getexecstate", ("displays the execution state.", "", "") },
            { "help",         ("gets info on every command.", "", "XeShell overrides this command.") },
            { "hwinfo",       ("does some listing of stuff for cOz.", "", "") },
            { "khoungdm",     ("throws a fatal error with code E69.", "", "") },
            { "objlist",      ("lists objects for cOz.", "", "") },
            { "setcolor",     ("sets the color of the console in Xbox 360 Neighborhood.", "setcolor name=[black/blue/bluegray/nosidecar/white]", "") },
            { "shutdown",     ("shuts down the console.", "", "") },
            { "spew",         ("spews debug output.", "", "") },
            { "threadex",     ("lists threads in a different format.", "", "") },
            { "whomadethis",  ("responds with the author of Freeboot XBDM.", "", "") }
        };
    }
}
