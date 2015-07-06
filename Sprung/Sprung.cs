using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Sprung
{
    public partial class Sprung : Form
    {

        private WindowManager windowManager = null;
        private SystemTray tray = null;
        private WindowMatcher windowMatcher = null;
        private Window mainWindow = null;

        const int MOD_ALT = 0x0001;
        const int MOD_CONTROL = 0x0002;
        const int MOD_SHIFT = 0x0004;
        const int WM_HOTKEY = 0x0312;

        public Sprung()
        {
            InitializeComponent();
            this.tray = new SystemTray();
            this.windowManager = new WindowManager();
            this.windowMatcher = new WindowMatcher(this.windowManager);
            this.Visible = false;
            this.Opacity = 0;
            this.ControlBox = false;
            this.ShowInTaskbar = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.mainWindow = new Window(this.Handle);
        }

        private void loadCallback(object sender, EventArgs e)
        {
            RegisterHotKey(this.Handle, 1, MOD_ALT, (int)Keys.Space);
        }

        private void exitCallback(object sender, EventArgs e)
        {
            UnregisterHotKey(this.Handle, 1);
            Application.Exit();
        }

        private void inputChangedCallback(object sender, EventArgs e)
        {
            String pattern = searchBox.Text;
            showProcesses(windowMatcher.getMatchedProcesses(pattern));
        }

        private void showProcesses(List<Window> windows)
        {
            Console.WriteLine("begin showProcessed");
            Console.Out.Flush();
            windowListBox.Items.Clear();
            foreach (Window w in windows)
            {
                windowListBox.Items.Add(w);
            }
            Console.WriteLine("end showProcessed");
            Console.Out.Flush();
            if (windowListBox.Items.Count > 0)
            {
                windowListBox.SelectedIndex = 0;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if(m.Msg == WM_HOTKEY && (int)m.WParam == 1)
            {
                this.Visible = true;
                this.Opacity = 100;
                this.CenterToScreen();
                this.mainWindow.SendToFront();
                this.Activate();
                this.searchBox.Focus();
                this.searchBox.Text = "";
                showProcesses(windowManager.getProcesses());
            }
            base.WndProc(ref m);
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private void searchBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (this.windowListBox.Items.Count == 0) return;
            if (e.KeyCode == Keys.Enter)
            {
                this.Visible = false;
                sendSelectedWindowToFront();
            } 
            else if (e.KeyCode == Keys.Down && this.windowListBox.SelectedIndex < (this.windowListBox.Items.Count - 1))
            {
                this.windowListBox.SelectedIndex++;
            }
            else if (e.KeyCode == Keys.Up && this.windowListBox.Items.Count > 0 && this.windowListBox.SelectedIndex > 0)
            {
                this.windowListBox.SelectedIndex--;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Visible = false;
                this.Opacity = 0;
            }
        }

        private void searchBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) Keys.Enter || e.KeyChar == (char) Keys.Escape)
            {
                e.Handled = true;
            }
        }


        private void sendSelectedWindowToFront()
        {
            if (this.windowListBox.Items.Count > 0)
            {
                // hide main window
                this.Visible = false;
                this.Opacity = 0;
                // show window that was selected
                int selectedIndex = this.windowListBox.SelectedIndex;
                selectedIndex = selectedIndex == -1 ? 0 : selectedIndex;
                Window selectedWindow = (Window) this.windowListBox.Items[selectedIndex];
                selectedWindow.SendToFront();
            }
        }
    }
}
