using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using Sprung.Tabs;
using System.Collections;

namespace Sprung
{
    public class WindowManager
    {
        private Settings settings;

        private List<Window> windows = new List<Window>();

        private static WindowManager instance = null;

        public static WindowManager GetInstance()
        {
            return (instance = (instance ?? new WindowManager()));
        }

        // Key: tuple (process name, window id)
        // Value: List of windows that are in that process & window
        public Dictionary<IntPtr, List<TabWindow>> Tabs { get; set; } = new Dictionary<IntPtr, List<TabWindow>>();

        public object TabsLock { get; set; } = new object();

        public WindowManager()
        {
            // TODO load via autofac
            this.settings = new Settings();
        }

        public List<Window> getWindowsWithTabs()
        {
            lock (TabsLock)
            {
                List<Window> windows = getWindows();

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
