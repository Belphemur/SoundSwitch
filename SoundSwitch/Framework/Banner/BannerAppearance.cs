using System;
using System.Drawing;

namespace SoundSwitch.Framework.Banner;

/// <summary>
/// Pure calculations behind the banner appearance settings, kept out of <see cref="BannerForm"/> so
/// they can be covered by tests without constructing WinForms controls.
/// </summary>
internal static class BannerAppearance
{
    /// <summary>Smallest banner scale, in percent, the UI offers.</summary>
    public const int MinScalePercentage = 50;

    /// <summary>Largest banner scale, in percent, the UI offers.</summary>
    public const int MaxScalePercentage = 200;

    /// <summary>Multiplier applied to fonts and padding by the compact microphone banner.</summary>
    public const float CompactFontScale = 0.8f;

    /// <summary>
    /// Multiplier applied to the icon by the compact microphone banner. It differs from the font
    /// multiplier because the compact banner shows a comparatively larger icon.
    /// </summary>
    public const float CompactImageScale = 1.5f;

    /// <summary>
    /// Luminance at which black and white text score the same contrast ratio against a
    /// background. Below it white wins, above it black wins.
    /// </summary>
    private const double LUMINANCE_CROSSOVER = 0.179d;

    /// <summary>
    /// Scale factor for the banner fonts and padding.
    /// </summary>
    public static float FontScale(int scalePercentage, bool compactMode) =>
        NormalizePercentage(scalePercentage) * (compactMode ? CompactFontScale : 1f);

    /// <summary>Scale factor for the banner icon.</summary>
    public static float ImageScale(int scalePercentage, bool compactMode) =>
        NormalizePercentage(scalePercentage) * (compactMode ? CompactImageScale : 1f);

    /// <summary>
    /// Size the icon is drawn at.
    /// </summary>
    /// <param name="imageSize">Size of the icon actually shown, or null when none is set</param>
    /// <param name="fallbackSize">Size to use when no icon is set</param>
    /// <param name="scale">Factor from <see cref="ImageScale"/></param>
    /// <remarks>
    /// The icon being displayed has to drive this, not the designer's placeholder box: notification
    /// icons are 32x32 while the placeholder is 16x16, so scaling the placeholder would halve every
    /// banner at the default scale.
    /// </remarks>
    public static Size ResolveImageSize(Size? imageSize, Size fallbackSize, float scale)
    {
        var baseSize = imageSize ?? fallbackSize;
        return new Size(
            Math.Max(1, (int)Math.Round(baseSize.Width * scale)),
            Math.Max(1, (int)Math.Round(baseSize.Height * scale)));
    }

    /// <summary>
    /// Foreground colour that stays readable on <paramref name="background"/>.
    /// </summary>
    /// <remarks>
    /// Picked by WCAG relative luminance rather than <see cref="Color.GetBrightness"/>, which is
    /// the largest colour channel and so picks black text for saturated blues and greens where
    /// white is the readable choice.
    /// </remarks>
    public static Color ContrastingForeground(Color background) =>
        RelativeLuminance(background) > LUMINANCE_CROSSOVER ? Color.Black : Color.White;

    /// <summary>
    /// WCAG relative luminance of a colour.
    /// </summary>
    private static double RelativeLuminance(Color color) =>
        0.2126 * Linearize(color.R) +
        0.7152 * Linearize(color.G) +
        0.0722 * Linearize(color.B);

    private static double Linearize(byte channel)
    {
        var value = channel / 255d;
        return value <= 0.03928d ? value / 12.92d : Math.Pow((value + 0.055d) / 1.055d, 2.4d);
    }

    /// <summary>
    /// Turns a stored percentage into a factor, falling back to 100% when it is outside the
    /// supported range so a hand-edited or corrupted configuration cannot produce an unusable
    /// banner.
    /// </summary>
    private static float NormalizePercentage(int scalePercentage) =>
        scalePercentage is < MinScalePercentage or > MaxScalePercentage
            ? 100 / 100f
            : scalePercentage / 100f;
}