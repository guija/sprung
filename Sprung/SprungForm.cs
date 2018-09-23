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
using System.Collections.Concurrent;
using MethodTimer;
using Sprung.Windows;
using System.Text.RegularExpressions;

namespace Sprung
{
    public partial class SprungForm : Form
    {
        static Regex RegExWholeWord = new Regex(@"(\r\n|[^A-Za-z0-9_\r\n]+?|\w+?) *$", RegexOptions.Compiled);

        const int MOD_ALT = 0x0001;
        const int MOD_CONTROL = 0x0002;
        const int MOD_SHIFT = 0x0004;
        const int WM_HOTKEY = 0x0312;
        const int HSHELL_WINDOWCREATED = 0x0001;
        const int HSHELL_WINDOWDESTROYED = 0x0002;
        const int HSHELL_WINDOWACTIVATED = 14;
        const int HOTKEY_LIST_WINDOWS = 0x0001;
        const int HOTKEY_LIST_WINDOWS_WITH_TABS = 0x0002;
        const uint WINEVENT_OUTOFCONTEXT = 0;
        const uint EVENT_SYSTEM_FOREGROUND = 3;

        private WindowManager windowManager = null;
        private SystemTray tray = null;
        private WindowMatcher windowMatcher = null;
        private Window mainWindow = null;
        private Settings settings = null;
        private List<Window> cachedWindows = null;
        private Window lastUsedWindow = null;
        private volatile bool inClosingProcess = false;
        private readonly int windowMessageNotifyShellHook;
        private ConcurrentDictionary<IntPtr, bool> closedWindowHandles = null;
        private WindowLastUsageComparer windowLastUsageComparer = null;

        private const string TabServiceHost = "localhost";
        private const int TabServicePort = 8212;
        private NancyHost tabService = null;

        WinEventDelegate handleActivatedWindowEventDelegate = null;
        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        public SprungForm()
        {
            InitializeComponent();
            this.settings = new Settings();
            this.tray = new SystemTray(settings);
            this.windowManager = WindowManager.GetInstance();
            this.windowMatcher = new WindowMatcher(this.windowManager);
            this.Visible = false;
            this.Opacity = 0;
            this.ControlBox = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.mainWindow = new Window(Handle);
            this.Deactivate += DeactivateCallback;
            this.KeyPreview = true;
            this.KeyDown += GlobalKeyDown;
            this.windowListBox.Sprung = this;
            this.windowListBox.ShowScrollbar = false;
            this.windowLastUsageComparer = new WindowLastUsageComparer();
            this.searchBox.TextChanged += new System.EventHandler(this.InputChangedCallback);
            this.searchBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SearchBoxKeyDown);
            this.searchBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SearchBoxKeyPress);

            closedWindowHandles = new ConcurrentDictionary<IntPtr, bool>();

            windowMessageNotifyShellHook = RegisterWindowMessage("SHELLHOOK");
            RegisterShellHookWindow(Handle);

            handleActivatedWindowEventDelegate = new WinEventDelegate(HandleActivatedWindowEvent);
            IntPtr m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, handleActivatedWindowEventDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
        }

        // Hide from ALT-TAB (is not hidden when border is border less)
        protected override CreateParams CreateParams
        {
            get
            {
                // Turn on WS_EX_TOOLWINDOW style bit
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80;
                return cp;
            }
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
            UnregisterHotKey(Handle, HOTKEY_LIST_WINDOWS);
            UnregisterHotKey(Handle, HOTKEY_LIST_WINDOWS_WITH_TABS);
            DeregisterShellHookWindow(Handle);
            Application.Exit();
        }

        private void InputChangedCallback(object sender, EventArgs e)
        {
            String pattern = searchBox.Text;
            windowManager.UpdateTitles();
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

            closedWindowHandles.Clear();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == windowMessageNotifyShellHook)
            {
                int code = m.WParam.ToInt32();
                IntPtr handle = m.LParam;

                if (code == HSHELL_WINDOWCREATED)
                {
                    HandleCreatedWindowEvent(handle);
                }
                else if (code == HSHELL_WINDOWDESTROYED)
                {
                    HandleDestroyedWindowEvent(handle);
                }
            }

            if (m.Msg == WM_HOTKEY)
            {
                int shortcutCode = (int)m.WParam;
                HandleGlobalHotkey(shortcutCode);
            }

            base.WndProc(ref m);
        }

        private void HandleActivatedWindowEvent(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            Window window = null;
            
            if (windowManager.Windows.TryGetValue(hwnd, out window))
            {
                window.LastActivation = DateTime.Now;
            }
        }

        private void HandleCreatedWindowEvent(IntPtr handle)
        {
            Window window = new Window(handle);
            windowManager.Windows.TryAdd(window.Handle, window);
        }

        private void HandleDestroyedWindowEvent(IntPtr handle)
        {
            Window closedWindow = null;
            windowManager.Windows.TryRemove(handle, out closedWindow);

            IntPtr closedWindowHandle = handle;

            if (!closedWindowHandles.ContainsKey(closedWindowHandle))
            {
                return;
            }

            bool contains = false; // Can be ignore, we do not need the value to be retrieved.
            while (!closedWindowHandles.TryRemove(closedWindowHandle, out contains)) ;

            // Remove from list
            for (int i = 0; i <= windowListBox.Items.Count; i++)
            {
                var window = windowListBox.Items[i] as Window;

                if (window.Handle != closedWindowHandle)
                {
                    continue;
                }

                windowListBox.Items.RemoveAt(i);

                if (windowListBox.SelectedIndex >= 0 && windowListBox.SelectedIndex < i)
                {
                    // Nothing to do
                }
                else
                {
                    if (windowListBox.Items.Count == 0)
                    {
                        windowListBox.SelectedIndex = -1;
                    }
                    else
                    {
                        // Select previous item
                        windowListBox.SelectedIndex = Math.Max(0, i - 1);
                    }
                }

                break;
            }
        }

        private void HandleGlobalHotkey(int shortcutCode)
        {
            if (shortcutCode != HOTKEY_LIST_WINDOWS && shortcutCode != HOTKEY_LIST_WINDOWS_WITH_TABS)
            {
                return;
            }

            this.Visible = true;
            this.Opacity = 100;
            this.CenterToScreen();
            this.mainWindow.SendToFront();
            this.Activate();
            this.searchBox.Focus();
            this.searchBox.Text = "";

            if (shortcutCode == HOTKEY_LIST_WINDOWS)
            {
                // this.cachedWindows = windowManager.GetWindows();
                this.cachedWindows = windowManager.Windows.Values.ToList();
            }
            else if (shortcutCode == HOTKEY_LIST_WINDOWS_WITH_TABS)
            {
                // this.cachedWindows = windowManager.GetWindowsWithTabs();
                this.cachedWindows = windowManager.Windows.Values.ToList();
            }
            else
            {
                Debug.WriteLine("Unknown key combination, should never happend");
            }

            this.cachedWindows.Sort(windowLastUsageComparer);
            ShowProcesses(this.cachedWindows);
            lastUsedWindow = this.cachedWindows.FirstOrDefault();
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

                // Mark the event as handled, otherwise the search input
                // will be modified and the list will be reloaded
                e.Handled = true;
            }
        }

        private void SearchBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) Keys.Enter || e.KeyChar == (char) Keys.Escape)
            {
                e.Handled = true;
            }
            else if (e.KeyChar == 127)
            {
                // CTRL + Backspace
                var m = RegExWholeWord.Match(searchBox.Text, 0, searchBox.SelectionStart);

                if (m.Success)
                {
                    searchBox.Text = searchBox.Text.Remove(m.Index, m.Length);
                    searchBox.SelectionStart = m.Index;
                }

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

            // Add the handle of the window that we want to close to a concurrent dictionary
            // so that we know which windows we actually want to close. As soon as the
            // destroyed event is intercepted we know that the window got closed
            // and we can remove it from the list. We use a dictionary instead of a HashSet
            // because C# does not provide a concurrent hash set.
            closedWindowHandles[selectedWindow.Handle] = true;
            selectedWindow.Close();
            this.mainWindow.SendToFront();
            inClosingProcess = false;
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
            
            this.HideBox();
        }

        [DllImport("user32.dll", EntryPoint = "RegisterWindowMessageA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int RegisterWindowMessage(string lpString);

        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int RegisterShellHookWindow(IntPtr hWnd);

        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int DeregisterShellHookWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);
    }
}
