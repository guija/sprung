using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sprung.Properties;
using System.Diagnostics;

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
            ToolStripMenuItem settings = new ToolStripMenuItem();
            ToolStripMenuItem help = new ToolStripMenuItem();
            ToolStripMenuItem reloadSettings = new ToolStripMenuItem("Reload Settings");

            symbol.Icon = Resources.spring;
            symbol.Visible = true;

            help.Text = "Help";
            settings.Text = "Settings";
            exit.Text = "Exit";

            cms.Items.Add(help);
            cms.Items.Add(settings);
            cms.Items.Add(reloadSettings);
            cms.Items.Add(exit);

            help.Click += HelpCallback;
            exit.Click += new EventHandler(this.exit);
            reloadSettings.Click += new EventHandler(this.reloadSettings);
            settings.Click += SettingsCallback;

            symbol.ContextMenuStrip = cms;
        }

        private void SettingsCallback(object sender, EventArgs e)
        {
            Process.Start("settings.json");
        }

        private void HelpCallback(object sender, EventArgs e)
        {
            Process.Start("https://github.com/guija/sprung/");
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
