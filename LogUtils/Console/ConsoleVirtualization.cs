using System;
using System.Runtime.InteropServices;

namespace LogUtils.Console
{
    internal static class ConsoleVirtualization
    {
        private const int STD_OUTPUT_HANDLE = -11;
        private const int STD_ERROR_HANDLE = -12;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        public static bool TryEnableVirtualTerminal(out int win32Error)
        {
            win32Error = 0;

            //Non‑Windows platforms (Linux, macOS, etc.) already support ANSI escapes
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return true;

            //Skip VT processing when either stdout or stderr isn’t attached to a real TTY
            if (System.Console.IsOutputRedirected || System.Console.IsErrorRedirected)
                return false;

            //Try to turn on ANSI processing on both the standard output and error handles
            foreach (var stdHandle in new[] { STD_OUTPUT_HANDLE, STD_ERROR_HANDLE })
            {
                //Get the raw Win32 handle for stdout or stderr
                var handle = GetStdHandle(stdHandle);
                //If the handle is invalid (NULL or -1), grab the error
                if (handle == IntPtr.Zero || handle == new IntPtr(-1))
                {
                    win32Error = Marshal.GetLastWin32Error();
                    return false;
                }

                //Read the current console mode flags for this handle
                if (!GetConsoleMode(handle, out uint mode))
                {
                    //On failure, record the Win32 error
                    win32Error = Marshal.GetLastWin32Error();
                    return false;
                }

                //Add the ENABLE_VIRTUAL_TERMINAL_PROCESSING bit to allow ANSI sequences
                mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
                //Write the updated mode back to the console
                if (!SetConsoleMode(handle, mode))
                {
                    //If setting the mode fails, record the error and abort
                    win32Error = Marshal.GetLastWin32Error();
                    return false;
                }
            }

            //Everything succeeded ANSI is enabled
            return true;
        }
    }
}