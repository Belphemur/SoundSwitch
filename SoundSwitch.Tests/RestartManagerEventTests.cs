using System;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Framework.WinApi;

namespace SoundSwitch.Tests;

/// <summary>
/// Pins the session-close decision fixed for #2580: SoundSwitch must only exit when the
/// session is really ending. Windows sends <c>WM_ENDSESSION</c> with <c>wParam == 0</c> when
/// another application refused the shutdown — the session continues and the tray app must
/// keep running. Previously every <c>WM_ENDSESSION</c> ended the process, so SoundSwitch
/// silently disappeared while the system kept running.
/// </summary>
[TestFixture]
public class RestartManagerEventTests
{
    private const int EndSessionCloseApp = 0x1;

    [Test]
    public void EndSession_WithZeroWParam_IsNotASessionEnding()
    {
        var cancelled = new WindowsAPIAdapter.RestartManagerEvent(WindowsAPIAdapter.RestartManagerEventType.EndSession) { WParam = IntPtr.Zero };

        cancelled.SessionEnding.Should().BeFalse("a cancelled session end (wParam=0) must leave the application running (#2580)");
    }

    [Test]
    public void EndSession_WithNonZeroWParam_IsASessionEnding()
    {
        var ending = new WindowsAPIAdapter.RestartManagerEvent(WindowsAPIAdapter.RestartManagerEventType.EndSession) { WParam = new IntPtr(1) };

        ending.SessionEnding.Should().BeTrue();
    }

    [Test]
    public void EndSession_FromRestartManager_ReportsCloseApp()
    {
        var installerClose = new WindowsAPIAdapter.RestartManagerEvent(WindowsAPIAdapter.RestartManagerEventType.EndSession)
        {
            WParam = new IntPtr(1),
            LParam = new IntPtr(EndSessionCloseApp)
        };

        installerClose.CloseAppRequested.Should().BeTrue("ENDSESSION_CLOSEAPP marks a Restart Manager (installer) close");
        installerClose.SessionEnding.Should().BeTrue();
    }

    [Test]
    public void EndSession_FromLogoffOrShutdown_DoesNotReportCloseApp()
    {
        var sessionClose = new WindowsAPIAdapter.RestartManagerEvent(WindowsAPIAdapter.RestartManagerEventType.EndSession) { WParam = new IntPtr(1) };

        sessionClose.CloseAppRequested.Should().BeFalse();
    }

    [Test]
    public void Query_FromRestartManager_ReportsCloseApp()
    {
        // WM_QUERYENDSESSION carries the ENDSESSION_CLOSEAPP hint in lParam: this is how
        // the log distinguishes an installer close from an interactive shutdown (#2580).
        var query = new WindowsAPIAdapter.RestartManagerEvent(WindowsAPIAdapter.RestartManagerEventType.Query) { LParam = new IntPtr(EndSessionCloseApp) };

        query.CloseAppRequested.Should().BeTrue();
    }

    [Test]
    public void Query_DefaultsToAgreeingToClose()
    {
        var query = new WindowsAPIAdapter.RestartManagerEvent(WindowsAPIAdapter.RestartManagerEventType.Query);

        query.Result.Should().Be(new IntPtr(1), "the app has no unsaved state and agrees to be closed");
    }
}
