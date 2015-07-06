using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Sprung
{
    class Window : IComparable<Window>
    {

        private const int WINDOW_TITLE_MAX_CHARS = 255;
        private IntPtr handle;
        private String processName;
        private String title;
        private Boolean noTitle = false;
        private Process process;

        int matchingPriority;
        int matchingGroups;

        public Window(IntPtr handle)
        {
            if (handle.Equals(IntPtr.Zero))
            {
                throw new Exception("Invalid arguments passed to constructor in class \"Process\"");
            }
            else
            {
                // Initialization
                this.handle = handle;
                int processId = getWindowProcessId(handle.ToInt32());
                //Process p = Process.GetProcessById(processId);
                this.process = Process.GetProcessById(processId);
                this.processName = this.process.ProcessName;
                // Get window title
                StringBuilder strbTitle = new StringBuilder(WINDOW_TITLE_MAX_CHARS);
                strbTitle.Length = _GetWindowText(this.handle, strbTitle, strbTitle.Capacity + 1);
                this.title = strbTitle.ToString();
                this.noTitle = this.title.Length == 0;
                // Fix the name of some processes
                if (this.processName == "explorer")
                {
                    this.title += " - Explorer";
                }
            }
        }

        public void SendToFront()
        {
            // TODO: First show / resize / position bug has to be somewhere here
            uint foreThread = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
            uint appThread = GetCurrentThreadId();
            const uint SW_SHOW = 5;
            const uint SW_RESTORE = 9;
            AttachThreadInput(foreThread, appThread, true);
            Application.DoEvents();
            if (IsIconic(this.handle))
            {
                ShowWindow(this.handle, SW_RESTORE);
            }
            else
            {
                ShowWindow(this.handle, SW_SHOW);
            }
            BringWindowToTop(this.handle);
            SetForegroundWindow(this.handle.ToInt32());
            AttachThreadInput(foreThread, appThread, false);
        }

        public IntPtr getHandle()
        {
            return this.handle;
        }

        public String getProcessName()
        {
            return this.processName;
        }

        public String getTitle()
        {
            return this.title;
        }

        public Process getProcess()
        {
            return this.process;
        }

        public Int32 getWindowProcessId(Int32 handle)
        {
            Int32 pointer = 1;
            GetWindowThreadProcessId(handle, out pointer);
            return pointer;
        }

        public Boolean hasNoTitle()
        {
            return noTitle;
        }
      
        public int getMatchingPriority()
        {
            return this.matchingPriority;
        }

        public void setMatchingPriority(int matchingPriority)
        {
            this.matchingPriority = matchingPriority;
        }

        public int getMatchingGroups()
        {
            return this.matchingGroups;
        }

        public void setMatchingGroups(int matchingGroups)
        {
            this.matchingGroups = matchingGroups;
        }

        public int CompareTo(Window other)
        {
            return (getMatchingPriority() < other.getMatchingPriority()) ? 1 : (getMatchingPriority() > other.getMatchingPriority()) ? -1 :
                (getMatchingGroups() < other.getMatchingGroups()) ? -1 : (getMatchingGroups() > other.getMatchingGroups()) ? 1 : 0;
        }

        private delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        [DllImport("user32")]
        private static extern UInt32 GetWindowThreadProcessId(Int32 hWnd, out Int32 lpdwProcessId);

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
