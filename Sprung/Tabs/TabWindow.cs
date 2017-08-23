using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sprung.Tabs
{
    public class TabWindow : Window
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("windowId")]
        public int WindowId { get; set; }

        [JsonProperty("isCurrent")]
        public bool IsCurrent { get; set; }

        public int CurrentTabIndex { get; set; }

        private const string NEXT_TAB_KEYS = "^{PGUP}";

        private const string PREVIOUS_TAB_KEYS = "^{PGDN}";

        // Go to the correct tab inside a window handle by using CTRL+PGUP, CTRL+PGDN
        // Hack, but works on all browser, no UI inspection needed.
        public override void SendToFront()
        {
            base.SendToFront();
            int changeVector = Index - CurrentTabIndex;
            int tabChanges = Math.Abs(changeVector);
            int direction = Math.Sign(changeVector);

            for (int i = 0; i < tabChanges; i++)
            {
                SendKeys.Send(direction < 0 ? NEXT_TAB_KEYS : PREVIOUS_TAB_KEYS);
            }
        }
    }
}