using System;
using System.Drawing;
using System.Windows.Forms;

namespace SoundSwitch.UI.UserControls
{
    public class IconListView : ListView
    {
        public IconListView()
        {
            OwnerDraw = true;
            View = View.Details;
            FullRowSelect = true;
        }

        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
            base.OnDrawColumnHeader(e);
        }

        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            if (string.IsNullOrEmpty(e.SubItem.Text) || !(e.SubItem.Tag is Icon icon))
            {
                e.DrawDefault = true;
                base.OnDrawSubItem(e);
                return;
            }

            e.DrawBackground();

            if (e.Item.Selected && Focused)
            {
                var r = new Rectangle(e.Bounds.Left, e.Bounds.Top, e.Bounds.Right, e.Bounds.Height);
                e.Graphics.FillRectangle(SystemBrushes.Highlight, r);
                e.SubItem.ForeColor = SystemColors.HighlightText;
            }
            else if (e.Item.Selected && !Focused)
            {
                var r = new Rectangle(e.Bounds.Left, e.Bounds.Top, e.Bounds.Right, e.Bounds.Height);
                e.Graphics.FillRectangle(SystemBrushes.ControlLight, r);
                e.SubItem.ForeColor = SystemColors.WindowText;
            }
            else
            {
                e.SubItem.ForeColor = SystemColors.WindowText;
            }

            var imageRect = new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Height, e.Bounds.Height);
            e.Graphics.DrawIcon(icon, imageRect);

            e.Graphics.DrawString(e.SubItem.Text,
                e.SubItem.Font,
                new SolidBrush(e.SubItem.ForeColor),
                (e.SubItem.Bounds.Location.X + icon.Width + 5),
                e.SubItem.Bounds.Location.Y);
        }
    }
}