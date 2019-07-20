using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DofusSwitch
{
    class Utils
    {
        [DllImport("User32.DLL")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_MINIMIZE = 6;
        private const int SW_RESTORE = 9;


        public static void MinProcess(int PID)
        {
            Process proc = Process.GetProcessById(PID);
            if (proc.ProcessName == string.Empty)
                return;

            ShowWindow(proc.MainWindowHandle, SW_MINIMIZE);
        }

        public static void MaxProcess(int PID)
        {
            Process proc = Process.GetProcessById(PID);
            if (proc.ProcessName == string.Empty)
                return;

            ShowWindow(proc.MainWindowHandle, SW_RESTORE);
        }

        public static string GetAlphaString(string text)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in text)
            {
                if ((c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
