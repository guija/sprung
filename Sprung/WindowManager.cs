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
                        // TODO
                        // iterate over tabs
                        // add tabs as windows to windows list
                        // create derived window class "FirefoxTabProxyWindow"
                        getFirefoxTabs(window);
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
            
            // firefox test
            /*
            Process firefoxProcess = firefoxWindow.getProcess();
            AutomationElement rootElement = AutomationElement.FromHandle(firefoxProcess.MainWindowHandle);

            //String rootName = rootElement.Current.Name;
            //Console.WriteLine("rootName = " + rootName);
            AutomationElement browserTabsToolBar = rootElement.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, @"Browser tabs"));
            //Console.WriteLine("browserTabsToolBar = " + browserTabsToolBar.Current.Name);
            AutomationElement browserTabsToolBarGroup = TreeWalker.ControlViewWalker.GetFirstChild(browserTabsToolBar);
            //Console.WriteLine("browserTabsToolBarGroup = " + browserTabsToolBarGroup.Current.Name);

            List<AutomationElement> tabsAutomationElements = new List<AutomationElement>();
            */

            // iterate over tabs
            /*
            AutomationElement tab = TreeWalker.ControlViewWalker.GetFirstChild(browserTabsToolBarGroup);
            while (tab != null)
                String tabname = tab.Current.Name;
                if (tabname != "")
                {
                    tabsAutomationElements.Add(tab);
                    Console.WriteLine("tab = " + tabname);
                }
                tab = TreeWalker.ControlViewWalker.GetNextSibling(tab);
            }
            */

            return tabs;

           //Console.WriteLine("select random tab");
           // select random tab
           //Random random = new Random();
           //int idx = random.Next(tabs.Count);
           //Console.WriteLine("index = " + idx);
           //Console.WriteLine("try to focus '" + tabs[idx].Current.Name + "'");

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
