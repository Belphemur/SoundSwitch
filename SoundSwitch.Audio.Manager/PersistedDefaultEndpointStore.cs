using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.Win32;

using Serilog;

using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Audio.Manager
{
    /// <summary>
    /// Read-only enumeration of Windows' per-app persisted default endpoints:
    /// <c>HKCU\Software\Microsoft\Multimedia\Audio\DefaultEndpoint\&lt;hash&gt;_&lt;N&gt;</c>.
    /// This registry store is what feeds Windows Settings' "App volume and device preferences" list
    /// and is written by the audio engine every time a per-app default is assigned (including
    /// SoundSwitch's <c>SwitchProcessTo</c>). Records outlive the process, which is what lets
    /// Windows (and this picker) list apps that aren't running anymore.
    ///
    /// Layout observed on Windows 11 25H2 (see the app-rules picker design doc): the key's default
    /// value holds the application identity in NT device form (<c>\Device\HarddiskVolume3\...exe</c>)
    /// and values named <c>&lt;role-index&gt;_000</c> hold the endpoint for the matching
    /// <see cref="ERole"/> as a full MMDEVAPI device token. Each token's trailing interface GUID
    /// (render <c>e6327cad-dcec-4949-ae8a-991e976a79d2</c>, capture
    /// <c>2eef81be-33fa-4800-9670-1cd474972c3f</c>) is the only reliable direction signal, so it is
    /// the one used here. Values whose name or shape does not match the observed layout are ignored,
    /// and <see cref="ReadAll"/> never throws for ordinary drift (the store format is undocumented).
    ///
    /// Pure registry read: no COM, no <c>ComThread</c>.
    /// </summary>
    public sealed class PersistedDefaultEndpointStore
    {
        private static readonly ILogger _logger = Log.ForContext<PersistedDefaultEndpointStore>();

        /// <summary>
        /// The registry path Windows writes per-app defaults to. Tests inject a scratch path instead.
        /// </summary>
        public const string DefaultStorePath = @"HKEY_CURRENT_USER\Software\Microsoft\Multimedia\Audio\DefaultEndpoint";

        private const string DeviceTokenPrefix = @"\\?\SWD#MMDEVAPI#";
        private const string RenderInterfaceId = "e6327cad-dcec-4949-ae8a-991e976a79d2";
        private const string CaptureInterfaceId = "2eef81be-33fa-4800-9670-1cd474972c3f";

        /// <summary>Values named <c>&lt;role-index&gt;_000</c>; the paired <c>&lt;role-index&gt;_000_p</c> metadata values don't match.</summary>
        private static readonly Regex DeviceValueNameRegex = new(@"^(\d{3})_000$", RegexOptions.Compiled);

        private readonly string _storePath;

        /// <summary>Reads the real per-user store (see <see cref="DefaultStorePath"/>).</summary>
        public PersistedDefaultEndpointStore() : this(DefaultStorePath)
        {
        }

        /// <param name="storePath">
        /// Absolute registry path of the store root, prefixed with the hive name, e.g.
        /// <c>HKEY_CURRENT_USER\Software\Microsoft\Multimedia\Audio\DefaultEndpoint</c>.
        /// Only <c>HKEY_CURRENT_USER</c> is supported; the per-app store is a per-user value.
        /// </param>
        public PersistedDefaultEndpointStore(string storePath)
        {
            _storePath = storePath;
        }

        /// <summary>
        /// One entry per <c>&lt;hash&gt;_&lt;N&gt;</c> record in the store.
        /// <see cref="DeviceIds"/> maps the roles Windows wrote (<see cref="ERole.eConsole"/>,
        /// <see cref="ERole.eMultimedia"/>, <see cref="ERole.eCommunications"/>) to the endpoint
        /// device id in the standard MMDevice id form the rest of the audio components expect.
        /// Missing roles simply aren't in the map.
        /// </summary>
        public sealed record Record(string ExePath, EDataFlow Flow, IReadOnlyDictionary<ERole, string> DeviceIds);

        /// <summary>
        /// Enumerate every record currently in the store, sorted by executable path so callers don't
        /// depend on registry enumeration order. Unknown shapes are skipped.
        /// </summary>
        /// <returns>
        /// The records, or <c>null</c> when the enumeration itself failed (access error). Only a
        /// store that legitimately holds no records yields an empty result — a caller must treat
        /// <c>null</c> as "state known, not as an empty store".
        /// </returns>
        public IReadOnlyList<Record>? TryReadAll()
        {
            try
            {
                using var root = OpenStoreRoot(_storePath);
                if (root == null)
                {
                    _logger.Debug("Persisted default endpoint store not present at {StorePath}", _storePath);
                    return Array.Empty<Record>();
                }

                var records = new List<Record>();
                foreach (var keyName in root.GetSubKeyNames())
                {
                    try
                    {
                        using var key = root.OpenSubKey(keyName);
                        var record = ReadRecord(key);
                        if (record != null)
                            records.Add(record);
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug(ex, "Skipping unreadable persisted endpoint record {Key}", keyName);
                    }
                }

                records.Sort((a, b) => string.Compare(a.ExePath, b.ExePath, StringComparison.OrdinalIgnoreCase));
                return records;
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Failed to enumerate the persisted default endpoint store at {StorePath}; treating this pass as unreadable, not empty", _storePath);
                return null;
            }
        }

        /// <summary>
        /// Enumerate every record currently in the store (see <see cref="TryReadAll"/>), taking an
        /// enumeration failure as an empty store — for readers that merely display the store, where
        /// the distinction does not matter.
        /// </summary>
        public IReadOnlyList<Record> ReadAll()
        {
            return TryReadAll() ?? Array.Empty<Record>();
        }

        /// <summary>
        /// Strip the NT device volume prefix (<c>\Device\HarddiskVolume&lt;N&gt;\</c>) off a store
        /// exe path, leaving the volume-relative remainder that both the picker and the
        /// app-rules reconciler compare case-insensitively. Empty when the path has no
        /// recognizable volume prefix.
        /// </summary>
        public static string GetVolumeRelativePath(string? ntPath)
        {
            const string volumePrefix = @"\Device\HarddiskVolume";
            if (string.IsNullOrEmpty(ntPath) || !ntPath.StartsWith(volumePrefix, StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            var index = volumePrefix.Length;
            while (index < ntPath.Length && char.IsDigit(ntPath[index]))
            {
                index++;
            }

            if (index < ntPath.Length && ntPath[index] == '\\')
                return ntPath.Substring(index + 1);

            return string.Empty;
        }

        /// <summary>
        /// Open a <c>HKEY_CURRENT_USER\...</c> style registry path read-only.
        /// Returns null when the key doesn't exist; other hives are unsupported by design.
        /// </summary>
        private static RegistryKey? OpenStoreRoot(string storePath)
        {
            var hiveSeparator = storePath.IndexOf('\\');
            if (hiveSeparator <= 0)
            {
                _logger.Debug("Unsupported registry path {StorePath}: expected a hive name prefix", storePath);
                return null;
            }

            var hiveName = storePath.Substring(0, hiveSeparator);
            if (string.Equals(hiveName, "HKEY_CURRENT_USER", StringComparison.OrdinalIgnoreCase))
                return Registry.CurrentUser.OpenSubKey(storePath.Substring(hiveSeparator + 1));

            _logger.Debug("Unsupported registry hive {Hive} for {StorePath}", hiveName, storePath);
            return null;
        }

        private static Record? ReadRecord(RegistryKey? key)
        {
            if (key == null)
                return null;

            if (!(key.GetValue(null) is string exePath) || exePath.Length == 0)
                return null;

            var deviceIds = new Dictionary<ERole, string>();
            EDataFlow? flow = null;
            foreach (var valueName in key.GetValueNames())
            {
                var match = DeviceValueNameRegex.Match(valueName);
                if (!match.Success)
                    continue; // unknown shape (also skips the paired <NNN>_000_p metadata values)

                if (!(key.GetValue(valueName) is string token) || token.Length == 0)
                    continue;

                var role = GetRole(match.Groups[1].Value);
                if (!role.HasValue)
                    continue; // unknown role index

                if (!TryGetFlow(token, out var tokenFlow))
                    continue; // unrecognizable device token

                flow ??= tokenFlow;
                // A token whose direction contradicts the record's first recognized flow is
                // ordinary drift (the engine keys a record per flow); keep the role map
                // consistent with the record's flow instead of surfacing the capture id under
                // a render record (or vice versa).
                if (tokenFlow != flow.Value)
                    continue; // drift: role token of the other flow

                deviceIds[role.Value] = UnwrapDeviceToken(token);            }

            // The direction signal is the device token itself; without any recognizable token a
            // record carries no direction and is not surfaced (ordinary drift, not an error).
            if (!flow.HasValue)
                return null;

            return new Record(exePath, flow.Value, deviceIds);
        }

        private static ERole? GetRole(string roleIndex)
        {
            switch (roleIndex)
            {
                case "000": return ERole.eConsole;
                case "001": return ERole.eMultimedia;
                case "002": return ERole.eCommunications;
                default: return null;
            }
        }

        /// <summary>
        /// Map a device token to its direction by interface suffix — the only reliable signal per
        /// the design doc: render <c>#e6327cad…</c>, capture <c>#2eef81be…</c>.
        /// </summary>
        private static bool TryGetFlow(string token, out EDataFlow flow)
        {
            flow = default;
            if (string.IsNullOrEmpty(token) || !token.StartsWith(DeviceTokenPrefix, StringComparison.OrdinalIgnoreCase))
                return false;

            var interfaceIndex = token.LastIndexOf('#');
            if (interfaceIndex < 0)
                return false;

            // The engine writes the trailing interface id brace-wrapped
            // (<token>#<interface with braces>); normalize so both shapes compare.
            var interfaceId = token.Substring(interfaceIndex + 1).Trim('{', '}');
            if (string.Equals(interfaceId, RenderInterfaceId, StringComparison.OrdinalIgnoreCase))
            {
                flow = EDataFlow.eRender;
                return true;
            }

            if (string.Equals(interfaceId, CaptureInterfaceId, StringComparison.OrdinalIgnoreCase))
            {
                flow = EDataFlow.eCapture;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Strip the symbolic-link wrapper off an MMDEVAPI token
        /// (<c>\\?\SWD#MMDEVAPI#{0.0.0.00000000}.&lt;guid&gt;#&lt;interface&gt;</c>) to get the plain
        /// MMDevice id (<c>{0.0.0.00000000}.&lt;guid&gt;</c>) the device enumerators use.
        /// Returns the token unchanged when it doesn't match the observed shape.
        /// </summary>
        private static string UnwrapDeviceToken(string token)
        {
            var interfaceIndex = token.LastIndexOf('#');
            if (!token.StartsWith(DeviceTokenPrefix, StringComparison.OrdinalIgnoreCase) ||
                interfaceIndex <= DeviceTokenPrefix.Length - 1)
                return token;

            var inner = token.Substring(DeviceTokenPrefix.Length, interfaceIndex - DeviceTokenPrefix.Length);
            return inner.Length > 0 ? inner : token;
        }
    }
}
