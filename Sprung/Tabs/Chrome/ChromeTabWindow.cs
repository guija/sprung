using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sprung.Tabs.Chrome
{
    public class ChromeTabWindow : Window
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("windowId")]
        public int WindowId { get; set; }

        [JsonProperty("isCurrent")]
        public bool IsCurrent { get; set; }

        public static List<ChromeTabWindow> Tabs { get; set; } = new List<ChromeTabWindow>();

        public static object TabsLock { get; set; } = new object();

        private int? currentTabIndex = null;
        
        public ChromeTabWindow()
        {
        }

        public override void SendToFront()
        {
            base.SendToFront();
            int changeVector = Index - currentTabIndex.Value;
            int tabChanges = Math.Abs(changeVector);
            int direction = Math.Sign(changeVector);
            for (int i = 0; i < tabChanges; i++)
            {
                SendKeys.Send(direction < 0 ? "^{PGUP}" : "^{PGDN}");
            }
        }
    }
}