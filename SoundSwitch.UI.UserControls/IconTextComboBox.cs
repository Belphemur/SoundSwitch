using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SoundSwitch.UI.UserControls
{
    public class IconTextComboBox<T>: ComboBox
    {
        public class DropDownItem
        {
            /// <summary>
            /// Tag of the item
            /// </summary>
            public T Tag { get; set; }

            /// <summary>
            /// Icon to use in the dropdown
            /// </summary>
            public Icon Icon { get; set; }

            /// <summary>
            /// Text to use in the dropdown
            /// </summary>
            public string Text { get; set; }
        }

        public IconTextComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
        }

        public new DropDownItem[] DataSource {
            get => (DropDownItem[]) base.DataSource;
            set => base.DataSource = value;
        }

        // Draws the items into the ColorSelector object
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();

            var item = (DropDownItem)Items[e.Index];

            var icon = item.Icon;
            e.DrawBackground();

            var imageRect = new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Height, e.Bounds.Height);
            e.Graphics.DrawIcon(icon, imageRect);

            e.Graphics.DrawString(item.Text,
                e.Font,
                new SolidBrush(e.ForeColor),
                (e.Bounds.Location.X + icon.Width + 5),
                e.Bounds.Location.Y);

            base.OnDrawItem(e);
        }
    }
}