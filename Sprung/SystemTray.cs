using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sprung.Properties;

namespace Sprung
{
    class SystemTray
    {

        private NotifyIcon symbol;
        private ContextMenuStrip cms;
        private Settings settings;

        public SystemTray(Settings settings)
        {
            this.symbol = new NotifyIcon();
            this.cms = new ContextMenuStrip();
            this.settings = settings;
            InitializeSystemTray();
        }

        private void InitializeSystemTray()
        {
            ToolStripMenuItem exit = new ToolStripMenuItem();
            ToolStripMenuItem options = new ToolStripMenuItem();
            ToolStripMenuItem help = new ToolStripMenuItem();
            ToolStripMenuItem reloadSettings = new ToolStripMenuItem("Reload Settings");

            symbol.Icon = Resources.spring;
            symbol.Visible = true;

            help.Text = "Hilfe";
            options.Text = "Einstellungen";
            exit.Text = "Beenden";

            cms.Items.Add(help);
            cms.Items.Add(options);
            cms.Items.Add(reloadSettings);
            cms.Items.Add(exit);
            

            exit.Click += new EventHandler(this.exit);
            reloadSettings.Click += new EventHandler(this.reloadSettings);

            symbol.ContextMenuStrip = cms;
        }

        private void exit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void reloadSettings(object sender, EventArgs e)
        {
            settings.load();
        }

    }
}
