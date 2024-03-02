# XeShell
A command prompt that interfaces with XBDM on a remote Xbox 360.

# Features
- DOS-like filesystem navigation and manipulation.
- Downloading and uploading data.
- Peeking and poking memory.
- Peeking file data.
- Signature scanning.
- Debugging and analysis.

# Prerequisites
### Building
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

### Running
- .NET 8.0 Runtime ([x86](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-8.0.1-windows-x86-installer), [x64](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-8.0.1-windows-x64-installer))
- Xbox 360 hardware (JTAG/RGH/DevKit) with a network connection
- Xbox Debug Monitor (XBDM) - *only the Freeboot plugin tested and working, but it should be interoperable with the official XBDM module.*

# Commands
**XeShell** includes all existing XBDM commands (including custom commands from Freeboot XBDM), as well as extended commands listed below.

Command|Alias|Description
-------|-----|-----------
`analyse`|`analyze`|Analyses a memory address containing a class for RTTI.
`asm`||Assembles PowerPC instructions to bytecode at the specified memory address.
`cd`|`cd..`|Changes to the specified directory.
`cls`|`clear`|Clears the command prompt.
`del`|`rm`|Deletes a remote file or directory.
`dir`|`ls`|Lists the current or specified directory's contents.
`disasm`||Disassembles PowerPC instructions at the specified memory address.
`download`|`dl`|Downloads a remote file or directory.
`exit`||Exits the command prompt.
`help`|`?`|Displays help information.
`info`|`info.`|Displays information about the connected console or specified filesystem node.
`launch`||Launches the specified executable binary.
`mkdir`||Creates a remote directory.
`peek`||Reads data from a memory address and prints it.
`peekfile`|`peekf`|Reads data from a file and prints it.
`poke`||Writes data to a virtual address.
`scan`||Scans memory for data.
`step`||Steps to the next instruction from the current breakpoint.
`undo`||Undoes memory changes via the `poke` command (only works per session).
`upload`|`ul`|Uploads a local file or directory.
`xbdm`||Sends a command to XBDM, bypassing XeShell overrides.
`xcpu`||Dumps the current CPU state.

***See the `help` command for additional information.***