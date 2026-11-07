using System;
using System.Threading;
using System.Windows.Forms;

using NUnit.Framework;

using SoundSwitch.Framework.Theme;
using SoundSwitch.Framework.WinApi;

namespace SoundSwitch.Tests;

/// <summary>
/// Regression tests for the app theme setting decision (issue #2567): the framework
/// colour mode and the custom-paint axis must agree, and Windows 10 — where WinForms
/// dark mode is incomplete per the .NET docs — must resolve to a coherent light
/// palette instead of the broken mixed rendering.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public class ThemeResolverTests
{
    private static readonly Func<ThemeMode> _originalThemeModeProvider = ThemeResolver.ThemeModeProvider;
    private static readonly Func<bool> _originalDarkModeSupported = ThemeResolver.DarkModeSupported;

    private static void UseTheme(ThemeMode mode, bool darkModeSupported)
    {
        ThemeResolver.ThemeModeProvider = () => mode;
        ThemeResolver.DarkModeSupported = () => darkModeSupported;
    }

    [SetUp]
    public void SetUp()
    {
        // Implicit values regressed in a test are caught by TearDown; keep each test explicit anyway.
        ThemeResolver.ThemeModeProvider = _originalThemeModeProvider;
        ThemeResolver.DarkModeSupported = _originalDarkModeSupported;
        ThemeResolver.ResetEffectiveThemeModeForTests();
        WindowsThemeHelper.DarkModeProvider = null;
    }

    [TearDown]
    public void TearDown()
    {
        ThemeResolver.ThemeModeProvider = _originalThemeModeProvider;
        ThemeResolver.DarkModeSupported = _originalDarkModeSupported;
        ThemeResolver.ResetEffectiveThemeModeForTests();
        WindowsThemeHelper.DarkModeProvider = null;
    }

    [TestCase(ThemeMode.Light, false, ExpectedResult = SystemColorMode.Classic)]
    [TestCase(ThemeMode.Light, true, ExpectedResult = SystemColorMode.Classic)]
    [TestCase(ThemeMode.Dark, false, ExpectedResult = SystemColorMode.Classic)] // Windows 10: clamped, framework cannot render dark
    [TestCase(ThemeMode.Dark, true, ExpectedResult = SystemColorMode.Dark)]
    [TestCase(ThemeMode.System, false, ExpectedResult = SystemColorMode.Classic)]
    [TestCase(ThemeMode.System, true, ExpectedResult = SystemColorMode.System)]
    public SystemColorMode ResolveColorMode_HonoursModeAndHostCapabilities(ThemeMode mode, bool darkModeSupported)
    {
        UseTheme(mode, darkModeSupported);

        return ThemeResolver.ResolveColorMode();
    }

    [TestCase(ThemeMode.Light, true, true, ExpectedResult = false)]
    [TestCase(ThemeMode.Dark, true, true, ExpectedResult = true)]
    [TestCase(ThemeMode.Dark, false, true, ExpectedResult = true)] // forced Dark ignores the OS app-mode
    [TestCase(ThemeMode.Dark, true, false, ExpectedResult = false)] // Windows 10 clamp
    [TestCase(ThemeMode.System, true, true, ExpectedResult = true)] // System follows the OS on a capable host
    [TestCase(ThemeMode.System, false, true, ExpectedResult = false)]
    [TestCase(ThemeMode.System, true, false, ExpectedResult = false)] // #2567: dark OS on Windows 10 must paint light everywhere
    [TestCase((ThemeMode)23, true, true, ExpectedResult = true)] // out-of-range persisted value behaves as System
    [TestCase((ThemeMode)(-1), true, true, ExpectedResult = true)]
    public bool ResolveIsDarkMode_IsTheSingleDecision(ThemeMode mode, bool osDarkMode, bool darkModeSupported)
    {
        return ThemeResolver.ResolveIsDarkMode(mode, osDarkMode, darkModeSupported);
    }

    [TestCase(ThemeMode.Light, ExpectedResult = false)]
    [TestCase(ThemeMode.Dark, ExpectedResult = true)]
    public bool IsDarkModeEnabled_FollowsTheForcedMode(ThemeMode mode)
    {
        // The OS registry read is bypassed: a forced mode wins regardless of the host,
        // which is what keeps custom paint (this helper) and the framework palette in
        // agreement without touching machine state in the tests.
        WindowsThemeHelper.DarkModeProvider = null;
        UseTheme(mode, darkModeSupported: true);

        return WindowsThemeHelper.IsDarkModeEnabled();
    }

    [Test]
    public void IsDarkModeEnabled_KeepsTheStartupThemeWhenRestartIsDeclined()
    {
        // At "startup" the user asked for Light; custom paint uses the captured selection.
        UseTheme(ThemeMode.Light, darkModeSupported: true);
        Assert.That(WindowsThemeHelper.IsDarkModeEnabled(), Is.False);

        // The user then picks Dark and declines the restart prompt: the setting is
        // persisted, but the process keeps rendering the startup theme so the custom
        // paint never disagrees with the WinForms palette until the next launch.
        UseTheme(ThemeMode.Dark, darkModeSupported: true);

        Assert.That(ThemeResolver.EffectiveThemeMode, Is.EqualTo(ThemeMode.Light));
        Assert.That(WindowsThemeHelper.IsDarkModeEnabled(), Is.False);
    }

    [Test]
    public void EffectiveThemeMode_CapturesTheSelectionOncePerProcess()
    {
        UseTheme(ThemeMode.Light, darkModeSupported: true);
        var firstRead = ThemeResolver.EffectiveThemeMode;

        // Later configuration changes (saved setting after a restart prompt was
        // declined) must not change what the process renders.
        UseTheme(ThemeMode.Dark, darkModeSupported: true);

        Assert.That(firstRead, Is.EqualTo(ThemeMode.Light));
        Assert.That(ThemeResolver.EffectiveThemeMode, Is.EqualTo(ThemeMode.Light));
    }

    [Test]
    public void EffectiveThemeMode_SystemKeepsConsultingTheOsAppMode()
    {
        // ResolveIsDarkMode(System, osDark) mirrors the live OS read: the System
        // selection stays dynamic even though the selection itself is captured.
        Assert.That(ThemeResolver.ResolveIsDarkMode(ThemeMode.System, osDarkMode: true, darkModeSupported: true), Is.True);
        Assert.That(ThemeResolver.ResolveIsDarkMode(ThemeMode.System, osDarkMode: false, darkModeSupported: true), Is.False);
    }
}
