using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Sprung
{
    class WindowManager
    {
        private const int MAX_CHARS_TITLE = 255;

        private List<Window> windows = new List<Window>();

        // Contains the processes which should not be listed as a result
        private List<String> eProcesses = new List<String>() { 
            "", 
            "Jumper", 
            "Program Manager",
            "TeamViewer",
            "TeamViewer Panel",
            "TeamViewer Panel (minimiert)",
            "Computer & Kontakte"
        };

        public void loadProcesses()
        {
            windows.Clear();
            EnumDelegate callback = new EnumDelegate(EnumWindowsProc);
            bool enumDel = EnumDesktopWindows(IntPtr.Zero, callback, IntPtr.Zero);
            if (!enumDel)
            {
                throw new Exception("Calling EnumDesktopWindows: Error ocurred: " + Marshal.GetLastWin32Error());
            }
        }

        public List<Window> getProcesses()
        {
            loadProcesses();
            return windows;
        }

        public void sendWindowToFront(Window process)
        {
            IntPtr hWnd = process.getAdress();
            forceWindowToFront(process.getAdress());
        }
        
        private bool EnumWindowsProc(IntPtr hWnd, int lParam)
        {
            if (!this.eProcesses.Contains(getProcessText(hWnd)) && IsWindowVisible(hWnd))
            {
                windows.Add(new Window(hWnd, getProcessText(hWnd)));
            }

            return true;
        }

        public string getProcessText(IntPtr hWnd)
        {
            StringBuilder strbTitle = new StringBuilder(MAX_CHARS_TITLE);
            strbTitle.Length = _GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
            return strbTitle.ToString();
        }

        public void forceWindowToFront(IntPtr hWnd)
        {
            uint foreThread = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
            uint appThread = GetCurrentThreadId();
            const uint SW_SHOW = 5;
            const uint SW_RESTORE = 9;

            AttachThreadInput(foreThread, appThread, true);
            Application.DoEvents();

            if (IsIconic(hWnd))
            {
                ShowWindow(hWnd, SW_RESTORE);
            }
            else
            {
                ShowWindow(hWnd, SW_SHOW);
            }

            BringWindowToTop(hWnd);
            SetForegroundWindow(hWnd.ToInt32());
            AttachThreadInput(foreThread, appThread, false);

        }

        private delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetWindowText", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int _GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool BringWindowToTop(HandleRef hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        [DllImport("user32.dll")]
        private static extern
        bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool IsIconic(IntPtr hWnd);
    }
}
