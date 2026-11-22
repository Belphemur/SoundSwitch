using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Framework.Updater.Installer;

namespace SoundSwitch.Tests;

/// <summary>
/// Pins the installer switches of the silent auto-update flow (#2580): Inno Setup closes
/// the running instance through the Windows Restart Manager (<c>/CLOSEAPPLICATIONS</c>) and
/// must relaunch the updated executable afterwards (<c>/RESTARTAPPLICATIONS</c>). Without
/// the relaunch the tray app stayed dead until the next logon.
/// </summary>
[TestFixture]
public class UpdateRunnerTests
{
    [Test]
    public void BuildInstallerArguments_InstructsInnoToCloseAndRestartApplications()
    {
        var args = UpdateRunner.BuildInstallerArguments(string.Empty);

        args.Should().Contain("/CLOSEAPPLICATIONS", "the running instance must be closed by the installer");
        args.Should().Contain("/RESTARTAPPLICATIONS", "the installer must relaunch SoundSwitch after installing (#2580)");
        args.Should().Contain("/NOCANCEL");
        args.Should().Contain("/NORESTART", "the machine itself must not be rebooted by an app update");
    }

    [Test]
    public void BuildInstallerArguments_PreservesCallerArguments()
    {
        var args = UpdateRunner.BuildInstallerArguments("/VERYSILENT");

        args.Should().StartWith("/VERYSILENT");
        args.Should().Contain("/CLOSEAPPLICATIONS");
        args.Should().Contain("/RESTARTAPPLICATIONS");
    }

    [Test]
    public void BuildInstallerArguments_AlwaysAppendsBothSwitchesOfTheCloseRestartPair()
    {
        // The two switches only work as a pair: closing via the Restart Manager without
        // requesting the restart is the regression that left the app dead (#2580).
        var args = UpdateRunner.BuildInstallerArguments(string.Empty);

        args.IndexOf("/CLOSEAPPLICATIONS").Should().BeLessThan(args.IndexOf("/RESTARTAPPLICATIONS"));
    }
}
