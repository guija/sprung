using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jumper
{
    class ProcessMatcher
    {

        List<Process> processes;

        public ProcessMatcher(List<Process> processes)
        {
            this.processes = new List<Process>(processes);
        }
        public List<Process> getMatchedProcesses(String pattern)
        {
            foreach (Process process in processes)
            {
                String title = process.getProcessTitle().ToLower();
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

                process.setMatchingPriority(matchingChars);
                process.setMatchingGroups(matchingGroups);
            }

            // sort
            processes.Sort();
            return processes;
        }
    }
}
