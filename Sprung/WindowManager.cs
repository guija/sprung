using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Automation;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Sprung
{
    class WindowManager
    {

        private Settings settings;
        private List<Window> windows = new List<Window>();

        public WindowManager(Settings settings)
        {
            this.settings = settings;
        }

        public List<Window> getProcesses()
        {
            windows.Clear();
            EnumDelegate callback = new EnumDelegate(EnumWindowsProc);
            bool enumDel = EnumDesktopWindows(IntPtr.Zero, callback, IntPtr.Zero);
            if (!enumDel)
            {
                throw new Exception("Calling EnumDesktopWindows: Error ocurred: " + Marshal.GetLastWin32Error());
            }
            return windows;
        }

        private bool EnumWindowsProc(IntPtr hWnd, int lParam)
        {
            if (IsWindowVisible(hWnd)) {
                Window window = new Window(hWnd);
                if (!settings.isWindowTitleExcluded(window.getTitle()) && !window.hasNoTitle())
                {
                    if (settings.isListTabsAsWindows() && window.getProcessName() == "firefox")
                    {
                        windows.AddRange(getFirefoxTabs(window));
                    }
                    else
                    {
                        windows.Add(window);
                    }
                    
                }
            }
            return true;
        }

        private List<Window> getFirefoxTabs(Window firefoxWindow)
        {
            List<Window> tabs = new List<Window>();
            string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            if (Environment.OSVersion.Version.Major >= 6) path = Directory.GetParent(path).ToString();
            path += "/AppData/Roaming/Mozilla/Firefox/Profiles";
            path = Directory.GetDirectories(path)[0];
            path += "/sessionstore-backups/";

            String recovery = path + "/recovery.js";
            String recoveryTmp = path + "/recovery.js.tmp";
            String file = File.Exists(recoveryTmp) ? recoveryTmp : recovery;

            StreamReader streamReader = new StreamReader(file);
            String content = streamReader.ReadToEnd();
            JObject data = JObject.Parse(content);
            int currentTabIndex = (int)data["windows"][0]["selected"] - 1;

            int i = 0;
            foreach (JObject tab in data["windows"][0]["tabs"])
            {
                JArray entries = (JArray)tab["entries"];
                JObject currentEntry = (JObject)entries.Last;
                String title = (String)currentEntry["title"];
                tabs.Add(new FirefoxTabWindow(firefoxWindow.getHandle(), currentTabIndex, i++, title));
            }
            return tabs;              
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
