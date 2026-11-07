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
        WindowsThemeHelper.DarkModeProvider = null;
    }

    [TearDown]
    public void TearDown()
    {
        ThemeResolver.ThemeModeProvider = _originalThemeModeProvider;
        ThemeResolver.DarkModeSupported = _originalDarkModeSupported;
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
}
