using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using NUnit.Framework;

using SoundSwitch.UI.Component.ListView;

namespace SoundSwitch.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
public class ListViewExtendedTests
{
    [Test]
    public void DrawGroupHeader_ShouldPaintDarkBackgroundAndSeparator()
    {
        using var listView = new ListViewExtended
        {
            BackColor = Color.FromArgb(32, 32, 32)
        };
        var group = new ListViewGroup("Selected", HorizontalAlignment.Center);
        listView.Groups.Add(group);

        using var bitmap = new Bitmap(80, 24);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Red);

        listView.DrawGroupHeader(graphics, new Rectangle(0, 0, 80, 24), group);

        Assert.That(bitmap.GetPixel(1, 1), Is.EqualTo(Color.FromArgb(32, 32, 32)));
        Assert.That(bitmap.GetPixel(40, 23), Is.EqualTo(Color.FromArgb(80, 80, 80)));
    }
}
