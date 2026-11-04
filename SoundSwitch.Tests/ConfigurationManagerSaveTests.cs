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

    /// <summary>
    /// Deleting a tree right after writing files inside it can collide with a short-lived
    /// external handle (e.g. an antivirus scanning the fresh files); wait it out.
    /// </summary>
    private static void DeleteDirectoryWithRetries(string path)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                Directory.Delete(path, true);
                return;
            }
            catch (Exception ex) when (attempt < 8 && ex is IOException or UnauthorizedAccessException)
            {
                Thread.Sleep(250);
            }
        }
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
            DeleteDirectoryWithRetries(directory);
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
            // Another process holds the file without sharing; the retry hook releases it
            // once the save has observed a failed attempt, so the retry is deterministic.
            var lockedStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            try
            {
                Exception failedAttempt = null;
                ConfigurationManager.SaveConfigurationToPath(new TestConfiguration {Value = "updated"}, filePath,
                    ex =>
                    {
                        failedAttempt = ex;
                        lockedStream.Dispose();
                    });

                failedAttempt.Should().NotBeNull();
            }
            finally
            {
                // Independent of the hook, so a failing test never leaves the directory locked.
                lockedStream.Dispose();
            }

            File.ReadAllText(filePath).Should().Contain("updated");
        }
        finally
        {
            DeleteDirectoryWithRetries(directory);
        }
    }

    [Test]
    public void SaveConfigurationToPath_RetriesWhenDestinationIsDeletePending()
    {
        var directory = CreateTempDirectory();
        try
        {
            var filePath = Path.Combine(directory, nameof(TestConfiguration) + ".json");
            File.WriteAllText(filePath, "previous");
            // A handle scheduled for delete on close (OneDrive/AV style replacement) keeps the
            // destination delete-pending; the retry hook releases it once the save has observed
            // the failure, so the retry is deterministic and must succeed.
            var deletePendingStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None,
                bufferSize: 1, FileOptions.DeleteOnClose);
            try
            {
                Exception failedAttempt = null;
                ConfigurationManager.SaveConfigurationToPath(new TestConfiguration {Value = "updated"}, filePath,
                    ex =>
                    {
                        failedAttempt = ex;
                        deletePendingStream.Dispose();
                    });

                failedAttempt.Should().NotBeNull();
            }
            finally
            {
                // Independent of the hook, so a failing test never leaves the directory locked.
                deletePendingStream.Dispose();
            }

            File.ReadAllText(filePath).Should().Contain("updated");
        }
        finally
        {
            DeleteDirectoryWithRetries(directory);
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
            DeleteDirectoryWithRetries(directory);
        }
    }

    [Test]
    public void SaveConfigurationToPath_PropagatesPermanentDestinationErrorsWithoutLeavingTempFile()
    {
        var directory = CreateTempDirectory();
        var filePath = Path.Combine(directory, nameof(TestConfiguration) + ".json");
        try
        {
            const string previousContent = "previous";
            File.WriteAllText(filePath, previousContent);
            try
            {
                // Access denied on a read-only destination is permanent, not a lock another
                // process could release: it must surface to the caller instead of being
                // silently retried and swallowed.
                File.SetAttributes(filePath, FileAttributes.ReadOnly);

                Action save = () =>
                    ConfigurationManager.SaveConfigurationToPath(new TestConfiguration {Value = "updated"}, filePath);

                save.Should().Throw<Exception>();
            }
            finally
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
            }

            // The failed save must not have replaced the previous configuration, nor left a temp file behind.
            File.ReadAllText(filePath).Should().Be(previousContent);
            Directory.GetFiles(directory, "*.tmp").Should().BeEmpty();
        }
        finally
        {
            DeleteDirectoryWithRetries(directory);
        }
    }

    [Test]
    public void SaveConfigurationToPath_PropagatesInterruptedSerializationAndKeepsPreviousConfiguration()
    {
        var directory = CreateTempDirectory();
        var filePath = Path.Combine(directory, nameof(TestConfiguration) + ".json");
        try
        {
            const string previousContent = "previous";
            File.WriteAllText(filePath, previousContent);
            // The property getter throws once serialization has started, leaving half-written
            // content in the temporary file.
            Action save = () =>
                ConfigurationManager.SaveConfigurationToPath(new InterruptedSerializationConfiguration(), filePath);

            save.Should().Throw<Exception>();

            // A mid-serialization failure must never corrupt the previous configuration on disk,
            // nor leave the partial temporary file behind.
            File.ReadAllText(filePath).Should().Be(previousContent);
            Directory.GetFiles(directory, "*.tmp").Should().BeEmpty();
        }
        finally
        {
            DeleteDirectoryWithRetries(directory);
        }
    }

    private class InterruptedSerializationConfiguration : IConfiguration
    {
        public string Value { get; set; } = "";

        public string FileLocation { get; set; }

        public string Boom => throw new InvalidOperationException("serialization interrupted");

        public void Save()
        {
            ConfigurationManager.SaveConfiguration(this);
        }

        public bool Migrate()
        {
            return false;
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
