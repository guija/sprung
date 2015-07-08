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

        private WindowManager windowManager;
        private SystemTray tray;
        private WindowMatcher windowMatcher;

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
            this.matchingBox.Columns.Clear();
            this.matchingBox.AutoGenerateColumns = false;
            this.matchingBox.AllowUserToAddRows = false;
            this.matchingBox.CellBorderStyle = DataGridViewCellBorderStyle.None;
            this.matchingBox.ColumnHeadersVisible = false;
            this.matchingBox.RowHeadersVisible = false;

            SprungLayout layout = new SprungLayout(matchingBox);
            layout.addImageColumn("", 32);
            layout.addTextColumn("Title", 537);
            layout.setNotSortable(true);
            layout.addProcesses(windows);
            layout.setSelectionMode(DataGridViewSelectionMode.FullRowSelect);
            layout.setScrolls(ScrollBars.None);
            layout.setResizeable(false);
            layout.setMultiSelect(false);
            layout.setReadOnly(true);

            this.matchingBox.DataSource = layout.getTable();

            for(int i = 0; i < matchingBox.Rows.Count && i < windows.Count; i++) {
                this.matchingBox.Rows[i].Cells[0].Value = resizeIcon(windows[i].getProcess().MainModule.FileName);
                this.matchingBox.Rows[i].Height = 32;
            }

            if (windows.Any())
            {
                this.matchingBox.Rows[0].Selected = true;
            }
        }

        private Icon resizeIcon(String fileName)
        {
            Size iconSize = new Size(24, 24);
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

        private void searchBoxKeyControl(object sender, KeyEventArgs e)
        {

            if (this.matchingBox.Rows.Count == 0) return;

            int selected = this.matchingBox.CurrentRow.Index;

            if (e.KeyCode == Keys.Enter)
            {
                sendSelectedWindowToFront();
                this.Visible = false;
                e.Handled = true;
                e.SuppressKeyPress = true;
            }else if (e.KeyCode == Keys.Down && this.matchingBox.CurrentCell.RowIndex < (this.matchingBox.Rows.Count - 1))
            {
                this.matchingBox.CurrentCell = this.matchingBox.Rows[selected + 1].Cells[0];
            }else if (e.KeyCode == Keys.Up && this.matchingBox.Rows.Count > 0 && this.matchingBox.CurrentCell.RowIndex > 0)
            {
                this.matchingBox.CurrentCell = this.matchingBox.Rows[selected - 1].Cells[0];
            }else if (e.KeyCode == Keys.Escape)
            {
                this.Visible = false;
                this.Opacity = 0;
            }
        }


        private void sendSelectedWindowToFront()
        {
            if (this.matchingBox.Rows.Count > 0)
            {
                int selected = this.matchingBox.CurrentRow.Index;
                Window window = windowManager.getProcesses()[selected];
                if(window != null) Console.WriteLine(window.getProcessTitle());
                this.Visible = false;
                this.Opacity = 0;
                this.windowManager.sendWindowToFront(window);
            }
        }

        private void searchBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Enter)) e.Handled = true;
        }
    }
}
