using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sprung
{
    public class WindowListBox : ListBox
    {
        private const int iconSize = 30;
        private const int newItemHeight = 36;
        private int iconMargin = 0;
        private Font titleFont = new Font("Arial", 10.0f, FontStyle.Regular);
        private Font processFont = new Font("Arial", 9.0f, FontStyle.Regular);
        private TextFormatFlags titleFlags = TextFormatFlags.Left | TextFormatFlags.Bottom;
        private TextFormatFlags processFlags = TextFormatFlags.Left | TextFormatFlags.Top;
        private Color processColor = Color.FromArgb(110, 110, 110);

        public WindowListBox()
        {
            this.DrawMode = DrawMode.OwnerDrawVariable;
            iconMargin = (newItemHeight - iconSize) / 2;
            this.ItemHeight = newItemHeight;
        }
        
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index >= 0 && e.Index < Items.Count)
            {
                this.ItemHeight = newItemHeight;
                Window window = Items[e.Index] as Window;
                String title = window.getTitle().Replace("&", "&&");
                String processName = window.getProcessName().Replace("&", "&&");

                e.DrawBackground();

                // Draw icon
                Icon icon = window.getIcon();
                if (icon != null)
                {
                    Image image = (Image)new Bitmap(icon.ToBitmap(), new Size(iconSize, iconSize));
                    e.Graphics.DrawImage(image, iconMargin, e.Bounds.Y + iconMargin);
                }

                // Draw window title
                Rectangle titleRect = e.Bounds;
                titleRect.Height /= 2;
                titleRect.X += iconSize + 3 * iconMargin;
                titleRect.Width -= iconSize + 3 * iconMargin;
                TextRenderer.DrawText(e.Graphics, title, titleFont, titleRect, e.ForeColor, titleFlags);

                // Draw process name title
                Rectangle processRect = titleRect;
                processRect.Y += processRect.Height;
                TextRenderer.DrawText(e.Graphics, processName, processFont, processRect, processColor, processFlags);
                e.DrawFocusRectangle();
            }
        }
    }
}
