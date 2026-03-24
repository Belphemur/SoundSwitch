using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SoundSwitch.Common.Framework.Icon;

namespace SoundSwitch.UI.Component;

public class IconTextComboBox : ComboBox
{
    public class DropDownItem : IDisposable
    {
        public static DropDownItem Empty = new DropDownItem
        {
            Text = ""
        };

        /// <summary>
        /// Tag of the item
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Reference-counted GDI icon handle for this item.
        /// The <see cref="DropDownItem"/> owns this handle and disposes it in <see cref="Dispose"/>.
        /// </summary>
        public IconHandle IconHandle { get; set; }

        /// <summary>
        /// Text to use in the dropdown
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Cast the tag in the wanted type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CastTag<T>()
        {
            return (T) Tag;
        }

        public void Dispose()
        {
            IconHandle?.Dispose();
        }
    }

    public IconTextComboBox()
    {
        DrawMode = DrawMode.OwnerDrawFixed;
        DropDownStyle = ComboBoxStyle.DropDownList;
        ValueMember = nameof(DropDownItem.Tag);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new DropDownItem[] DataSource
    {
        get => (DropDownItem[]) base.DataSource;
        set
        {
            var old = base.DataSource as DropDownItem[];
            // Avoid disposing items that are still being assigned to the same control.
            if (ReferenceEquals(old, value)) return;
            base.DataSource = value;
            if (old != null)
            {
                foreach (var item in old)
                    item.Dispose();
            }
        }
    }

    // Draws the items into the ColorSelector object
    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        e.DrawBackground();
        e.DrawFocusRectangle();
        if (e.Index >= 0 && e.Index < Items.Count)
        {
            var item = (DropDownItem) Items[e.Index];

            if (item == DropDownItem.Empty)
            {
                base.OnDrawItem(e);
                return;
            }

            var iconHandle = item.IconHandle;
            if (iconHandle == null)
            {
                base.OnDrawItem(e);
                return;
            }

            e.DrawBackground();

            var icon = iconHandle.Icon;
            var imageRect = new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Height, e.Bounds.Height);
            e.Graphics.DrawIcon(icon, imageRect);

            e.Graphics.DrawString(item.Text,
                e.Font,
                new SolidBrush(e.ForeColor),
                (e.Bounds.Location.X + icon.Width + 5),
                e.Bounds.Location.Y);
        }

        base.OnDrawItem(e);
    }
}
