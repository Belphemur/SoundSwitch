using System.Runtime.InteropServices;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Audio.Manager.Interop.Interface;
using SoundSwitch.Audio.Manager.Interop.Interface.Policy;

namespace SoundSwitch.Audio.Manager.Interop.Client
{
    internal class PolicyClient
    {
        private readonly IPolicyConfig _configVII;
        private readonly IPolicyConfigVista _configVista;
        private readonly IPolicyConfigX _configX;
        private readonly _PolicyConfigClient _policyConfig;

        public PolicyClient()
        {
            _policyConfig = new _PolicyConfigClient();

            _configX = _policyConfig as IPolicyConfigX;
            _configVII = _policyConfig as IPolicyConfig;
            _configVista = _policyConfig as IPolicyConfigVista;
        }

        ~PolicyClient()
        {
            if (_policyConfig != null && Marshal.IsComObject(_policyConfig))
                Marshal.FinalReleaseComObject(_policyConfig);
        }

        public void SetDefaultEndpoint(string devId, ERole eRole)
        {
            if (_configX != null)
            {
                Marshal.ThrowExceptionForHR(_configX.SetDefaultEndpoint(devId, eRole));
            }
            else if (_configVII != null)
            {
                Marshal.ThrowExceptionForHR(_configVII.SetDefaultEndpoint(devId, eRole));
            }
            else if (_configVista != null)
            {
                Marshal.ThrowExceptionForHR(_configVista.SetDefaultEndpoint(devId, eRole));
            }
        }

        [ComImport, Guid(ComGuid.POLICY_CONFIG_CLIENT_IID)]
        private class _PolicyConfigClient
        {
        }
    }
}