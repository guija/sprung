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

        private List<Window> windows = null;
        private WindowsManager manager = null;
        private WindowsMatcher matcher = null;
        private SystemTray tray = null;

        const int MOD_ALT = 0x0001;
        const int MOD_CONTROL = 0x0002;
        const int MOD_SHIFT = 0x0004;
        const int WM_HOTKEY = 0x0312;

        /*
         * 
         * Initialize Jumper program.
         * 
         */
        public Sprung()
        {
            InitializeComponent();
            this.tray = new SystemTray();
            this.windows = new List<Window>();
            this.manager = new WindowsManager();
        }

        /*
         * 
         * Loading the Jumper program.
         * 
         */
        private void loadJumper(object sender, EventArgs e)
        {
            RegisterHotKey(this.Handle, 1, MOD_ALT, (int)Keys.Space);
        }

        /*
         * 
         * Closing the application.
         * 
         */
        private void exitJumper(object sender, EventArgs e)
        {
            UnregisterHotKey(this.Handle, 1);
            Application.Exit();
        }

        /*
         * 
         * Search for matching processes.
         * If field is empty, nothing happens.
         * 
         */
        private void searchBoxInput(object sender, EventArgs e)
        {
            String pattern = searchBox.Text;
            this.manager.loadProcesses();
            this.matcher = new WindowsMatcher(manager.getProcesses());
            showMatchedProcesses(matcher.getMatchedProcesses(pattern));
        }

        /*
         * 
         * Return the sorted process list.
         * 
         */
        private void showMatchedProcesses(List<Window> windows)
        {

            matchingBox.Columns.Clear();
            this.windows = windows;

            matchingBox.AutoGenerateColumns = false;
            var imageCol = new DataGridViewImageColumn();
            var titleCol = new DataGridViewTextBoxColumn();

            imageCol.DataPropertyName = "";
            titleCol.DataPropertyName = "Title";

            matchingBox.Columns.Add(imageCol);
            matchingBox.Columns.Add(titleCol);

            var dt = new DataTable();
            dt.Columns.Add("Photo", typeof(Image));
            dt.Columns.Add("Title");
            matchingBox.DataSource = dt;

            matchingBox.Columns[0].Width = 24;
            matchingBox.Columns[1].Width = 504;

            matchingBox.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            matchingBox.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;

            foreach (Window window in windows)
            {
                if (!(window.getProcessName() == "") && !(window.getProcessTitle() == "")) {
                    dt.Rows.Add(null, window.getProcessTitle());
                }
            }

            matchingBox.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            matchingBox.MultiSelect = false;
            matchingBox.ReadOnly = true;

            for(int i = 0; i < matchingBox.Rows.Count && i < windows.Count; i++) {
                matchingBox.Rows[i].Cells[0].Value = setIcon(windows[i].getProcess().MainModule.FileName);
            }

            // Mark the first entry as it is (hopefully) the process the user is looking for.
            if (windows.Any())
            {
                matchingBox.Rows[0].Selected = true;
            }
        }

        private Icon setIcon(String fileName)
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

        /*
         * 
         * Main method for managing messages and its functionalities.
         * 
         */
        protected override void WndProc(ref Message m)
        {
            if(m.Msg == WM_HOTKEY && (int)m.WParam == 1)
            {
                this.Visible = true;
                this.Opacity = 100;
                this.CenterToScreen();
                this.manager.forceWindowToFront(this.Handle);
                this.Activate();
                this.searchBox.Focus();
                this.searchBox.Text = "";
                this.manager.loadProcesses();
                showMatchedProcesses(manager.getProcesses());
            }

            base.WndProc(ref m);
        }

        /*
         * Import of RegisterHotKey and UnregisterHotKey
         * Necessary for managing the controls for calling this program.
         */
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /*
         * 
         * Defining the controls for key down, key up, enter and escape.
         * 
         */
        private void searchBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (this.matchingBox.Rows.Count == 0) return;

            if (e.KeyCode == Keys.Enter)
            {
                sendSelectedWindowToFront();
                this.Visible = false;
                e.Handled = true;
                e.SuppressKeyPress = true;
            }/*
            else if (e.KeyCode == Keys.Down && this.matchingBox.SelectedIndex < (this.matchingBox.Items.Count - 1))
            {
                this.matchingBox.SelectedIndex++;
            }
            else if (e.KeyCode == Keys.Up && this.matchingBox.Items.Count > 0 && this.matchingBox.SelectedIndex > 0)
            {
                this.matchingBox.SelectedIndex--;
            }
            else */if (e.KeyCode == Keys.Escape)
            {
                this.Visible = false;
                this.Opacity = 0;
            }
            else
            {
                // Todo: Don't do anything
            }
        }


        private void sendSelectedWindowToFront()
        {
            if (this.matchingBox.Rows.Count > 0)
            {
                //int selected = this.matchingBox.SelectedIndex;
                Window window = windows.ElementAt(0);
                this.Visible = false;
                this.Opacity = 0;
                this.manager.sendWindowToFront(window);
            }
        }

        private void searchBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Enter)) e.Handled = true;
        }
    }
}
