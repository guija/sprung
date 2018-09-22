using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprung.Windows
{
    public class WindowLastUsageComparer : IComparer<Window>
    { 
        public int Compare(Window x, Window y)
        {
            return y.LastActivation.CompareTo(x.LastActivation);
        }
    }
}
