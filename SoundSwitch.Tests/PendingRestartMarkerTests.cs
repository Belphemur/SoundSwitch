using System;
using System.IO;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Framework.Updater;

namespace SoundSwitch.Tests;

/// <summary>
/// Pins the pending-restart marker's write/consume contract for the missed-installer-relaunch
/// fallback (PR #2624): the marker only records "an update was in flight"; it is deleted on
/// the next start, which then reports whether it was the Restart Manager's relaunch or a
/// plain start. The marker is detection+logging, never a second process — the user mutex
/// guarantees a single instance — so both consumption cases behave identically.
/// </summary>
[TestFixture]
public class PendingRestartMarkerTests
{
    private string _scratchDirectory;

    [SetUp]
    public void CreateScratchDirectory()
    {
        _scratchDirectory = Path.Combine(Path.GetTempPath(), "soundswitch-marker-tests-" + Guid.NewGuid().ToString("N"));
    }

    [TearDown]
    public void RemoveScratchDirectory()
    {
        if (Directory.Exists(_scratchDirectory))
        {
            Directory.Delete(_scratchDirectory, recursive: true);
        }
    }

    [Test]
    public void Write_CreatesMarkerFileWithTimestampContent()
    {
        PendingRestartMarker.Write(_scratchDirectory);

        File.Exists(PendingRestartMarker.GetPath(_scratchDirectory)).Should().BeTrue();
        File.ReadAllText(PendingRestartMarker.GetPath(_scratchDirectory)).Should().NotBeEmpty("the content is a timestamp, never parsed");
    }

    [Test]
    public void TryConsume_WithMarkerPresent_ConsumesAndReports()
    {
        PendingRestartMarker.Write(_scratchDirectory);

        PendingRestartMarker.TryConsume(_scratchDirectory, launchedByRestartManager: false).Should().BeTrue();
        File.Exists(PendingRestartMarker.GetPath(_scratchDirectory)).Should().BeFalse("consumption deletes the marker");
    }

    [Test]
    public void TryConsume_Twice_OnlyTheFirstConsumes()
    {
        PendingRestartMarker.Write(_scratchDirectory);

        PendingRestartMarker.TryConsume(_scratchDirectory, launchedByRestartManager: true).Should().BeTrue();
        PendingRestartMarker.TryConsume(_scratchDirectory, launchedByRestartManager: true).Should().BeFalse("the marker was already consumed");
    }

    [Test]
    public void TryConsume_BothStartKinds_ConsumeTheSameMarker()
    {
        // Whether the Restart Manager relaunched the executable or this is a plain start,
        // the marker is consumed exactly once — it can never create a second instance.
        foreach (var launchedByRestartManager in new[] { true, false })
        {
            PendingRestartMarker.Write(_scratchDirectory);
            PendingRestartMarker.TryConsume(_scratchDirectory, launchedByRestartManager).Should().BeTrue();
            File.Exists(PendingRestartMarker.GetPath(_scratchDirectory)).Should().BeFalse();
        }
    }

    [Test]
    public void TryConsume_WithoutMarker_ReturnsFalse()
    {
        PendingRestartMarker.TryConsume(_scratchDirectory, launchedByRestartManager: false).Should().BeFalse();
    }

    [Test]
    public void Write_Twice_LeavesASingleMarker()
    {
        PendingRestartMarker.Write(_scratchDirectory);
        PendingRestartMarker.Write(_scratchDirectory);

        File.Exists(PendingRestartMarker.GetPath(_scratchDirectory)).Should().BeTrue();
        PendingRestartMarker.TryConsume(_scratchDirectory, launchedByRestartManager: false).Should().BeTrue();
    }

    [Test]
    public void GetPath_CombinesDirectoryAndFileName()
    {
        PendingRestartMarker.GetPath(@"C:\Data").Should().Be(Path.Combine(@"C:\Data", "pending-restart"));
    }
}
