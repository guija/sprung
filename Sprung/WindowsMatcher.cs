using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprung
{
    class WindowsMatcher
    {
        private List<Window> windows;

        public WindowsMatcher(List<Window> processes)
        {
            this.windows = new List<Window>(processes);
        }
        public List<Window> getMatchedProcesses(String pattern)
        {
            foreach (Window window in windows)
            {
                String title = window.getProcessTitle().ToLower();
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
            }

            windows.Sort();
            return windows;
        }
    }
}
