using System;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using Newtonsoft.Json;
using MethodTimer;

namespace Sprung.Windows
{
    public class Window : IComparable<Window>
    {
        protected const uint SW_SHOWMAXIMIZED = 3;
        protected const uint SW_SHOW = 5;
        protected const uint SW_RESTORE = 9;
        protected const uint WM_CLOSE = 0x0010;
        protected const int WINDOW_TITLE_MAX_CHARS = 255;
        public const int IconSize = 30;

        public IntPtr Handle { get; set; }

        public string ProcessName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        [JsonProperty("title")]
        public string TitleRaw { get; set; }

        public Process Process { get; set; }

        protected Icon icon = null;

        protected Image iconImage = null;

        public int MatchingPriority { get; set; } = 0;

        public int MatchingGroups { get; set; } = 0;

        public DateTime LastActivation { get; set; } = DateTime.Now;

        public Window()
        {
        }

        public Window(IntPtr handle)
        {
            if (handle.Equals(IntPtr.Zero))
            {
                throw new Exception("Invalid arguments passed to constructor in class \"Process\"");
            }

            this.Handle = handle;
            int processId = GetWindowProcessId(handle.ToInt32());
            this.Process = Process.GetProcessById(processId);
            this.ProcessName = this.Process.ProcessName;
            UpdateTitle();
        }

        public virtual void UpdateTitle()
        {
            StringBuilder stringBuilderTitle = new StringBuilder(WINDOW_TITLE_MAX_CHARS);
            stringBuilderTitle.Length = _GetWindowText(this.Handle, stringBuilderTitle, stringBuilderTitle.Capacity + 1);
            this.Title = stringBuilderTitle.ToString();

            TitleRaw = this.Title;

            // Add process name to title if it is not in the window name yet
            if (!this.Title.ToLower().Contains(this.ProcessName.ToLower()))
            {
                this.Title += String.Format(" - {0}", this.ProcessName);
            }
        }

        public virtual void SendToFront()
        {
            uint foreThread = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
            uint appThread = GetCurrentThreadId();
            AttachThreadInput(foreThread, appThread, true);
            Application.DoEvents();
            WINDOWPLACEMENT placement = GetPlacement(Handle);
            if (placement.showCmd == ShowWindowCommands.Maximized) ShowWindow(Handle, SW_SHOWMAXIMIZED);
            else if (placement.showCmd == ShowWindowCommands.Minimized) ShowWindow(Handle, SW_RESTORE);
            else if (placement.showCmd == ShowWindowCommands.Normal) ShowWindow(Handle, SW_SHOW);
            else ShowWindow(Handle, SW_SHOW);
            BringWindowToTop(Handle);
            SetForegroundWindow(Handle.ToInt32());
            AttachThreadInput(foreThread, appThread, false);
        }

        // Returns true if closing was successfull
        public void Close()
        {
            SendMessage(Handle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

        public Int32 GetWindowProcessId(Int32 handle)
        {
            Int32 pointer = 1;
            GetWindowThreadProcessId(handle, out pointer);
            return pointer;
        }

        public int CompareTo(Window other)
        {
            return (MatchingPriority < other.MatchingPriority) 
                ? 1 
                : (MatchingPriority > other.MatchingPriority) 
                    ? -1 
                    : (MatchingGroups < other.MatchingGroups) 
                        ? -1 
                        : (MatchingGroups > other.MatchingGroups) ? 1 : 0;
        }

        public Image GetIconImage()
        {
            Icon icon = GetIcon();

            if (icon == null)
            {
                return null;
            }

            return (iconImage = (Image)new Bitmap(icon.ToBitmap(), new Size(IconSize, IconSize)));
        }

        public Icon GetIcon()
        {
            if (icon != null)
            {
                return icon;
            }

            try
            {
                String processFileName = Process.MainModule.FileName;
                return (icon = Icon.ExtractAssociatedIcon(processFileName));
            }
            catch (Exception)
            {
                return (icon = SystemIcons.Application);
            }
        }

        private delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        [DllImport("user32")]
        private static extern UInt32 GetWindowThreadProcessId(Int32 hWnd, out Int32 lpdwProcessId);

        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetWindowText", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int _GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

        [DllImport("user32.dll")]
        static extern bool IsWindow(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        // window placement test
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public ShowWindowCommands showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        internal enum ShowWindowCommands : int
        {
            Hide = 0,
            Normal = 1,
            Minimized = 2,
            Maximized = 3,
        }

        private static WINDOWPLACEMENT GetPlacement(IntPtr hwnd)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            GetWindowPlacement(hwnd, ref placement);
            return placement;
        }
    }
}
