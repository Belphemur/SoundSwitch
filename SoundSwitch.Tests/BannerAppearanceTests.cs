using System;
using System.Drawing;

using FluentAssertions;
using NUnit.Framework;

using SoundSwitch.Framework.Banner;

namespace SoundSwitch.Tests;

[TestFixture]
public class BannerAppearanceTests
{
    private static readonly Size DesignPlaceholder = new(16, 16);
    private static readonly Size NotificationIcon = new(32, 32);

    [TestCase(50, 0.5f)]
    [TestCase(100, 1f)]
    [TestCase(150, 1.5f)]
    [TestCase(200, 2f)]
    public void FontScale_FollowsTheConfiguredPercentage(int percentage, float expected)
    {
        BannerAppearance.FontScale(percentage, false).Should().Be(expected);
        BannerAppearance.ImageScale(percentage, false).Should().Be(expected);
    }

    [Test]
    public void CompactBannerUsesItsOwnMultipliers()
    {
        // The compact microphone banner renders a comparatively larger icon in a smaller banner,
        // so the icon multiplier deliberately differs from the font one.
        BannerAppearance.FontScale(100, true).Should().Be(0.8f);
        BannerAppearance.ImageScale(100, true).Should().Be(1.5f);
    }

    [TestCase(0)]
    [TestCase(-50)]
    [TestCase(49)]
    [TestCase(201)]
    [TestCase(int.MaxValue)]
    public void ScaleOutsideTheSupportedRangeFallsBackTo100Percent(int percentage)
    {
        // Guards a hand-edited or corrupted configuration against an unusable banner.
        BannerAppearance.FontScale(percentage, false).Should().Be(1f);
        BannerAppearance.ImageScale(percentage, false).Should().Be(1f);
    }

    [Test]
    public void IconSizeIsDerivedFromTheDisplayedIconNotTheDesignPlaceholder()
    {
        // Regression: scaling the designer's 16x16 placeholder halved every 32x32 notification icon
        // at the default scale, because the placeholder was used as the base size.
        BannerAppearance.ResolveImageSize(NotificationIcon, DesignPlaceholder, 1f)
            .Should().Be(NotificationIcon);

        BannerAppearance.ResolveImageSize(null, DesignPlaceholder, 1f)
            .Should().Be(DesignPlaceholder);
    }

    [Test]
    public void IconSizeScalesUpAndDown()
    {
        BannerAppearance.ResolveImageSize(NotificationIcon, DesignPlaceholder, 0.5f)
            .Should().Be(new Size(16, 16));
        BannerAppearance.ResolveImageSize(NotificationIcon, DesignPlaceholder, 2f)
            .Should().Be(new Size(64, 64));
    }

    [Test]
    public void IconSizeNeverCollapsesToZero()
    {
        // A zero-sized picture box would hide the icon entirely.
        BannerAppearance.ResolveImageSize(new Size(1, 1), DesignPlaceholder, 0.1f)
            .Should().Be(new Size(1, 1));
    }

    // Picked by WCAG relative luminance, the point where black and white score equally against the
    // background is 0.179. Black text wins above it, white below.
    [TestCase(0, 0, 0, true)]        // luminance 0.000
    [TestCase(0, 0, 255, true)]      // luminance 0.072
    [TestCase(255, 0, 0, false)]     // luminance 0.213
    [TestCase(128, 128, 128, false)] // luminance 0.216
    [TestCase(0, 255, 0, false)]     // luminance 0.715
    [TestCase(255, 255, 0, false)]   // luminance 0.928
    [TestCase(255, 255, 255, false)] // luminance 1.000
    public void ForegroundContrastsWithTheConfiguredBackground(byte r, byte g, byte b, bool expectWhite)
    {
        BannerAppearance.ContrastingForeground(Color.FromArgb(r, g, b))
            .Should().Be(expectWhite ? Color.White : Color.Black);
    }

    [TestCase(0, 0, 0)]
    [TestCase(255, 255, 255)]
    [TestCase(255, 0, 0)]
    [TestCase(0, 0, 255)]
    [TestCase(0, 255, 0)]
    [TestCase(0, 255, 255)]
    [TestCase(255, 255, 0)]
    [TestCase(255, 0, 255)]
    [TestCase(64, 64, 64)]
    [TestCase(128, 128, 128)]
    [TestCase(192, 192, 192)]
    [TestCase(20, 30, 200)]
    [TestCase(240, 230, 40)]
    public void ForegroundMeetsTheMinimumContrastRatioOnAnyBackground(byte r, byte g, byte b)
    {
        // The user picks this colour, so whatever it is, the text has to stay readable on it.
        var background = Color.FromArgb(r, g, b);
        var foreground = BannerAppearance.ContrastingForeground(background);

        ContrastRatio(background, foreground).Should().BeGreaterThanOrEqualTo(4.5d,
            "WCAG AA for normal text");
    }

    [Test]
    public void ForegroundAlwaysScoresBetterThanTheOppositeChoice()
    {
        // Otherwise the crossover point is mistuned.
        for (var value = 0; value <= 255; value += 5)
        {
            var background = Color.FromArgb(value, value, value);
            var foreground = BannerAppearance.ContrastingForeground(background);
            var alternative = foreground == Color.Black ? Color.White : Color.Black;

            ContrastRatio(background, foreground)
                .Should().BeGreaterThanOrEqualTo(ContrastRatio(background, alternative));
        }
    }

    /// <summary>
    /// WCAG 2.x contrast ratio between two colours, from 1 (identical) to 21 (black on white).
    /// </summary>
    private static double ContrastRatio(Color a, Color b)
    {
        var lighter = Math.Max(Luminance(a), Luminance(b));
        var darker = Math.Min(Luminance(a), Luminance(b));
        return (lighter + 0.05d) / (darker + 0.05d);
    }

    private static double Luminance(Color color)
    {
        static double Linearize(byte channel)
        {
            var value = channel / 255d;
            return value <= 0.03928d ? value / 12.92d : Math.Pow((value + 0.055d) / 1.055d, 2.4d);
        }

        return 0.2126d * Linearize(color.R) + 0.7152d * Linearize(color.G) + 0.0722d * Linearize(color.B);
    }
}