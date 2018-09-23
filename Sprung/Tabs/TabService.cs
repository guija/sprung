using Nancy;
using Nancy.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sprung.Windows;

namespace Sprung.Tabs
{
    public class TabService : NancyModule
    {
        private WindowManager windowManager = WindowManager.GetInstance();

        public TabService()
        {
            Post["/chrome"] = SetChromeTabs;
            Post["/firefox"] = SetFirefoxTabs;
            Get["/test"] = Test;

            // Allow CORS, so that extension are able to get answer
            After.AddItemToEndOfPipeline(ctx => ctx.Response
                .WithHeader("Access-Control-Allow-Origin", "*")
                .WithHeader("Access-Control-Allow-Methods", "POST,GET")
                .WithHeader("Access-Control-Allow-Headers", "Accept, Origin, Content-type"));
        }

        private string SetFirefoxTabs(dynamic parameters)
        {
            return SetTabs(parameters, "- Mozilla Firefox", "firefox");
        }

        private string SetChromeTabs(dynamic parameters)
        {
            return SetTabs(parameters, "- Google Chrome", "chrome");
        }

        private string Test(dynamic parameters)
        {
            return "ok";
        }

        private string SetTabs(dynamic parameters, string windowTitleSuffix, string processName)
        {
            string body = this.Request.Body.AsString();
            JArray tabs = JArray.Parse(body);

            lock (windowManager)
            {
                List<TabWindow> tabList = new List<TabWindow>();

                foreach (JObject tab in tabs)
                {
                    TabWindow tabWindow = JsonConvert.DeserializeObject<TabWindow>(tab.ToString());
                    tabList.Add(tabWindow);
                }

                Dictionary<string, int> windowTitleToWindowId = new Dictionary<string, int>();
                IntPtr handle = IntPtr.Zero;

                TabWindow currentTab = tabList.Where(tab => tab.IsCurrent).FirstOrDefault();

                if (currentTab == null)
                {
                    return string.Empty;
                }

                string currentTabTitle = currentTab.TitleRaw.Trim();
                int currentTabIndex = currentTab.Index;

                foreach (Window window in windowManager.Windows.Values)
                {
                    string titleWithoutProgramName = window.TitleRaw.Replace(windowTitleSuffix, string.Empty).Trim();

                    if (currentTabTitle == titleWithoutProgramName)
                    {
                        handle = window.Handle;
                    }
                }

                // Happens when tab was switched, then the title is not the
                // same of the window
                if (handle == IntPtr.Zero)
                {
                    return string.Empty;
                }

                foreach (TabWindow tab in tabList)
                {
                    tab.Handle = handle;
                    int processId = tab.GetWindowProcessId(tab.Handle.ToInt32());
                    tab.Process = Process.GetProcessById(processId);
                    tab.ProcessName = processName;
                    tab.Title = tab.TitleRaw + windowTitleSuffix;
                    tab.CurrentTabIndex = currentTabIndex;
                }

                if (!tabList.Any() && windowManager.Tabs.ContainsKey(handle))
                {
                    windowManager.Tabs.Remove(handle);
                }
                else
                {
                    windowManager.Tabs[handle] = tabList;
                }
            }

            return string.Empty;
        }
    }
}
