using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Automation;

namespace Sprung
{
    class WindowManager
    {

        private Settings settings;
        private List<Window> windows = new List<Window>();

        // Contains the processes which should not be listed as a result
        private static List<String> excludedProcesses = new List<String>() { 
            "", 
            "Sprung", 
            "Program Manager - Explorer" 
        };

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
            
            // Find tab toolbar
            Process firefoxProcess = firefoxWindow.getProcess();
            AutomationElement rootElement = AutomationElement.FromHandle(firefoxProcess.MainWindowHandle);          
            AutomationElement browserTabsToolBar = rootElement.FindFirst(TreeScope.Children, new OrCondition(
                new PropertyCondition(AutomationElement.NameProperty, "Browser tabs"),
                new PropertyCondition(AutomationElement.NameProperty, "Browser-Tabs")));
            AutomationElement browserTabsToolBarGroup = TreeWalker.ControlViewWalker.GetFirstChild(browserTabsToolBar);

            // iterate over tabs
            int tabIndex = 0;
            int currentTabIndex = 0;
            AutomationElement tab = TreeWalker.ControlViewWalker.GetFirstChild(browserTabsToolBarGroup);
            while (tab != null) 
            {
                String title = tab.Current.Name;
                if (title != "")
                {
                    if (firefoxWindow.getTitle() == title + " - Mozilla Firefox")
                    {
                        currentTabIndex = tabIndex;
                    }
                    Window tabWindow = new FirefoxTabWindow(firefoxWindow.getHandle(), 0, tabIndex, title);
                    tabs.Add(tabWindow);
                    tabIndex++;
                }
                tab = TreeWalker.ControlViewWalker.GetNextSibling(tab);
            }
            // adjust current tab index for every tab
            foreach (FirefoxTabWindow window in tabs)
            {
                window.currentTabIndex = currentTabIndex;
            }
            return tabs;

           // Sadly this doesn't work in firefox & chrome (pattern not supported)
           //SelectionItemPattern sip = tabs[idx].GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern; 
           //sip.Select();
            
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
