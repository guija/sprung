using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sprung.Properties;
using System.Diagnostics;
using System.ComponentModel;

namespace Sprung
{
    class SystemTray
    {
        private NotifyIcon symbol;
        private ContextMenuStrip contextMenuStrip;
        private Settings settings;

        public SystemTray(Settings settings)
        {
            this.symbol = new NotifyIcon();
            this.contextMenuStrip = new ContextMenuStrip();
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

            contextMenuStrip.Items.Add(help);
            contextMenuStrip.Items.Add(settings);
            contextMenuStrip.Items.Add(reloadSettings);
            contextMenuStrip.Items.Add(exit);

            help.Click += HelpCallback;
            exit.Click += new EventHandler(this.ExitCallback);
            reloadSettings.Click += new EventHandler(this.ReloadSettings);
            settings.Click += SettingsCallback;

            symbol.ContextMenuStrip = contextMenuStrip;
        }

        private void SettingsCallback(object sender, EventArgs e)
        {
            string sprungFolder = AppDomain.CurrentDomain.BaseDirectory;
            string settingsFile = sprungFolder + "settings.json";
            string editorExecutable = @"C:\Windows\Notepad.exe";

            ProcessStartInfo info = new ProcessStartInfo(editorExecutable, settingsFile);
            info.UseShellExecute = true;
            info.Verb = "runas";

            try
            {
                Process.Start(info);
            }
            catch (Win32Exception exception)
            {
                const int ERROR_CANCELLED = 1223; // 1223 --> The operation was canceled by the user.
                if (exception.NativeErrorCode == ERROR_CANCELLED)
                {
                    MessageBox.Show("You only can edit the Sprung settings.json configuration file as a administrator");
                }
                else
                {
                    throw;
                }
            }
        }

        private void HelpCallback(object sender, EventArgs e)
        {
            Process.Start("https://github.com/guija/sprung/");
        }

        private void ExitCallback(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ReloadSettings(object sender, EventArgs e)
        {
            settings.load();
        }
    }
}
