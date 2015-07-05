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
            windowsListBox.Items.Clear();
            foreach (Window w in windows)
            {
                windowsListBox.Items.Add(w);
            }
            if (windowsListBox.Items.Count > 0)
            {
                windowsListBox.SelectedIndex = 0;
            }
        }

        private Icon resizeIcon(String fileName)
        {
            Size iconSize = SystemInformation.SmallIconSize;
            Bitmap bitmap = new Bitmap(iconSize.Width, iconSize.Height);
            Icon ico = Icon.ExtractAssociatedIcon(fileName);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(ico.ToBitmap(), new Rectangle(Point.Empty, iconSize));
            }

            return Icon.FromHandle(bitmap.GetHicon());
        }

        protected override void WndProc(ref Message m)
        {
            if(m.Msg == WM_HOTKEY && (int)m.WParam == 1)
            {
                this.Visible = true;
                this.Opacity = 100;
                this.CenterToScreen();
                this.windowManager.forceWindowToFront(this.Handle);
                this.Activate();
                this.searchBox.Focus();
                this.searchBox.Text = "";
                this.windowManager.loadProcesses();
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
            if (this.windowsListBox.Items.Count == 0) return;
            if (e.KeyCode == Keys.Enter)
            {
                sendSelectedWindowToFront();
                this.Visible = false;
                e.Handled = true;
                e.SuppressKeyPress = true;
            } 
            else if (e.KeyCode == Keys.Down && this.windowsListBox.SelectedIndex < (this.windowsListBox.Items.Count - 1))
            {
                this.windowsListBox.SelectedIndex++;
            }
            else if (e.KeyCode == Keys.Up && this.windowsListBox.Items.Count > 0 && this.windowsListBox.SelectedIndex > 0)
            {
                this.windowsListBox.SelectedIndex--;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Visible = false;
                this.Opacity = 0;
            }
        }


        private void sendSelectedWindowToFront()
        {
            if (this.windowsListBox.Items.Count > 0)
            {
                int selectedIndex = this.windowsListBox.SelectedIndex;
                selectedIndex = selectedIndex == -1 ? 0 : selectedIndex;
                Window selectedWindow = (Window) this.windowsListBox.Items[selectedIndex];
                this.Visible = false;
                this.Opacity = 0;
                this.windowManager.sentToFront(selectedWindow);
            }
        }

        private void searchBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            // Fix so that no error sound is played when exit key is
            // pressed while the input box is focused because for 
            // us it's a regular action (hide window).
            if (e.KeyChar == Convert.ToChar(Keys.Enter)) e.Handled = true;
        }
    }
}
