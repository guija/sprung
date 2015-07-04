using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Jumper.Properties;

namespace Jumper
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

            exit.Click += new EventHandler(exitJumper);

            symbol.ContextMenuStrip = cms;
        }

        /*
         * 
         * Closing the application.
         * 
         */
        private void exitJumper(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}
