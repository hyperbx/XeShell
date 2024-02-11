using System.Runtime.InteropServices;

namespace XeShell.Interop
{
    public partial class Win32
    {
        [LibraryImport("user32.dll")]
        private static partial short GetAsyncKeyState(EKeyCode in_key);

        public static bool IsKeyDown(EKeyCode in_key)
        {
            return (GetAsyncKeyState(in_key) & 0x8000) != 0;
        }
    }
}
