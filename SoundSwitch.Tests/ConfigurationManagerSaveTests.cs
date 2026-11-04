using System;
using System.IO;
using System.Threading;

using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

using SoundSwitch.Framework.Configuration;

namespace SoundSwitch.Tests;

[TestFixture]
public class ConfigurationManagerSaveTests
{
    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "SoundSwitchTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    [Test]
    public void SaveConfigurationToPath_SwapsConfigurationAtomicallyAndLeavesNoTempFile()
    {
        var directory = CreateTempDirectory();
        try
        {
            var filePath = Path.Combine(directory, nameof(TestConfiguration) + ".json");
            ConfigurationManager.SaveConfigurationToPath(new TestConfiguration {Value = "first"}, filePath);
            ConfigurationManager.SaveConfigurationToPath(new TestConfiguration {Value = "second"}, filePath);

            var saved = JsonConvert.DeserializeObject<TestConfiguration>(File.ReadAllText(filePath));
            saved!.Value.Should().Be("second");
            Directory.GetFiles(directory, "*.tmp").Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Test]
    public void SaveConfigurationToPath_RetriesWhenDestinationIsTransientlyLockedByAnotherProcess()
    {
        var directory = CreateTempDirectory();
        try
        {
            var filePath = Path.Combine(directory, nameof(TestConfiguration) + ".json");
            File.WriteAllText(filePath, "previous");
            // Another process holds the file without sharing; release it while the save retries.
            var lockedStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            using var releaseLock = new Timer(_ => lockedStream.Dispose(), null,
                TimeSpan.FromMilliseconds(100), Timeout.InfiniteTimeSpan);

            ConfigurationManager.SaveConfigurationToPath(new TestConfiguration {Value = "updated"}, filePath);

            File.ReadAllText(filePath).Should().Contain("updated");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Test]
    public void SaveConfigurationToPath_DoesNotThrowAndKeepsPreviousConfigurationOnPersistentLock()
    {
        var directory = CreateTempDirectory();
        try
        {
            var filePath = Path.Combine(directory, nameof(TestConfiguration) + ".json");
            const string previousContent = "previous";
            File.WriteAllText(filePath, previousContent);
            // Another process holds the file without sharing while the save gives up.
            var lockedStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            try
            {
                Action save = () =>
                    ConfigurationManager.SaveConfigurationToPath(new TestConfiguration {Value = "updated"}, filePath);

                save.Should().NotThrow();
            }
            finally
            {
                lockedStream.Dispose();
            }

            File.ReadAllText(filePath).Should().Be(previousContent);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private class TestConfiguration : IConfiguration
    {
        public string Value { get; set; } = "";

        public string FileLocation { get; set; }

        public void Save()
        {
            ConfigurationManager.SaveConfiguration(this);
        }

        public bool Migrate()
        {
            return false;
        }
    }
}
