using Nancy;
using Nancy.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sprung.Tabs.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprung.Tabs
{
    public class TabService : NancyModule
    {
        // TODO per autofac einbinden
        private WindowManager windowManager = new WindowManager();

        public TabService()
        {
            Post["/chrome"] = SetChromeTabs;
        }

        private string SetChromeTabs(dynamic parameters)
        {
            string body = this.Request.Body.AsString();
            Debug.WriteLine(body);

            JArray tabs = JArray.Parse(body);

            lock (ChromeTabWindow.TabsLock)
            {
                ChromeTabWindow.Tabs.Clear();
                foreach (JObject tab in tabs)
                {
                    ChromeTabWindow chromeTabWindow = JsonConvert.DeserializeObject<ChromeTabWindow>(tab.ToString());
                    ChromeTabWindow.Tabs.Add(chromeTabWindow);
                }

                Dictionary<string, int> windowTitleToWindowId = new Dictionary<string, int>();
                Dictionary<int, IntPtr> windowIdToHandle = new Dictionary<int, IntPtr>();
                int currentTabIndex = 0;

                foreach(ChromeTabWindow tab in ChromeTabWindow.Tabs)
                {
                    if (tab.IsCurrent)
                    {
                        Debug.WriteLine($"Raw: {tab.RawTitle}, windowId: {tab.WindowId}");
                        windowTitleToWindowId[tab.RawTitle] = tab.WindowId;
                        currentTabIndex = tab.Index;
                    }
                }

                foreach(Window window in windowManager.getWindows())
                {
                    string titleWithoutProgramName = window.RawTitle.Replace(" - Google Chrome", "");
                    if (windowTitleToWindowId.ContainsKey(titleWithoutProgramName))
                    {
                        int windowId = windowTitleToWindowId[titleWithoutProgramName];
                        Debug.WriteLine($"windowId: {windowId}, handle = {window.Handle}");
                        windowIdToHandle[windowId] = window.Handle;
                    }
                }

                foreach (ChromeTabWindow tab in ChromeTabWindow.Tabs)
                {
                    tab.Handle = windowIdToHandle[tab.WindowId];
                    int processId = tab.getWindowProcessId(tab.Handle.ToInt32());
                    tab.Process = Process.GetProcessById(processId);
                    tab.ProcessName = "chrome";
                    tab.Title = tab.RawTitle + " - Google Chrome";
                    tab.CurrentTabIndex = currentTabIndex;
                }
            }

            return string.Empty;
        }
    }
}
