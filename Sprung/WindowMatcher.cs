using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprung
{
    class WindowMatcher
    {
        private WindowManager windowManager = null;

        public WindowMatcher(WindowManager windowManager)
        {
            this.windowManager = windowManager;
        }

        public List<Window> getMatchedProcesses(String pattern)
        {
            List<Window> windows = new List<Window>();
            foreach (Window window in this.windowManager.getProcesses())
            {
                String title = window.getTitle().ToLower();
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
                window.setMatchingPriority(matchingChars);
                window.setMatchingGroups(matchingGroups);
                windows.Add(window);
            }

            windows.Sort();
            return windows;
        }
    }
}
