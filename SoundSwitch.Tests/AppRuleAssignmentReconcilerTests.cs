using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using FluentAssertions;

using Microsoft.Win32;

using NUnit.Framework;

using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Client;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Audio.Manager.Interop.Interface.Policy.Extended;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Model;
using SoundSwitch.Services;

namespace SoundSwitch.Tests;

/// <summary>
/// The ownership reconcile state machine of the app-rules identity ADR (§6.4): runs against a
/// scratch persisted-endpoint store and a stateful fake of the per-pid policy config injected
/// through the <c>AudioSwitcher</c> test seam. Windows only — the COM apartment and the real
/// process enumeration in a clear are exercised.
/// </summary>
[TestFixture]
[Platform("Win")]
public class AppRuleAssignmentReconcilerTests
{
    private const string RenderToken =
        @"\\?\SWD#MMDEVAPI#{0.0.0.00000000}.{11111111-2222-3333-4444-555555555555}#e6327cad-dcec-4949-ae8a-991e976a79d2";
    private const string CaptureToken =
        @"\\?\SWD#MMDEVAPI#{0.0.1.00000000}.{aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee}#2eef81be-33fa-4800-9670-1cd474972c3f";

    private RegistryKey _scratchRoot;
    private string _scratchKeyName;
    private string _scratchStorePath;
    private SoundSwitchConfiguration _configuration;
    private StatefulPolicyConfig _policy;
    private AppRuleAssignmentReconciler _reconciler;

    [SetUp]
    public void SetUp()
    {
        _scratchKeyName = Guid.NewGuid().ToString("N");
        _scratchRoot = Registry.CurrentUser.CreateSubKey($"Software\\SoundSwitch\\Tests\\AppRuleAssignmentReconciler\\{_scratchKeyName}");
        _scratchStorePath = $@"HKEY_CURRENT_USER\Software\SoundSwitch\Tests\AppRuleAssignmentReconciler\{_scratchKeyName}";
        _configuration = new SoundSwitchConfiguration();
        _policy = new StatefulPolicyConfig();
        AudioSwitcher.Instance.SetExtendedPolicyClientForTest(new ExtendedPolicyClient(_policy));
        _reconciler = new AppRuleAssignmentReconciler(_configuration, AudioSwitcher.Instance,
            new PersistedDefaultEndpointStore(_scratchStorePath));
    }

    [TearDown]
    public void TearDown()
    {
        AudioSwitcher.Instance.SetExtendedPolicyClientForTest(null);
        _scratchRoot.Dispose();
        Registry.CurrentUser.DeleteSubKeyTree($"Software\\SoundSwitch\\Tests\\AppRuleAssignmentReconciler\\{_scratchKeyName}", false);
    }

    // ── helpers ─────────────────────────────────────────────────────

    private static AppSoundRule DynamicRule(string processPattern) => new()
    {
        Enabled = true,
        Mode = AppRuleMode.Automatic,
        ProcessPath = processPattern
    };

    private static AppSoundRule AssignmentRule(string exePath) => new()
    {
        Enabled = true,
        Enforcement = AppRuleEnforcement.Assignment,
        ProcessPath = exePath,
        AssignmentExePath = exePath
    };

    private void WriteStoreRecord(string keyName, string ntExePath)
    {
        using var key = _scratchRoot.CreateSubKey(keyName);
        key.SetValue(null, ntExePath, RegistryValueKind.String);
        key.SetValue("000_000", RenderToken, RegistryValueKind.String);
    }

    /// <summary>Write a store record whose identity is the drive-letter form of <paramref name="letterPath"/>.</summary>
    private void WriteStoreRecordFor(string letterPath)
    {
        WriteStoreRecord(Guid.NewGuid().ToString("N"), $@"\Device\HarddiskVolume3\{letterPath[2..]}");
    }

    /// <summary>A real live sleeper process; returns its image path and pid.</summary>
    private (string Path, int Pid) LaunchSleeper()
    {
        var process = Process.Start(new ProcessStartInfo("cmd.exe", "/c ping -n 30 127.0.0.1 > NUL")
        {
            CreateNoWindow = true,
            UseShellExecute = false
        });
        process.Should().NotBeNull();
        _sleepers.Add(process!);

        var spin = Stopwatch.StartNew();
        string path = null;
        while (spin.Elapsed < TimeSpan.FromSeconds(10))
        {
            try { path = process.MainModule?.FileName; } catch { /* not ready yet */ }
            if (!string.IsNullOrEmpty(path)) break;
            Thread.Sleep(50);
        }

        path.Should().NotBeNullOrEmpty("the sleeper's image path is the identity under test");
        return (path, process.Id);
    }

    private readonly List<Process> _sleepers = new();

    [TearDown]
    public void KillSleepers()
    {
        foreach (var process in _sleepers.Where(process => !process.HasExited))
        {
            try { process.Kill(); } catch { /* already gone */ }
            process.Dispose();
        }
        _sleepers.Clear();
    }

    // ── adoption & windows-assignments merge ────────────────────────

    [Test]
    public void Reconcile_AdoptsPreLedgerRecordMatchingEnabledRule()
    {
        WriteStoreRecord("abc0001_0", @"\Device\HarddiskVolume3\Apps\Spotify\Spotify.exe");
        var rule = DynamicRule(".*Spotify.exe.*");
        _configuration.AppSoundRules.Add(rule);

        _reconciler.Reconcile();

        _configuration.AssignmentLedger.Should().ContainSingle();
        var entry = _configuration.AssignmentLedger.Single();
        entry.RuleId.Should().Be(rule.Id);
        entry.Enforcement.Should().Be(AppRuleEnforcement.Dynamic);
        AppRuleIdentityMatcher.SameApp(entry.ExePath, @"\Device\HarddiskVolume3\Apps\Spotify\Spotify.exe").Should().BeTrue();
        _reconciler.TakeDirty().Should().BeTrue();
    }

    [Test]
    public void Reconcile_AdoptionIsIdempotent()
    {
        WriteStoreRecord("abc0002_0", @"\Device\HarddiskVolume3\Apps\Spotify\Spotify.exe");
        _configuration.AppSoundRules.Add(DynamicRule(".*Spotify.exe.*"));

        _reconciler.Reconcile();
        _reconciler.TakeDirty().Should().BeTrue();

        _reconciler.Reconcile();
        _reconciler.TakeDirty().Should().BeFalse();
        _configuration.AssignmentLedger.Should().ContainSingle();
    }

    [Test]
    public void GetWindowsAssignments_UserRecordsNotOwnedByRules_AreListed()
    {
        WriteStoreRecord("abc0003_0", @"\Device\HarddiskVolume3\Apps\OnlyInWindows\WindowsApp.exe");

        var assignments = _reconciler.GetWindowsAssignments();

        assignments.Should().ContainSingle();
        assignments[0].RelativePath.Should().Be(@"Apps\OnlyInWindows\WindowsApp.exe");
        assignments[0].PlaybackDeviceId.Should().Be("{0.0.0.00000000}.{11111111-2222-3333-4444-555555555555}");
        _configuration.AssignmentLedger.Should().BeEmpty("display is a pure read — nothing adopted");
    }

    [Test]
    public void GetWindowsAssignments_RuleOwnedRecord_NeverListed()
    {
        WriteStoreRecord("abc0004_0", @"\Device\HarddiskVolume3\Apps\Spotify\Spotify.exe");
        _configuration.AppSoundRules.Add(DynamicRule(".*Spotify.exe.*"));

        // adoption marks the record ours
        _reconciler.Reconcile();
        _reconciler.TakeDirty();

        _reconciler.GetWindowsAssignments().Should().BeEmpty();
    }

    [Test]
    public void GetWindowsAssignments_LedgerCoveredRecordWithLetterPath_MatchesStoreNtPath()
    {
        WriteStoreRecord("abc0005_0", @"\Device\HarddiskVolume3\Apps\Spotify\Spotify.exe");
        // An entry carrying the volume-relative letter-form of the same identity.
        _configuration.AssignmentLedger.Add(new AssignmentLedgerEntry
        {
            RuleId = Guid.NewGuid(),
            ExePath = @"C:\Apps\Spotify\Spotify.exe",
            Enforcement = AppRuleEnforcement.Assignment,
            PlaybackDeviceId = "playback-id"
        });

        _reconciler.GetWindowsAssignments().Should().BeEmpty("one identity, one row (ADR §11.5)");
    }

    [Test]
    public void GetWindowsAssignments_DisabledRuleDoesNotOwn()
    {
        WriteStoreRecord("abc0006_0", @"\Device\HarddiskVolume3\Apps\Spotify\Spotify.exe");
        _configuration.AppSoundRules.Add(new AppSoundRule { Enabled = false, ProcessPath = ".*Spotify.exe.*" });

        _reconciler.GetWindowsAssignments().Should().ContainSingle();
    }

    // ── stale-drop ──────────────────────────────────────────────────

    [Test]
    public void Reconcile_DropsEntryWhenItsRecordVanished()
    {
        _configuration.AssignmentLedger.Add(new AssignmentLedgerEntry
        {
            RuleId = Guid.NewGuid(),
            ExePath = @"C:\Apps\Spotify\Spotify.exe",
            Enforcement = AppRuleEnforcement.Assignment
        });

        _reconciler.Reconcile();

        _configuration.AssignmentLedger.Should().BeEmpty();
        _reconciler.TakeDirty().Should().BeTrue();
    }

    [Test]
    public void Reconcile_DroppedEntryIsIdempotentAndStoreIsUntouched()
    {
        _configuration.AssignmentLedger.Add(new AssignmentLedgerEntry
        {
            RuleId = Guid.NewGuid(),
            ExePath = @"C:\Apps\Spotify\Spotify.exe",
            Enforcement = AppRuleEnforcement.Dynamic
        });

        _reconciler.Reconcile();

        _configuration.AssignmentLedger.Should().BeEmpty();
        new PersistedDefaultEndpointStore(_scratchStorePath).ReadAll().Should().BeEmpty("the reconciler never writes Windows' store");
        _reconciler.TakeDirty().Should().BeTrue();
        _reconciler.Reconcile();
        _reconciler.TakeDirty().Should().BeFalse();
    }

    [Test]
    public void Reconcile_LiveRuleReleaseKeepsRecord()
    {
        WriteStoreRecord("abc0007_0", @"\Device\HarddiskVolume3\Apps\Spotify\Spotify.exe");
        var rule = DynamicRule(".*Spotify.exe.*");

        // The record of the rule was adopted (pre-ledger), then the rule is deleted.
        _configuration.AppSoundRules.Add(rule);
        _reconciler.Reconcile();
        _reconciler.TakeDirty();

        _configuration.AppSoundRules.Remove(rule);
        _reconciler.ClearRuleAssignments(rule);

        _configuration.AssignmentLedger.Should().BeEmpty("a removed dynamic rule releases its claim");
        new PersistedDefaultEndpointStore(_scratchStorePath).ReadAll().Should().HaveCount(1, "the leftover route keeps today's delete behaviour");
    }

    // ── deferred restore-default clear (live and pending) ───────────

    [Test]
    public void Reconcile_DeletedAssignmentRule_ClearsRunningAppImmediately()
    {
        var (path, pid) = LaunchSleeper();
        WriteStoreRecordFor(path); // the route's record still exists in Windows' store
        _policy.SetDevice(pid, EDataFlow.eRender, RenderToken);
        _policy.SetDevice(pid, EDataFlow.eCapture, CaptureToken);
        _configuration.AssignmentLedger.Add(new AssignmentLedgerEntry
        {
            RuleId = Guid.NewGuid(),
            ExePath = path,
            Enforcement = AppRuleEnforcement.Assignment,
            PlaybackDeviceId = "playback-id",
            RecordingDeviceId = "capture-id"
        });

        _reconciler.Reconcile();

        _configuration.AssignmentLedger.Should().BeEmpty();
        _policy.GetDevice(pid, EDataFlow.eRender).Should().BeNull("the route was cleared for the running app");
        _policy.GetDevice(pid, EDataFlow.eCapture).Should().BeNull();
    }

    [Test]
    public void Reconcile_DisabledAssignmentRuleWithRunningApp_RestoresDefault()
    {
        var (path, pid) = LaunchSleeper();
        WriteStoreRecordFor(path);
        _policy.SetDevice(pid, EDataFlow.eRender, RenderToken);
        var rule = AssignmentRule(path);
        rule.Enabled = false;
        _configuration.AppSoundRules.Add(rule);
        _configuration.AssignmentLedger.Add(new AssignmentLedgerEntry
        {
            RuleId = rule.Id,
            ExePath = path,
            Enforcement = AppRuleEnforcement.Assignment,
            PlaybackDeviceId = "playback-id"
        });

        _reconciler.Reconcile();

        _configuration.AssignmentLedger.Should().BeEmpty();
        _policy.GetDevice(pid, EDataFlow.eRender).Should().BeNull();
    }

    [Test]
    public void Reconcile_DeletedAssignmentRuleWithClosedApp_KeepsPendingClear()
    {
        WriteStoreRecordFor(@"C:\Apps\Not Running\RareApp.exe");
        _configuration.AssignmentLedger.Add(new AssignmentLedgerEntry
        {
            RuleId = Guid.NewGuid(),
            ExePath = @"C:\Apps\Not Running\RareApp.exe",
            Enforcement = AppRuleEnforcement.Assignment,
            PlaybackDeviceId = "playback-id"
        });

        _reconciler.Reconcile();

        _configuration.AssignmentLedger.Should().ContainSingle("a closed app keeps its route until its next sighting (ADR §6.3)");
        _reconciler.TakeDirty().Should().BeFalse("nothing changed on this pass");
    }

    [Test]
    public void ClearRuleAssignments_AssignmentRuleWithRunningApp_ClearsRoute()
    {
        var (path, pid) = LaunchSleeper();
        WriteStoreRecordFor(path);
        _policy.SetDevice(pid, EDataFlow.eRender, RenderToken);
        var rule = AssignmentRule(path);
        _configuration.AppSoundRules.Add(rule);
        _configuration.AssignmentLedger.Add(new AssignmentLedgerEntry
        {
            RuleId = rule.Id,
            ExePath = path,
            Enforcement = AppRuleEnforcement.Assignment,
            PlaybackDeviceId = "playback-id"
        });

        _reconciler.ClearRuleAssignments(rule);

        _policy.GetDevice(pid, EDataFlow.eRender).Should().BeNull();
        _configuration.AssignmentLedger.Should().BeEmpty("the route was cleared while the app was running");
    }

    [Test]
    public void ClearRuleAssignments_AssignmentRuleWithClosedApp_KeepsPending()
    {
        var rule = AssignmentRule(@"C:\Apps\Not Running\RareApp.exe");
        _configuration.AssignmentLedger.Add(new AssignmentLedgerEntry
        {
            RuleId = rule.Id,
            ExePath = rule.AssignmentExePath,
            Enforcement = AppRuleEnforcement.Assignment,
            PlaybackDeviceId = "playback-id"
        });

        _reconciler.ClearRuleAssignments(rule);

        _configuration.AssignmentLedger.Should().ContainSingle();
    }

    // ── policy client behaviour ─────────────────────────────────────

    [Test]
    public void ClearDefaultEndPoint_ClearsStateViaEngineOnly()
    {
        var (path, pid) = LaunchSleeper();
        _policy.SetDevice(pid, EDataFlow.eRender, RenderToken);

        AudioSwitcher.Instance.ClearProcessDefaultEndpoint(EDataFlow.eRender, (uint)pid).Should().BeTrue();
        _policy.GetDevice(pid, EDataFlow.eRender).Should().BeNull();
    }

    [Test]
    public void ClearDefaultEndPoint_FailsForPidWithoutSession()
    {
        // A pid without an audio session refuses the write (E_INVALIDARG) — nothing cleared.
        AudioSwitcher.Instance.ClearProcessDefaultEndpoint(EDataFlow.eRender, uint.MaxValue).Should().BeFalse();
    }

    /// <summary>
    /// Stateful fake of <see cref="IAudioPolicyConfig"/> that mirrors the engine: only pids the
    /// test registered as audio-capable accept writes, a null/empty device clears, and Get of
    /// a missing entry reports the no-audio-session shape (null).
    /// </summary>
    private sealed class StatefulPolicyConfig : IAudioPolicyConfig
    {
        private readonly Dictionary<(int Pid, EDataFlow Flow, ERole Role), string> _map = new();
        private readonly HashSet<int> _livePids = new();

        public void SetDevice(int pid, EDataFlow flow, string token)
        {
            _livePids.Add(pid);
            _map[(pid, flow, ERole.eConsole)] = token;
        }

        public string? GetDevice(int pid, EDataFlow flow)
        {
            return _map.TryGetValue((pid, flow, ERole.eConsole), out var token) ? token : null;
        }

        public bool SetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, string deviceId)
        {
            if (!_livePids.Contains((int)processId))
                return false; // E_INVALIDARG: pid without audio session

            if (string.IsNullOrEmpty(deviceId))
            {
                _map.Remove(((int)processId, flow, role));
                return true;
            }

            _map[((int)processId, flow, role)] = deviceId;
            return true;
        }

        public string? GetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role)
        {
            return _map.TryGetValue(((int)processId, flow, role), out var token) ? token : null;
        }

        public void ClearAllPersistedApplicationDefaultEndpoints() => _map.Clear();

        public void Dispose()
        {
        }
    }
}
