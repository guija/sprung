using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using Sprung.Tabs;
using System.Collections;
using System.Diagnostics;

namespace Sprung
{
    public class WindowManager
    {
        private Settings settings = Settings.GetInstance();

        private List<Window> windows = new List<Window>();

        private static WindowManager instance = null;

        public static WindowManager GetInstance()
        {
            return (instance = (instance ?? new WindowManager()));
        }

        // Key: handle of the window
        // Value: List of tabs that are in that window
        public Dictionary<IntPtr, List<TabWindow>> Tabs { get; set; } = new Dictionary<IntPtr, List<TabWindow>>();

        public object TabsLock { get; set; } = new object();

        public List<Window> GetWindowsWithTabs()
        {
            lock (TabsLock)
            {
                List<Window> windows = GetWindows();
                List<Window> windowsWithTabs = new List<Window>();

                foreach (Window window in windows)
                {
                    if (Tabs.ContainsKey(window.Handle) && Tabs[window.Handle].Count > 0)
                    {
                        windowsWithTabs.AddRange(Tabs[window.Handle]);
                    }
                    else
                    {
                        windowsWithTabs.Add(window);
                    }
                }

                return windowsWithTabs;
            }
        }

        public List<Window> GetWindows()
        {
            windows.Clear();
            EnumDelegate callback = new EnumDelegate(EnumWindowsProc);
            bool enumDel = EnumDesktopWindows(IntPtr.Zero, callback, IntPtr.Zero);
            if (!enumDel)
            {
                throw new Exception("Calling EnumDesktopWindows: Error ocurred: " + Marshal.GetLastWin32Error());
            }

            List<Window> filteredWindows = FilterWindows10ApplicationFrameHostWindows(windows);
            windows = filteredWindows;
            return windows;
        }

        private List<Window> FilterWindows10ApplicationFrameHostWindows(List<Window> windows)
        {
            List<Window> filteredList = new List<Window>();

            foreach (Window window in windows)
            {
                if(window.ProcessName == "ApplicationFrameHost")
                {
                    filteredList.Add(window);
                    continue;
                }

                Window wrapperWindow = windows.Where(w => w.TitleRaw == window.TitleRaw && w.Handle != window.Handle && w.ProcessName == "ApplicationFrameHost").FirstOrDefault();

                // Ignore all windows for which a window exists that has the ApplicationFrameHost process
                // name. Because that window is the one to be displayed
                if (wrapperWindow != null)
                {
                    // The wrapper window has the real process name, take that process name and replace the
                    // "ApplicationFrameHost" process name of the windows 10 app window
                    wrapperWindow.ProcessName = window.ProcessName;
                    continue;
                }

                filteredList.Add(window);
            }

            return filteredList;
        }

        private bool EnumWindowsProc(IntPtr hWnd, int lParam)
        {
            if (IsWindowVisible(hWnd)) {

                Window window = new Window(hWnd);

                if (!settings.IsWindowTitleExcluded(window.Title) && window.TitleRaw != string.Empty)
                {
                    windows.Add(window);
                }
            }

            return true;
        }

        private delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);
    }
}
