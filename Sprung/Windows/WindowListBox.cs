using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sprung.Windows
{
    public class WindowListBox : ListBox
    {
        private const int newItemHeight = 36;
        private int iconMargin = 0;
        private const string DefaultFontName = "Arial";
        private Font titleFont = new Font(DefaultFontName, 10.0f, FontStyle.Regular);
        private Font processFont = new Font(DefaultFontName, 9.0f, FontStyle.Regular);
        private TextFormatFlags titleFlags = TextFormatFlags.Left | TextFormatFlags.Bottom;
        private TextFormatFlags processFlags = TextFormatFlags.Left | TextFormatFlags.Top;
        private Color processColor = Color.FromArgb(110, 110, 110);

        public Sprung Sprung { get; set; }

        public WindowListBox()
        {
            this.DrawMode = DrawMode.OwnerDrawVariable;
            iconMargin = (newItemHeight - Window.IconSize) / 2;
            this.ItemHeight = newItemHeight;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            Sprung.SendSelectedWindowToFront();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            // Check if no windows selected yet
            if(e.Index < 0 || e.Index >= Items.Count)
            {
                return;
            }
            
            this.ItemHeight = newItemHeight;
            Window window = Items[e.Index] as Window;

            // Title fixes
            String title = window.TitleRaw.Replace("&", "&&");
            String processName = window.ProcessName.Replace("&", "&&");

            e.DrawBackground();

            // Draw icon
            Image iconImage = window.GetIconImage();

            if (iconImage != null)
            {
                e.Graphics.DrawImage(iconImage, iconMargin, e.Bounds.Y + iconMargin);
            }

            // Draw window title
            Rectangle titleRect = e.Bounds;
            titleRect.Height /= 2;
            titleRect.X += Window.IconSize + 3 * iconMargin;
            titleRect.Width -= Window.IconSize + 3 * iconMargin;
            TextRenderer.DrawText(e.Graphics, title, titleFont, titleRect, e.ForeColor, titleFlags);

            // Draw process name title
            Rectangle processRect = titleRect;
            processRect.Y += processRect.Height;
            TextRenderer.DrawText(e.Graphics, processName, processFont, processRect, processColor, processFlags);
            e.DrawFocusRectangle();
        }
    }
}
