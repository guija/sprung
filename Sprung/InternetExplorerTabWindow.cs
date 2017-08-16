using SHDocVw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sprung
{
    public class InternetExplorerTabWindow : Window
    {
        private SHDocVw.InternetExplorer ie;

        public InternetExplorerTabWindow( SHDocVw.InternetExplorer tab ) : base(new IntPtr(tab.HWND)) {
            this.Title = tab.LocationName + " - Internet Explorer";
            this.ie = tab;
            Console.Write(this.Handle);
        }

        public override void SendToFront()
        {
            base.SendToFront();
        }
    }
}
