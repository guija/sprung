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
using Sprung.Tabs.Chrome;

namespace Sprung
{
    public class WindowManager
    {
        private Settings settings;
        private List<Window> windows = new List<Window>();
        private bool showTabs = false;

        public WindowManager()
        {
            // TODO load via autofac
            this.settings = new Settings();
        }

        public List<Window> getWindowsWithTabs()
        {
            this.showTabs = true;
            List<Window> windows = getWindows();

            if (showTabs)
            {
                lock (ChromeTabWindow.TabsLock)
                {
                    windows.AddRange(ChromeTabWindow.Tabs);
                }
            }

            this.showTabs = false;
            return windows;
        }

        public List<Window> getWindows()
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

                    /*
                    if ((showTabs || settings.isListTabsAsWindows()) && window.getProcessName() == "firefox")
                    {
                        windows.AddRange(getFirefoxTabs(window));
                    }
                    else if ((showTabs || settings.isListTabsAsWindows()) && window.getProcessName() == "iexplore")
                    {
                        windows.AddRange(getIETabs(window));
                    }
                    else
                    {
                    */

                    windows.Add(window);
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
            int currentTabIndex = 0, i = 0;
            foreach (JObject tab in data["windows"][0]["tabs"])
            {
                String title = (String) tab["entries"].Last["title"];
                title += " - Mozilla Firefox";
                currentTabIndex = title == firefoxWindow.getTitle() ? i : currentTabIndex;
                tabs.Add(new FirefoxTabWindow(firefoxWindow.getHandle(), currentTabIndex, i++, title));
            }
            foreach(FirefoxTabWindow w in tabs) {
                w.currentTabIndex = currentTabIndex;
            }
            return tabs;              
        }

        public List<Window> getIETabs(Window ieWindow) {
            List<Window> tabs = new List<Window>();
            foreach (SHDocVw.InternetExplorer tab in new SHDocVw.ShellWindows())
            {
                if (!tab.LocationURL.StartsWith("file"))
                {
                    Console.WriteLine(tab.LocationURL);
                    Console.WriteLine("visible = " + tab.Visible);
                    Console.WriteLine("hasFocus = " + ((mshtml.HTMLDocument)tab.Document).hasFocus());
                    tabs.Add(new InternetExplorerTabWindow(tab));
                }
            }
            return tabs;
        }

        private delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

    }
}
