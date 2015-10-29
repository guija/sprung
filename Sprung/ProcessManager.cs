using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IProcess = System.Diagnostics.Process;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Jumper
{
    class ProcessManager
    {
        private const int MAX_CHARS_TITLE = 255;

        private List<Process> processes = new List<Process>();

        /*
         * Contains the processes which should not be listed as a result.
         */
        private List<String> eProcesses = new List<String>() { "", "Jumper", "Program Manager" };

        public void loadProcesses()
        {
            /*
             * Jumper works with the same instance all the time.
             * So you need to clear the old results in case the user commited a new searching pattern.
             */
            processes.Clear();
            EnumDelegate callback = new EnumDelegate(EnumWindowsProc);
            bool enumDel = EnumDesktopWindows(IntPtr.Zero, callback, IntPtr.Zero);
            if (!enumDel)
            {
                throw new Exception("Calling EnumDesktopWindows: Error ocurred: " + Marshal.GetLastWin32Error());
            }
        }


        /*
         * Return all processes.
         */
        public List<Process> getProcesses()
        {
            return processes;
        }

        /*
         * Bring the specified window / process to the top.
         */
        public void sendWindowToFront(Process process)
        {
            IntPtr hWnd = process.getAdress();
            forceFrontWindow(process.getAdress());
        }
        
        /*
         * Crawling through all top-level windows opened by the user.
         */
        private bool EnumWindowsProc(IntPtr hWnd, int lParam)
        {

            if (!this.eProcesses.Contains(getProcessText(hWnd)) && IsWindowVisible(hWnd))
            {
                processes.Add(new Process(hWnd, getProcessText(hWnd)));
            }

            return true;
        }

        /*
         * Get the process text.
         * Limited to 255 characters for preventing cutted strings.
         */
        public string getProcessText(IntPtr hWnd)
        {
            StringBuilder strbTitle = new StringBuilder(MAX_CHARS_TITLE);
            strbTitle.Length = _GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
            return strbTitle.ToString();
        }

        /*
         * Force the window on the specified adress to show up on the front.
         * Used by sendWindowToFront(Process process).
         */
        public void forceFrontWindow(IntPtr hWnd)
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

        /*
         * Import of EnumDesktopWindows
         * Neccessary for crawling through all top-level windows.
         */
        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        /*
         * Import of GetWindowsText
         * Neccessary to list the matching processes in the Jumper window.
         */
        [DllImport("user32.dll", EntryPoint = "GetWindowText", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int _GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

        /*
         * Import of IsWindowsVisible
         * Neccessary to determine the current visible status of the specified window.
         */
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        /*
         * Import of SetForegroundWindow
         * Neccessary for bringing the selected window to the foreground.
         * All keyboard actions are then focused on this specific window.
         */
        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);

        /*
         * Import of GetForegroundWindow
         * Neccessary for getting the adress of the currently used window.
         */
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        /*
         * Import of GetWindowThreadProcessId
         * Neccessary for retrieving the thread identifier.
         */
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /*
         * Import of GetWindowThreadProcessId
         * Neccessary if the ProcessId is not needed
         * If ProcessId not needed, pass IntPtr.Zero to the second argument.
         */
        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        /*
         * Import of GetCurrentThreadId
         * Neccessary for retrieving the id of the current used thread.
         */
        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        /*
         * Import of AttachThreadInput
         * Neccessary for attaching two threads.
         * Attaches / Detaches input from one thread to another.
         */
        [DllImport("user32.dll")]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        /*
         * Import of BringWindowToTop
         * Neccessary for bringing the specified window to the top of the Z order. 
         * If the window is a top-level window, it is activated. 
         * If the window is a child window, the top-level parent window associated with the child window is activated.
         * 
         * Argument as an adress (IntPtr) and as a handle (HandleRef)
         */
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool BringWindowToTop(HandleRef hWnd);

        /*
         * Import of ShowWindow
         * Neccessary to set the show status of any windows.
         * Examples: SW_SHOW = 5 (activates and displays specified window and keeps current position and size).
         */
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        /*
         * Import of ShowWindowAsync
         * Neccessary if setting the status of a window without waiting for the return.
         */
        [DllImport("user32.dll")]
        private static extern
        bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        /*
         * Import of SendMessage
         * Neccessary for sending a message to one or more windows.
         * Returns when window has processed the message.
         */
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        /*
         * Import of IsIconic
         * Neccessary for check if window is iconic (minimized).
         */
        [DllImport("user32.dll")]
        static extern bool IsIconic(IntPtr hWnd);
    }
}
