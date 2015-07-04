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

        public SystemTray()
        {
            this.symbol = new NotifyIcon();
            this.cms = new ContextMenuStrip();
            InitializeSystemTray();
        }

        private void InitializeSystemTray()
        {
            ToolStripMenuItem exit = new ToolStripMenuItem();
            ToolStripMenuItem options = new ToolStripMenuItem();
            ToolStripMenuItem help = new ToolStripMenuItem();

            symbol.Icon = Resources.spring;
            symbol.Visible = true;

            help.Text = "Hilfe";
            options.Text = "Einstellungen";
            exit.Text = "Beenden";

            cms.Items.Add(help);
            cms.Items.Add(options);
            cms.Items.Add(exit);

            exit.Click += new EventHandler(this.exit);

            symbol.ContextMenuStrip = cms;
        }

        private void exit(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}
