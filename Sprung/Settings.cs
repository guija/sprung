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
using System.Windows.Forms;

namespace Sprung
{
    class Settings
    {

        private String path = "settings.json";
        private KeysConverter keysConverter = new KeysConverter();
        private List<Regex> excludedPatterns = new List<Regex>();
        private Boolean listTabsAsWindows = false;
        private Keys shortcut = Keys.Alt | Keys.Space;
        private JObject settings;

        public Settings()
        {
            load();
        }

        public void load() {
            try
            {
                StreamReader streamReader = new StreamReader(path);
                String content = streamReader.ReadToEnd();
                settings = JObject.Parse(content);
                if (settings["excluded"] != null)
                {
                    foreach (string pattern in settings["excluded"])
                    {
                        excludedPatterns.Add(new Regex(pattern));
                    }
                }
                if (settings["list_tabs_as_windows"] != null)
                {
                    listTabsAsWindows = (Boolean)settings["list_tabs_as_windows"];
                }
                if (settings["open_window_list"] != null)
                {
                    String shortcutAsString = (String)settings["open_window_list"];
                    this.shortcut = (Keys)keysConverter.ConvertFrom(shortcutAsString);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while loading settings from settings.json file: " + e.Message, "Sprung settings error");
            }
        }

        public void save()
        {
            settings["list_tabs_as_windows"] = listTabsAsWindows;
            settings["open_window_list"] = keysConverter.ConvertToString(shortcut);
            List<String> excludedPatternStrings = excludedPatterns.Select(x => x.ToString()).ToList();
            settings["excluded"] = new JArray(excludedPatternStrings);            
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

        public void setExcludedPatterns(List<String> patterns)
        {
            List<Regex> regexList = new List<Regex>();
            foreach (String pattern in patterns)
            {
                regexList.Add(new Regex(pattern));
            }
            this.excludedPatterns = regexList;
        }

        public void setExcludedPatterns(List<Regex> patterns)
        {
            this.excludedPatterns = patterns;
        }

        public List<Regex> getExcludedPatterns()
        {
            return excludedPatterns;
        }

        public Boolean isListTabsAsWindows()
        {
            return listTabsAsWindows;
        }

        public void setListTabsAsWindows(Boolean listTabsAsWindows)
        {
            this.listTabsAsWindows = listTabsAsWindows;
        }

        public void setShortcut(Keys keys)
        {
            this.shortcut = keys;
        }

        public Keys getShortcut()
        {
            return this.shortcut;
        }
    }
}
