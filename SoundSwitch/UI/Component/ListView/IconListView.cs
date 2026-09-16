using System.Drawing;
using System.Windows.Forms;

using SoundSwitch.Common.Framework.Icon;
using SoundSwitch.Framework.WinApi;

namespace SoundSwitch.UI.Component.ListView;

public class IconListView : ListViewExtended
{
    /// <summary>
    /// Column header text colour in dark mode. The native common control does not expose
    /// a managed palette for the header strip, so it is painted explicitly.
    /// </summary>
    internal static Color DarkColumnHeaderTextColor => Color.FromArgb(240, 240, 240);

    private const int ColumnHeaderPadding = 7;

    public IconListView()
    {
        OwnerDraw = true;
        View = View.Details;
        FullRowSelect = true;
    }

    protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
    {
        if (!WindowsThemeHelper.IsDarkModeEnabled())
        {
            e.DrawDefault = true;
            base.OnDrawColumnHeader(e);
            return;
        }

        using var backgroundBrush = new SolidBrush(SystemColors.Window);
        e.Graphics.FillRectangle(backgroundBrush, e.Bounds);

        using var font = new Font(Font, FontStyle.Bold);
        TextRenderer.DrawText(e.Graphics, e.Header.Text, font, e.Bounds, DarkColumnHeaderTextColor,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis |
            TextFormatFlags.PreserveGraphicsClipping);
    }

    protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
    {
        // Resolve an IconHandle or a raw Icon stored in the sub-item Tag.
        System.Drawing.Icon icon = e.SubItem.Tag switch
        {
            IconHandle h => h.Icon,
            System.Drawing.Icon i => i,
            _ => null
        };

        if (string.IsNullOrEmpty(e.SubItem.Text) || icon == null)
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
