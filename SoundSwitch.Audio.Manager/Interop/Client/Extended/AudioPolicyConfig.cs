using System;
using System.Linq;
using System.Runtime.InteropServices;
using SoundSwitch.Audio.Manager.Interop.Com.Base;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Audio.Manager.Interop.Interface.Policy.Extended;

namespace SoundSwitch.Audio.Manager.Interop.Client.Extended
{
    public sealed class AudioPolicyConfig : IAudioPolicyConfig
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate HRESULT QueryInterface(IntPtr audioPolicyConfigFactory, IntPtr guid, ref IntPtr audioPolicyConfig);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate HRESULT GetPersistedDefaultAudioEndpointDelegate(IntPtr audioPolicyConfigFactory, uint processId, EDataFlow flow, ERole role, ref HSTRING deviceId);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate HRESULT SetPersistedDefaultAudioEndpointDelegate(IntPtr audioPolicyConfigFactory, uint processId, EDataFlow flow, ERole role, HSTRING deviceId);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate HRESULT ClearAllPersistedApplicationDefaultEndpointsDelegate(IntPtr audioPolicyConfigFactory);

        private readonly int _ptrSize;
        private readonly IntPtr _factory;
        private readonly IntPtr _vfTable;

        private const int GUID_SIZE = 16;

        private readonly string[] _knownValidGuids =
        {
            // Pre-H1H2
            "2a59116d-6c4f-45e0-a74f-707e3fef9258",
            // H1H2/Win11
            "ab3d4648-e242-459f-b02f-541c70306324",
            //Windows 10.0.16299
            "32aa8e18-6496-4e24-9f94-b800e7eccc45"
        };

        public AudioPolicyConfig()
        {
            _ptrSize = Marshal.SizeOf<IntPtr>();
            using var name = HSTRING.FromString("Windows.Media.Internal.AudioPolicyConfig");
            AudioSes.DllGetActivationFactory(name, out _factory);
            var iinspectable = (IInspectableSlim)Marshal.GetObjectForIUnknown(_factory);
            var count = Marshal.AllocHGlobal(_ptrSize);
            var iids = Marshal.AllocHGlobal(_ptrSize);

            try
            {
                var result = iinspectable.GetIids(count, ref iids);
                if (result != HRESULT.S_OK)
                    throw new InvalidComObjectException($"Can't get IIDS for {nameof(AudioPolicyConfig)}: {result}");

                var totalGuids = Marshal.ReadInt32(count);
                if (totalGuids <= 0)
                    throw new InvalidComObjectException($"Invalid number of GUIDs found: {totalGuids}");

                var audioPolicyConfigGuidPtr = iids + GUID_SIZE * (totalGuids - 1);
                var audioPolicyConfigGuid = GuidPtrToString(audioPolicyConfigGuidPtr);
                if (!_knownValidGuids.Contains(audioPolicyConfigGuid))
                    throw new InvalidComObjectException($"Unknown AudioPolicyConfig GUID: {audioPolicyConfigGuid}");

                var vftable = Marshal.PtrToStructure<IntPtr>(_factory);
                var queryInterfacePtr = Marshal.PtrToStructure<IntPtr>(vftable);
                var queryInterface = Marshal.GetDelegateForFunctionPointer<QueryInterface>(queryInterfacePtr);

                result = queryInterface(_factory, audioPolicyConfigGuidPtr, ref _factory);
                if (result != HRESULT.S_OK)
                    throw new InvalidComObjectException($"{nameof(QueryInterface)} for {nameof(AudioPolicyConfig)} failed: {result}");
                _vfTable = Marshal.PtrToStructure<IntPtr>(_factory);
            }
            finally
            {
                Marshal.FreeHGlobal(count);
                Marshal.FreeHGlobal(iids);
            }
        }

        private static string GuidPtrToString(IntPtr guidPtr)
        {
            var buffer = new byte[GUID_SIZE];
            Marshal.Copy(guidPtr, buffer, 0, GUID_SIZE);
            var gchandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                var guid = (Guid) (Marshal.PtrToStructure(gchandle.AddrOfPinnedObject(), typeof(Guid)) ?? throw new Exception($"Unable to convert GUID"));
                return $"{guid.Data1:x8}-{guid.Data2:x4}-{guid.Data3:x4}-{guid.Data4[0]:x2}{guid.Data4[1]:x2}-{guid.Data4[2]:x2}{guid.Data4[3]:x2}{guid.Data4[4]:x2}{guid.Data4[5]:x2}{guid.Data4[6]:x2}{guid.Data4[7]:x2}";
            }
            finally
            {
                gchandle.Free();
            }
        }

        public string GetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role)
        {
            var getPersistedDefaultAudioEndpointPtr = Marshal.PtrToStructure<IntPtr>(_vfTable + (_ptrSize * 26));
            var getPersistedDefaultAudioEndpoint = Marshal.GetDelegateForFunctionPointer<GetPersistedDefaultAudioEndpointDelegate>(getPersistedDefaultAudioEndpointPtr);

            var deviceIdPtr = new HSTRING();
            try
            {
                var result = getPersistedDefaultAudioEndpoint(_factory, processId, flow, role, ref deviceIdPtr);
                if (result != HRESULT.S_OK)
                {
                    if (result != HRESULT.PROCESS_NO_AUDIO)
                        throw new InvalidComObjectException($"Can't get the persisted audio endpoint: {result}");

                    return null;
                }

                return deviceIdPtr.ToString();
            }
            finally
            {
                deviceIdPtr.Dispose();
            }
        }

        public void SetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, string deviceId)
        {
            using var deviceIdHString = HSTRING.FromString(deviceId);
            var setPersistedDefaultAudioEndpointPtr = Marshal.PtrToStructure<IntPtr>(_vfTable + (_ptrSize * 25));
            var setPersistedDefaultAudioEndpoint = Marshal.GetDelegateForFunctionPointer<SetPersistedDefaultAudioEndpointDelegate>(setPersistedDefaultAudioEndpointPtr);
            var result = setPersistedDefaultAudioEndpoint(_factory, processId, flow, role, deviceIdHString);
            if (result != HRESULT.S_OK && result != HRESULT.PROCESS_NO_AUDIO)
                throw new InvalidComObjectException($"Can't set the persistent audio endpoint: {result}");
        }

        public void ClearAllPersistedApplicationDefaultEndpoints()
        {
            var clearAllPersistedDefaultAudioEndpointsPtr = Marshal.PtrToStructure<IntPtr>(_vfTable + (_ptrSize * 27));
            var clearAllPersistedDefaultAudioEndpoints = Marshal.GetDelegateForFunctionPointer<ClearAllPersistedApplicationDefaultEndpointsDelegate>(clearAllPersistedDefaultAudioEndpointsPtr);
            var result = clearAllPersistedDefaultAudioEndpoints(_factory);
            if (result != HRESULT.S_OK)
                throw new InvalidComObjectException($"Reset audio endpoints: {result}");
        }

        public void Dispose()
        {
            Marshal.Release(_factory);
            Marshal.Release(_vfTable);
        }
    }
}
