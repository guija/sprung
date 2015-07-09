using SHDocVw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sprung
{
    class IETabWindow : Window
    {

        private SHDocVw.InternetExplorer ie;

        public IETabWindow( SHDocVw.InternetExplorer tab ) : base(new IntPtr(tab.HWND)) {
            this.title = tab.LocationName + " - Internet Explorer";
            this.ie = tab;
            Console.Write(this.handle);
        }

        public override void SendToFront()
        {
            //IWebBrowserApp wb = (IWebBrowserApp)ie;
            //ie.Document.focus();
            //((mshtml.HTMLDocument) ie.Document).focus();
            base.SendToFront();
        }

    }
}
