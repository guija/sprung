using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprung
{
    class WindowMatcher
    {
        private WindowManager windowManager;

        public WindowMatcher(WindowManager windowManager)
        {
            this.windowManager = windowManager;
        }

        public List<Window> match(String pattern)
        {
            // TODO getWIndows oder getWindow with Tabs?
            // nur ueber config steuern ob tabs verwendet werden oder nicht?
            // oder hier noch param listTabs einfuegen?
            return match(pattern, windowManager.GetWindows());
        }

        public List<Window> match(String pattern, List<Window> windows)
        {
            if (pattern.Length == 0) return windows;
            foreach (Window window in windows)
            {
                String title = window.Title.ToLower();
                pattern = pattern.ToLower();
                int matchingGroups = 0;
                int matchingChars = 0;
                int titleCharPos = 0;
                int textCharPos = 0;
                bool lastMatched = false;

                while (titleCharPos < title.Length && textCharPos < pattern.Length)
                {
                    char titleChar = title[titleCharPos];
                    char textChar = pattern[textCharPos];
                    if (titleChar == textChar)
                    {
                        titleCharPos++;
                        textCharPos++;
                        matchingChars++;
                        if (!lastMatched) matchingGroups++;
                        lastMatched = true;
                    }
                    else
                    {
                        titleCharPos++;
                        lastMatched = false;
                    }
                }

                window.MatchingPriority = matchingChars;
                window.MatchingGroups = matchingGroups;
            }

            windows.Sort();
            return windows;
        }

    }
}
