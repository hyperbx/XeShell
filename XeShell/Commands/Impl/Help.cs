﻿using XeSharp.Device;

namespace XeShell.Commands.Impl
{
    [Command("help", "?")]
    public class Help : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command, XeDbgConsole in_console)
        {
            static void PrintDefinitions(string in_category, Dictionary<string, (string Description, string Usage)> in_defs)
            {
                Console.WriteLine();
                Console.WriteLine($"{in_category}:");

                foreach (var entry in in_defs)
                {
                    Console.WriteLine($"{entry.Key}: {entry.Value.Description.Replace("\n", "\n        ")}");

					if (string.IsNullOrEmpty(entry.Value.Usage))
						continue;

					Console.WriteLine($"    Usage: {entry.Value.Usage.Replace("\n", "\n           ")}");
                }
            }

            PrintDefinitions("XeShell commands", _xeDefinitions);
            PrintDefinitions("XBDM commands", _xbdmDefinitions);

			if (in_console.Client.Info.IsFreebootXBDM)
                PrintDefinitions("XBDM extended commands (speculatory)", _xbdmExDefinitions);
        }

        private Dictionary<string, (string Description, string Usage)> _xeDefinitions = new()
        {
            { "cd/cd..",        ("Changes to the specified directory.", "cd [path]") },
            { "cls",            ("Clear XeShell command prompt.", "") },
            { "dir",            ("Lists the current or specified directory's contents.", "dir [opt: path]") },
            { "help/?",         ("Displays help information.", "") },
            { "info/info.",     ("Displays information about the connected console or specified filesystem node.", "info [opt: file/directory]") },
            { "launch",         ("Launches the specified executable binary.", "launch [path]") },
            { "peek",           ("Reads data from a memory address and prints it.", "peek [address] [opt: amount of bytes to read]") },
            { "peekfile/peekf", ("Reads data from a file and prints it.", "peekfile [path] [opt: amount of bytes to read]") },
            { "poke",           ("Writes data to a virtual address.", "poke [address] [opt: i8/u8/i16/u16/i32/u32/i64/u64/nop/string/wstring] [data]") },
            { "scan",           ("Scans memory for data.", "scan [opt: i8/u8/i16/u16/i32/u32/i64/u64/string/wstring] [data] [opt: module name]") },
            { "undo",           ("Undoes memory changes via the poke command (only works per session).", "undo [opt: address]") }
        };

        private Dictionary<string, (string Description, string Usage)> _xbdmDefinitions = new()
        {
			{ "altaddr",           ("Gets the title ip address.", "") },
			{ "break",             ("Manages breakpoints.", "") },
			{ "bye",               ("Ends the connection.", "") },
			{ "consolefeatures",   ("Gets the console features (debug, ram).", "") },
			{ "consoletype",       ("Gets the console type (test, dev).", "") },
			{ "continue",          ("Continues a thread.", "") },
			{ "dbgname",           ("Gets the console name.", "") },
			{ "debugger",          ("Signals that a debugger has been attached or deattached.", "") },
			{ "delete",            ("Deletes a file or directory.", "") },
			{ "dirlist",           ("Gets a list of items in a folder.", "") },
			{ "dmversion",         ("Gets the debugger version.", "") },
			{ "drivefreespace",    ("Gets some stats about the drive's volume size.", "") },
			{ "drivelist",         ("Gets the list of connected drives.", "") },
			{ "drivemap",          ("Sets if FLASH:\\ is visible in the drive browser.", "") },
			{ "dumpmode",          ("Configures the crash dump mode.", "") },
			{ "dvdeject",          ("Opens or closes the dvd tray.", "") },
			{ "getconsoleid",      ("Gets the console id.", "") },
			{ "getcontext",        ("Gets a thread context.", "") },
			{ "getcpukey",         ("Gets the cpu key.", "") },
			{ "getexecstate",      ("Displays the execution state.", "") },
			{ "getfile",           ("Xbox->PC transfer.", "") },
			{ "getfileattributes", ("Gets file attributes.", "") },
			{ "getmem",            ("Reads memory at a virtual address and returns its data.", "getmem addr=[address] length=[amount of bytes to read]") },
			{ "getmemex",          ("Gets memory in data.", "") },
			{ "getpid",            ("Gets the process id.", "") },
			{ "go",                ("Continues all threads.", "") },
			{ "help",              ("Gets info on every command.", "") },
			{ "isdebugger",        ("Gets info on the current debugger.", "") },
			{ "isstopped",         ("Returns information on a stopped thread.", "") },
			{ "magicboot",         ("Changes the currently running title, and/or reboots the console.", "") },
			{ "mkdir",             ("Creates a folder.", "") },
			{ "modsections",       ("Lists the module sections.", "") },
			{ "modules",           ("Returns a list of all the loaded modules (exe, dll).", "") },
			{ "nostopon",          ("Changes what we stop on.", "") },
			{ "notify",            ("Creates a notification channel.", "") },
			{ "notifyat",          ("Manages a notification channel.", "") },
			{ "rename",            ("Renames a file or directory.", "") },
			{ "resume",            ("Resumes a thread.", "") },
			{ "screenshot",        ("Takes a screenshot.", "") },
			{ "sendfile",          ("PC->Xbox transfer.", "") },
			{ "sendvfile",         ("PC->Xbox transfer with several files.", "") },
			{ "setcolor",          ("Sets the color of the console in Xbox 360 Neighborhood.", "setcolor name=[black/blue/bluegray/nosidecar/white]") },
			{ "setcontext",        ("Sets a thread context.", "") },
			{ "setfileattributes", ("Sets file attributes.", "") },
			{ "setmem",            ("Writes a byte array to a virtual address.", "setmem addr=[address] data=[bytes to write]") },
			{ "setsystime",        ("Sets the system time.", "") },
			{ "shutdown",          ("Shuts down the console.", "") },
			{ "stop",              ("Stops all threads.", "") },
			{ "stopon",            ("Changes what we stop on.", "") },
			{ "spew",              ("Spews debug output.", "") },
			{ "suspend",           ("Suspends a thread.", "") },
			{ "systeminfo",        ("Gets system info.", "") },
			{ "systime",           ("Sets the system time.", "") },
			{ "threadex",          ("Lists threads in a different format.", "") },
			{ "threadinfo",        ("Returns info on a thread.", "") },
			{ "threads",           ("Gets a list of threads.", "") },
			{ "xbeinfo",           ("Gets information on the running executable.", "") },
			{ "xexfield",          ("Gets an xex field.", "") },
			{ "walkmem",           ("Lists memory and protection pages.", "") }
        };

		private Dictionary<string, (string Description, string Usage)> _xbdmExDefinitions = new()
        {
            { "hwinfo",      ("Does some listing of stuff for cOz.", "") },
            { "khoungdm",    ("Throws a fatal error with code E69.", "") },
            { "objlist",     ("Lists objects for cOz.", "") },
            { "whomadethis", ("Responds with the author of Freeboot XBDM.", "") }
        };
    }
}