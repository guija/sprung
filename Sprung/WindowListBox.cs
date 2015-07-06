using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sprung
{
    public class WindowListBox : ListBox
    {
        public WindowListBox()
        {
            this.DrawMode = DrawMode.OwnerDrawVariable;
            this.ItemHeight = 24;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            const TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter;
            if (e.Index >= 0 && e.Index < Items.Count)
            {

                this.ItemHeight = 24;
                Window window = Items[e.Index] as Window;
                String text = window.getProcessTitle();
                text = text.Length > 60 ? text.Substring(0, 60) + "..." : text;
                
                // Get icon from process and resize it
                Icon icon = Icon.ExtractAssociatedIcon(window.getProcess().MainModule.FileName);
                Image image = (Image) new Bitmap(icon.ToBitmap(), new Size(20,20));
                
                e.DrawBackground();

                // draw icon
                e.Graphics.DrawImage(image, 2, e.Bounds.Y + 2);

                var textRect = e.Bounds;
                textRect.X += 24;
                textRect.Width -= 20;
                Font font = new Font("Arial", 12.0f, FontStyle.Regular);
                TextRenderer.DrawText(e.Graphics, text, font, textRect, e.ForeColor, flags);
                e.DrawFocusRectangle();
            }
        }

    }
}
