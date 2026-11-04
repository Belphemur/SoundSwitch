using System;
using System.Threading;
using System.Windows.Forms;

using FluentAssertions;
using NUnit.Framework;

using SoundSwitch.UI.Component;

namespace SoundSwitch.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public class NumericUpDownWithUnitsTests
{
    private sealed class SettingsModel
    {
        public bool QuickMenuEnabled { get; set; }
        public int BannerScalePercentage { get; set; } = 100;
    }

    [Test]
    public void BindValue_ShowsTheModelValueAndWritesChangesBack()
    {
        var model = new SettingsModel();
        using var control = new NumericUpDownWithUnits
        {
            Minimum = 50,
            Maximum = 200,
            TextUnit = "%"
        };

        control.BindValue(() => model.BannerScalePercentage,
            percentage => model.BannerScalePercentage = percentage);

        control.Value.Should().Be(100);
        control.Text.Should().Be("100%");

        control.Value = 150;

        control.Value.Should().Be(150);
        model.BannerScalePercentage.Should().Be(150);
    }

    [Test]
    public void BindValue_DoesNotRegisterADataBinding()
    {
        // An inactive binding (a control on a tab page that was never displayed) is still pushed to by
        // the shared PropertyManager, and that null push becomes a 0 Value.
        using var control = new NumericUpDownWithUnits { Minimum = 50, Maximum = 200 };

        control.BindValue(() => 100, _ => { });

        control.DataBindings.Count.Should().Be(0);
    }

    [Test]
    public void ChangingAnotherBoundControl_WhileTheRangeLimitedControlIsOnAnUndisplayedTab_DoesNotThrow()
    {
        var model = new SettingsModel();
        using var form = new Form();
        using var tabs = new TabControl { Dock = DockStyle.Fill };
        using var generalPage = new TabPage("General");
        using var bannerPage = new TabPage("Banner");
        tabs.TabPages.AddRange([generalPage, bannerPage]);
        form.Controls.Add(tabs);

        using var quickMenu = new CheckBox();
        generalPage.Controls.Add(quickMenu);
        quickMenu.DataBindings.Add(nameof(CheckBox.Checked), model, nameof(SettingsModel.QuickMenuEnabled),
            false, DataSourceUpdateMode.OnPropertyChanged);

        using var scale = new NumericUpDownWithUnits { Minimum = 50, Maximum = 200, TextUnit = "%" };
        scale.BindValue(() => model.BannerScalePercentage,
            percentage => model.BannerScalePercentage = percentage);
        bannerPage.Controls.Add(scale);

        form.Show();
        Application.DoEvents();

        // What clicking "Quick Menu on hotkey" does: the checkbox binding writes into the data source,
        // which makes the shared PropertyManager push to every binding, inactive ones included.
        var toggleQuickMenu = () => quickMenu.Checked = true;
        toggleQuickMenu.Should().NotThrow();

        model.QuickMenuEnabled.Should().BeTrue();
        scale.Value.Should().Be(100);

        tabs.SelectedTab = bannerPage;
        Application.DoEvents();

        scale.Value.Should().Be(100);
    }
}
