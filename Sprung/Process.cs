using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IProcess = System.Diagnostics.Process;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Jumper
{
    class Process : IComparable<Process>
    {

        IntPtr adress;
        String processName;
        String processTitle;

        int matchingPriority;
        int matchingGroups;

        /*
         * 
         * Class for holding current process information.
         * ToDo: Need to save a pointer for adress of current process.
         * 
         */
        public Process(IntPtr adress, String processTitle)
        {
            if (adress.Equals(IntPtr.Zero) || processTitle == "")
            {
                throw new Exception("Invalid arguments passed to constructor in class \"Process\"");
            }
            else
            {
                this.adress = adress;
                this.processName = IProcess.GetProcessById(getWindowProcessId(adress.ToInt32())).ProcessName;
                this.processTitle = processTitle;
            }
        }

        /*
         * 
         * Get current process adress.
         * 
         */
        public IntPtr getAdress()
        {
            return this.adress;
        }

        /*
         * 
         * Get current process name.
         * 
         */
        public String getProcessName()
        {
            return this.processName;
        }

        /*
         * 
         * Get current process title.
         * The process title is the test showed in the title bar of an application.
         * 
         */
        public String getProcessTitle()
        {
            return this.processTitle;
        }

        /*
         * 
         * Get the current Process ID from the window / process located at the specific adress.
         * 
         */
        public Int32 getWindowProcessId(Int32 adress)
        {
            Int32 pointer = 1;
            GetWindowThreadProcessId(adress, out pointer);
            return pointer;
        }
        
        /*
         * 
         * Returns the amount of matching chars (priority) with the given pattern.
         * 
         */
        public int getMatchingPriority()
        {
            return this.matchingPriority;
        }

        /*
         * 
         * Set the matching priority (amount of matching chars).
         * 
         */
        public void setMatchingPriority(int matchingPriority)
        {
            this.matchingPriority = matchingPriority;
        }

        /*
         * 
         * Get the amount of matching character groups.
         * 
         */
        public int getMatchingGroups()
        {
            return this.matchingGroups;
        }

        /*
         * 
         * Return the amount of matching groups.
         * 
         */
        public void setMatchingGroups(int matchingGroups)
        {
            this.matchingGroups = matchingGroups;
        }

        /*
         * 
         * With the IComparable interface, we're allowed to compare this object with another object of the same class (Process).
         * Returns 1 when this object needs to be located behind the other object in a list.
         * Returns -1 when this object needs to be located before the other object.
         * Returns 0 when this object has the same priority as the other object.
         * 
         */
        public int CompareTo(Process other)
        {
            /*
             * Comparing the two objects by checking the amount of matching characters from the input given by the user.
             */
            return (getMatchingPriority() < other.getMatchingPriority()) ? 1 : (getMatchingPriority() > other.getMatchingPriority()) ? -1 :
                (getMatchingGroups() < other.getMatchingGroups()) ? -1 : (getMatchingGroups() > other.getMatchingGroups()) ? 1 : 0;
        }

        [DllImport("user32")]
        private static extern UInt32 GetWindowThreadProcessId(Int32 hWnd, out Int32 lpdwProcessId);
    }
}
