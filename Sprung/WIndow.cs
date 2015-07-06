using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Sprung
{
    class Window : IComparable<Window>
    {

        private IntPtr adress;
        private String processName;
        private String processTitle;
        private Process process;

        int matchingPriority;
        int matchingGroups;

        public Window(IntPtr adress, String processTitle)
        {
            if (adress.Equals(IntPtr.Zero) || processTitle == "")
            {
                throw new Exception("Invalid arguments passed to constructor in class \"Process\"");
            }
            else
            {
                this.adress = adress;
                this.process = Process.GetProcessById(getWindowProcessId(adress.ToInt32()));
                this.processName = this.process.ProcessName;
                this.processTitle = processTitle;
                Console.WriteLine(this.processName);
                if (this.processName == "explorer")
                {
                    this.processTitle += " - Explorer";
                }
            }
        }

        public IntPtr getAdress()
        {
            return this.adress;
        }

        public String getProcessName()
        {
            return this.processName;
        }

        public String getProcessTitle()
        {
            return this.processTitle;
        }

        public Process getProcess()
        {
            return this.process;
        }


        public Int32 getWindowProcessId(Int32 adress)
        {
            Int32 pointer = 1;
            GetWindowThreadProcessId(adress, out pointer);
            return pointer;
        }
      
        public int getMatchingPriority()
        {
            return this.matchingPriority;
        }

        public void setMatchingPriority(int matchingPriority)
        {
            this.matchingPriority = matchingPriority;
        }

        public int getMatchingGroups()
        {
            return this.matchingGroups;
        }

        public void setMatchingGroups(int matchingGroups)
        {
            this.matchingGroups = matchingGroups;
        }

        public int CompareTo(Window other)
        {
            return (getMatchingPriority() < other.getMatchingPriority()) ? 1 : (getMatchingPriority() > other.getMatchingPriority()) ? -1 :
                (getMatchingGroups() < other.getMatchingGroups()) ? -1 : (getMatchingGroups() > other.getMatchingGroups()) ? 1 : 0;
        }

        [DllImport("user32")]
        private static extern UInt32 GetWindowThreadProcessId(Int32 hWnd, out Int32 lpdwProcessId);
    }
}
