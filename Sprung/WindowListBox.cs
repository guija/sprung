using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sprung
{
    class WindowListBox : ListBox
    {
        public WindowListBox()
        {
            this.DrawMode = DrawMode.OwnerDrawFixed;
            ItemHeight = 18;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {

            const TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter;

            //base.OnDrawItem(e);
            if (e.Index >= 0 && e.Index < Items.Count)
            {
                Window window = Items[e.Index] as Window;
                String text = window.getProcessTitle();
                // Get icon from process and resize it
                Icon icon = Icon.ExtractAssociatedIcon(window.getProcess().MainModule.FileName);
                Image image = (Image) new Bitmap(icon.ToBitmap(), new Size(14,14));
                
                e.DrawBackground();

                // draw icon
                e.Graphics.DrawImage(image, 2, e.Bounds.Y + 2);

                var textRect = e.Bounds;
                textRect.X += 20;
                textRect.Width -= 20;
                TextRenderer.DrawText(e.Graphics, text, e.Font, textRect, e.ForeColor, flags);
                e.DrawFocusRectangle();
            }
        }

    }
}
