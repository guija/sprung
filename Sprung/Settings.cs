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
    public class Settings
    {
        private const String settingsFileName = "settings.json";

        private JObject settings;

        private static KeysConverter keysConverter = new KeysConverter();

        private List<Regex> excludedPatterns = new List<Regex>();

        public bool IsListTabsAsWindows { get; set; } = false;

        public Keys Shortcut { get; set; } = Keys.Alt | Keys.Space;

        public Keys ShortcutShowTabs { get; set; } = Keys.Alt | Keys.Shift | Keys.Space;

        private static Settings instance = null;

        public Settings()
        {
            load();
        }

        public static Settings GetInstance()
        {
            return instance ?? (instance = new Settings());
        }

        // TODO constants
        public void load()
        {
            try
            {
                StreamReader streamReader = new StreamReader(settingsFileName);
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
                    IsListTabsAsWindows = (bool)settings["list_tabs_as_windows"];
                }

                if (settings["open_window_list"] != null)
                {
                    String shortcutAsString = (String)settings["open_window_list"];
                    shortcutAsString = shortcutAsString.Replace("+", ",");
                    Shortcut = (Keys)Enum.Parse(typeof(Keys), shortcutAsString);
                }

                if (settings["open_window_list_with_tabs"] != null)
                {
                    String shortcutAsString = (String)settings["open_window_list_with_tabs"];
                    shortcutAsString = shortcutAsString.Replace("+", ",");
                    ShortcutShowTabs = (Keys)Enum.Parse(typeof(Keys), shortcutAsString);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while loading settings from settings.json file: " + e.Message, "Sprung settings error");
            }
        }

        // TODO
        /*
        public void save()
        {
            settings["list_tabs_as_windows"] = listTabsAsWindows;
            settings["open_window_list"] = keysConverter.ConvertToString(shortcut);
            List<String> excludedPatternStrings = excludedPatterns.Select(x => x.ToString()).ToList();
            settings["excluded"] = new JArray(excludedPatternStrings);            
        }
        */

        public Boolean IsWindowTitleExcluded(string windowTitle)
        {
            return excludedPatterns.Any(regex => regex.Match(windowTitle).Success);
        }

        public void SetExcludedPatterns(List<String> patterns)
        {
            excludedPatterns.Clear();
            excludedPatterns.AddRange(patterns.Select(pattern => new Regex(pattern)));
        }

        public void SetExcludedPatterns(List<Regex> patterns)
        {
            this.excludedPatterns = patterns;
        }
    }
}
