using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SoundSwitch.Audio.Manager.Interop.Interface;

namespace SoundSwitch.Audio.Manager.Interop
{
    public class PolicyClient
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

            GC.Collect();
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

        [ComImport, Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9")]
        private class _PolicyConfigClient
        {
        }
    }
}