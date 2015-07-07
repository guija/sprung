using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Sprung
{
    class Settings
    {

        private List<Regex> excludedPatterns = new List<Regex>();
        private Boolean listTabsAsWindows = false;

        public Settings()
        {
            load();
        }

        public void load() {
            StreamReader streamReader = new StreamReader(@"settings.json");
            String content = streamReader.ReadToEnd();
            JObject settings = JObject.Parse(content);
            if(settings["excluded"] != null) {
                foreach(string pattern in settings["excluded"]) {
                    excludedPatterns.Add(new Regex(pattern));
                    Console.WriteLine(pattern);
                }
            }
            if (settings["list_tabs_as_windows"] != null)
            {
                listTabsAsWindows = (Boolean) settings["list_tabs_as_windows"];
            }
            else
            {
                listTabsAsWindows = false;
            }

            Console.WriteLine("list_tabs_as_windows: " + listTabsAsWindows);
            
        }

        public Boolean isWindowTitleExcluded(string windowTitle) {
            foreach (Regex regex in excludedPatterns)
            {
                if (regex.Match(windowTitle).Success)
                {
                    return true;
                }
            }
            return false;
        }

        public Boolean isListTabsAsWindows()
        {
            return listTabsAsWindows;
        }
    }
}
