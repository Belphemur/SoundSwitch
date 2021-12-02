using System;
using System.Collections.Generic;
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

                IEnumerable<IntPtr> GuidPtrs(IntPtr source)
                {
                    var total = GUID_SIZE * (uint)Marshal.ReadInt32(count);
                    for (var i = 0; i < total; i += GUID_SIZE)
                    {
                        yield return source + i;
                    }
                }

                var audioPolicyConfigGuidPtr = GuidPtrs(iids).Last();
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

        public string GetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role)
        {
            var getPersistedDefaultAudioEndpointPtr = Marshal.PtrToStructure<IntPtr>(_vfTable + (_ptrSize * 26));
            var getPersistedDefaultAudioEndpoint = Marshal.GetDelegateForFunctionPointer<GetPersistedDefaultAudioEndpointDelegate>(getPersistedDefaultAudioEndpointPtr);
            HSTRING deviceIdPtr = new();
            var result = getPersistedDefaultAudioEndpoint(_factory, processId, flow, role, ref deviceIdPtr);
            if (result != HRESULT.S_OK)
            {
                if (result != HRESULT.PROCESS_NO_AUDIO)
                    throw new InvalidComObjectException($"Can't get the persisted audio endpoint: {result}");

                return null;
            }

            return deviceIdPtr.ToString();
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
