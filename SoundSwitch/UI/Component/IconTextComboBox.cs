using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SoundSwitch.UI.Component
{
    public class IconTextComboBox : ComboBox
    {
        public class DropDownItem
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
            /// Icon to use in the dropdown
            /// </summary>
            public Icon Icon { get; set; }

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
            set => base.DataSource = value;
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

                var icon = item.Icon;
                e.DrawBackground();

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
}