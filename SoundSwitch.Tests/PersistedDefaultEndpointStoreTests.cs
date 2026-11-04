using System;
using System.Linq;

using FluentAssertions;

using Microsoft.Win32;

using NUnit.Framework;

using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Tests;

[TestFixture]
public class PersistedDefaultEndpointStoreTests
{
    private const string ScratchRootPath = @"HKEY_CURRENT_USER\Software\SoundSwitch\Tests\PersistedDefaultEndpointStore";

    private const string RenderToken =
        @"\\?\SWD#MMDEVAPI#{0.0.0.00000000}.{11111111-2222-3333-4444-555555555555}#e6327cad-dcec-4949-ae8a-991e976a79d2";
    private const string CaptureToken =
        @"\\?\SWD#MMDEVAPI#{0.0.0.00000000}.{aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee}#2eef81be-33fa-4800-9670-1cd474972c3f";
    private const string UnknownInterfaceToken =
        @"\\?\SWD#MMDEVAPI#{0.0.0.00000000}.{99999999-8888-7777-6666-555555555555}#deadedea-1234-5678-90ab-cdef12345678";

    private const string RenderInnerId = "{0.0.0.00000000}.{11111111-2222-3333-4444-555555555555}";
    private const string CaptureInnerId = "{0.0.0.00000000}.{aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee}";

    private RegistryKey _scratchRoot;
    private string _scratchKeyName;

    [SetUp]
    public void SetUp()
    {
        _scratchKeyName = Guid.NewGuid().ToString("N");
        _scratchRoot = Registry.CurrentUser.CreateSubKey(
            $"Software\\SoundSwitch\\Tests\\PersistedDefaultEndpointStore\\{_scratchKeyName}");
    }

    [TearDown]
    public void TearDown()
    {
        _scratchRoot.Dispose();
        Registry.CurrentUser.DeleteSubKeyTree(
            $"Software\\SoundSwitch\\Tests\\PersistedDefaultEndpointStore\\{_scratchKeyName}", false);
    }

    private PersistedDefaultEndpointStore CreateStore(string subPath = null)
    {
        var storePath = subPath == null
            ? $@"HKEY_CURRENT_USER\Software\SoundSwitch\Tests\PersistedDefaultEndpointStore\{_scratchKeyName}"
            : $@"HKEY_CURRENT_USER\Software\SoundSwitch\Tests\PersistedDefaultEndpointStore\{_scratchKeyName}\{subPath}";
        return new PersistedDefaultEndpointStore(storePath);
    }

    private void WriteRecord(string keyName, string exePath, params (string Name, string Value)[] values)
    {
        using var key = _scratchRoot.CreateSubKey(keyName);
        if (exePath != null)
            key.SetValue(null, exePath, RegistryValueKind.String);
        foreach (var (name, value) in values)
        {
            key.SetValue(name, value, RegistryValueKind.String);
        }
    }

    [Test]
    public void ReadAll_ShouldParseRecordWithRoleDevicePairs()
    {
        WriteRecord("505ccc8f_0", @"\Device\HarddiskVolume3\Apps\Spotify\Spotify.exe",
            ("000_000", RenderToken),
            ("001_000", RenderToken),
            ("002_000", RenderToken),
            ("000_000_p", "{abc00000-0000-0000-0000-000000000000}"), // paired metadata
            ("001_000_p", "{abc00000-0000-0000-0000-000000000000}"),
            ("002_000_p", "{abc00000-0000-0000-0000-000000000000}"));

        var records = CreateStore().ReadAll();

        records.Should().HaveCount(1);
        var record = records[0];
        record.ExePath.Should().Be(@"\Device\HarddiskVolume3\Apps\Spotify\Spotify.exe");
        record.Flow.Should().Be(EDataFlow.eRender);
        record.DeviceIds.Keys.Should().BeEquivalentTo(new[] {ERole.eConsole, ERole.eMultimedia, ERole.eCommunications});
        record.DeviceIds.Should().OnlyContain(pair => pair.Value == RenderInnerId);
    }

    [Test]
    public void ReadAll_ShouldDeriveFlowFromTokenInterfaceSuffix_NotFromKeyName()
    {
        // The numeric key suffix is not relied upon: a "_1" key whose tokens are capture endpoints
        // must still be classified as eCapture (and a "_0" key with capture tokens likewise).
        WriteRecord("abcdef01_0", @"\Device\HarddiskVolume3\Apps\Teams\Teams.exe", ("001_000", CaptureToken));
        WriteRecord("abcdef02_1", @"\Device\HarddiskVolume3\Apps\Zoom\Zoom.exe", ("001_000", CaptureToken));

        var records = CreateStore().ReadAll();

        records.Should().HaveCount(2);
        records.Should().OnlyContain(record => record.Flow == EDataFlow.eCapture);
        records.Select(record => record.DeviceIds[ERole.eMultimedia])
            .Should().BeEquivalentTo(CaptureInnerId, CaptureInnerId);
    }

    [Test]
    public void ReadAll_ShouldIgnoreUnknownValueShapes()
    {
        WriteRecord("cafe0001_0", @"\Device\HarddiskVolume3\Apps\App\App.exe",
            ("000_000", RenderToken),
            ("000_000_p", "{abc00000-0000-0000-0000-000000000000}"),
            ("junk_value", RenderToken),
            ("123_999", RenderToken),      // not a role-device pair
            ("003_000_p", RenderToken));

        var records = CreateStore().ReadAll();

        records.Should().HaveCount(1);
        records[0].DeviceIds.Keys.Should().BeEquivalentTo(new[] {ERole.eConsole});
    }

    [Test]
    public void ReadAll_ShouldSkipValueWithUnknownInterfaceSuffix()
    {
        WriteRecord("cafe0002_0", @"\Device\HarddiskVolume3\Apps\App\App.exe",
            ("001_000", UnknownInterfaceToken));

        var records = CreateStore().ReadAll();

        records.Should().BeEmpty();
    }

    [Test]
    public void ReadAll_ShouldSkipRecordWithoutExecutablePath()
    {
        _scratchRoot.CreateSubKey("cafe0003_0").Dispose(); // only a default-less empty key
        WriteRecord("cafe0004_0", null, ("000_000", RenderToken));

        var records = CreateStore().ReadAll();

        records.Should().BeEmpty();
    }

    [Test]
    public void ReadAll_ShouldSkipRecordWithNonStringExecutablePath()
    {
        using var key = _scratchRoot.CreateSubKey("cafe0005_0");
        key.SetValue(null, 42, RegistryValueKind.DWord);
        key.SetValue("000_000", RenderToken, RegistryValueKind.String);

        var records = CreateStore().ReadAll();

        records.Should().BeEmpty();
    }

    [Test]
    public void ReadAll_ShouldTolerateEmptyAndMalformedDeviceValues()
    {
        WriteRecord("cafe0006_0", @"\Device\HarddiskVolume3\Apps\App\App.exe",
            ("000_000", RenderToken),
            ("001_000", ""),                // empty device value
            ("002_000", "not-a-token"));    // malformed device value

        var records = CreateStore().ReadAll();

        records.Should().HaveCount(1);
        records[0].Flow.Should().Be(EDataFlow.eRender);
        records[0].DeviceIds.Keys.Should().BeEquivalentTo(new[] {ERole.eConsole});
    }

    [Test]
    public void ReadAll_ShouldReturnEmptyWhenStoreDoesNotExist()
    {
        var records = CreateStore("does-not-exist").ReadAll();

        records.Should().BeEmpty();
    }

    [Test]
    public void ReadAll_ShouldReturnRecordsSortedByExecutablePath()
    {
        WriteRecord("b_key", @"\Device\HarddiskVolume3\B\App.exe", ("001_000", RenderToken));
        WriteRecord("a_key", @"\Device\HarddiskVolume3\A\App.exe", ("001_000", RenderToken));

        var records = CreateStore().ReadAll();

        records.Select(record => record.ExePath).Should().Equal(
            @"\Device\HarddiskVolume3\A\App.exe",
            @"\Device\HarddiskVolume3\B\App.exe");
    }
}
