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
using System.Threading;
using Nancy.Hosting.Self;

namespace Sprung
{
    public partial class Sprung : Form
    {
        private WindowManager windowManager = null;
        private SystemTray tray = null;
        private WindowMatcher windowMatcher = null;
        private Window mainWindow = null;
        private Settings settings = null;
        private List<Window> cachedWindows = null;
        private Window lastUsedWindow = null;
        private volatile bool inClosingProcess = false;

        private const string TabServiceHost = "localhost";
        private const int TabServicePort = 8212;
        private NancyHost tabService = null;

        const int MOD_ALT = 0x0001;
        const int MOD_CONTROL = 0x0002;
        const int MOD_SHIFT = 0x0004;
        const int WM_HOTKEY = 0x0312;

        public Sprung()
        {
            InitializeComponent();
            this.settings = new Settings();
            this.tray = new SystemTray(settings);
            this.windowManager = WindowManager.GetInstance();
            this.windowMatcher = new WindowMatcher(this.windowManager);
            this.Visible = false;
            this.Opacity = 0;
            this.ControlBox = false;
            this.ShowInTaskbar = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.mainWindow = new Window(this.Handle);
            this.Deactivate += DeactivateCallback;
            this.KeyPreview = true;
            this.KeyDown += GlobalKeyDown;
            this.windowListBox.Sprung = this;

            // new Thread(StartTabService).Start();
            // StartTabService();
        }

        private void StartTabService()
        {
            Debug.WriteLine("StartTabService");

            HostConfiguration hostConfiguration = new HostConfiguration()
            {
                UrlReservations = new UrlReservations { CreateAutomatically = true }
            };

            Uri uri = new Uri($"http://{TabServiceHost}:{TabServicePort}");

            this.tabService = new NancyHost(hostConfiguration, uri);
            this.tabService.Start();
        }

        private void GlobalKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Debug.WriteLine("global key");
                HideBox();

                if (lastUsedWindow != null)
                {
                    lastUsedWindow.SendToFront();
                    lastUsedWindow = null;
                }
            }
        }

        private bool IsSearchBoxInForeground()
        {
            return GetForegroundWindow() == this.Handle;
        }

        private void LoadCallback(object sender, EventArgs e)
        {
            InitShortcuts();
        }

        // TODO Handle shortcut with actions etc. see keytrack project
        public void InitShortcuts()
        {
            // Init normal shortcut
            int modifiers = (int)(Keys.Modifiers & settings.Shortcut);
            int keyCode = (int)(Keys.KeyCode & settings.Shortcut);
            int transformedModifier = 0x0;
            if ((modifiers & (int)Keys.Control) > 0) transformedModifier |= MOD_CONTROL;
            if ((modifiers & (int)Keys.Alt) > 0) transformedModifier |= MOD_ALT;
            if ((modifiers & (int)Keys.Shift) > 0) transformedModifier |= MOD_SHIFT; ;
            RegisterHotKey(this.Handle, 1, transformedModifier, keyCode);

            // Init shortcut with tabs included
            modifiers = (int)(Keys.Modifiers & settings.ShortcutShowTabs);
            keyCode = (int)(Keys.KeyCode & settings.Shortcut);
            transformedModifier = 0x0;
            if ((modifiers & (int)Keys.Control) > 0) transformedModifier |= MOD_CONTROL;
            if ((modifiers & (int)Keys.Alt) > 0) transformedModifier |= MOD_ALT;
            if ((modifiers & (int)Keys.Shift) > 0) transformedModifier |= MOD_SHIFT; ;
            RegisterHotKey(this.Handle, 2, transformedModifier, keyCode);
        }

        private void ExitCallback(object sender, EventArgs e)
        {
            UnregisterHotKey(this.Handle, 1);
            UnregisterHotKey(this.Handle, 2);
            Application.Exit();
        }

        private void InputChangedCallback(object sender, EventArgs e)
        {
            String pattern = searchBox.Text;
            ShowProcesses(windowMatcher.match(pattern, cachedWindows));
        }

        private void ShowProcesses(List<Window> windows)
        {
            windowListBox.BeginUpdate();
            windowListBox.Items.Clear();
            windowListBox.Items.AddRange(windows.ToArray());
            windowListBox.EndUpdate();

            if (windowListBox.Items.Count > 0)
            {
                windowListBox.SelectedIndex = 0;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if(m.Msg == WM_HOTKEY && ((int) m.WParam == 1 || (int)m.WParam == 2))
            {
                this.Visible = true;
                this.Opacity = 100;
                this.CenterToScreen();
                this.mainWindow.SendToFront();
                this.Activate();
                this.searchBox.Focus();
                this.searchBox.Text = "";

                if ((int)m.WParam == 1)
                {
                    this.cachedWindows = windowManager.GetWindows();
                }
                else if ((int)m.WParam == 2)
                {
                    this.cachedWindows = windowManager.GetWindowsWithTabs();
                }
                else
                {
                    Debug.WriteLine("Unknown key combination, should never happend");
                }
                
                ShowProcesses(this.cachedWindows);
                lastUsedWindow = this.cachedWindows.FirstOrDefault();
            }

            base.WndProc(ref m);
        }

        private void SearchBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (this.windowListBox.Items.Count == 0) return;
            if (e.KeyCode == Keys.Enter)
            {
                HideBox();
                SendSelectedWindowToFront();
            } 
            else if (e.KeyCode == Keys.Down && this.windowListBox.SelectedIndex < (this.windowListBox.Items.Count - 1))
            {
                this.windowListBox.SelectedIndex++;
            }
            else if (e.KeyCode == Keys.Up && this.windowListBox.Items.Count > 0 && this.windowListBox.SelectedIndex > 0)
            {
                this.windowListBox.SelectedIndex--;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                CloseSelectedWindow();
            }
        }

        private void SearchBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) Keys.Enter || e.KeyChar == (char) Keys.Escape)
            {
                e.Handled = true;
            }
        }

        private void HideBox()
        {
            // hide main window
            this.Visible = true;
            this.Opacity = 0;
        }

        private void ShowBox()
        {
            // hide main window
            this.Visible = true;
            this.Opacity = 100;
        }

        public void CloseSelectedWindow()
        {
            Window selectedWindow = GetSelectedWindow();

            if (selectedWindow == null)
            {
                return;
            }

            // Synchronisation here is needed since the loose focus event would hide the 
            // window after sending it to the front (The deactivate event is triggered / processed
            // after Close and Before SendToFront.
            // This happens when we try to close a window but that window e.g. opens
            // a dialog on closing ("e.g. are you sure you want to close this file?").
            // Then the Sprung search window looses focus to the window to be closed and the
            // deactivate event is triggered.
            // We use a volatile Variable instead of a lock because "inClosingProcess" we dont
            // want to synchronize, we rather want the deactivate event to be ignored completely
            // and not be processed afterwards. See the usage of the variable "inClosingProcess"
            // in the event handler.
            inClosingProcess = true;
            bool isClosed = selectedWindow.Close();
            this.mainWindow.SendToFront();
            inClosingProcess = false;

            if (isClosed)
            {
                // Remove the item from the list
                int indexOfClosedWindow = GetSelectedWindowIndex() ?? 0;
                windowListBox.Items.RemoveAt(indexOfClosedWindow);

                // Select previous item
                windowListBox.SelectedIndex = Math.Max(0, indexOfClosedWindow - 1);
            }
        }

        public void SendSelectedWindowToFront()
        {
            Window selectedWindow = GetSelectedWindow();

            if (selectedWindow == null)
            {
                return;
            }

            selectedWindow.SendToFront();
        }

        private int? GetSelectedWindowIndex()
        {
            if (this.windowListBox.Items.Count > 0)
            {
                // If no entry was selected yet but there are items in the list
                // then take the first one
                int selectedIndex = Math.Max(0, this.windowListBox.SelectedIndex);
                return selectedIndex;
            }

            return null;
        }

        private Window GetSelectedWindow()
        {
            int? selectedIndex = GetSelectedWindowIndex();

            if (!selectedIndex.HasValue)
            {
                return null;
            }

            Window selectedWindow = (Window)this.windowListBox.Items[selectedIndex.Value];

            return selectedWindow;
        }

        // Hides window when the main windows loses the focus
        private void DeactivateCallback(object sender, EventArgs e)
        {
            if (inClosingProcess)
            {
                return;
            }
            
            Debug.WriteLine("lost focus");
            this.HideBox();
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();
    }
}
