using System;
using System.Runtime.InteropServices;

namespace LogUtils.Helpers.Console
{
    public static class ConsoleVirtualizationHelper
    {
        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        public static void EnableVirtualTerminalProcessing()
        {
            try
            {
                var handle = GetStdHandle(STD_OUTPUT_HANDLE);
                if (!GetConsoleMode(handle, out uint mode))
                {
                    return;
                }
                mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
                SetConsoleMode(handle, mode);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to enable virtual terminal processing", e);
            }
        }
    }
}