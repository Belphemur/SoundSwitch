using SoundSwitch.Audio.Policies.Interop;

namespace SoundSwitch.Audio.Policies
{
    public class AudioSwitcher
    {
        private IPolicyConfigVista _configVista;
        private static AudioSwitcher _instance;

        private AudioSwitcher()
        {
        }

        public static AudioSwitcher Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                return _instance = new AudioSwitcher();
            }
        }

        public IPolicyConfigVista PolicyConfig
        {
            get
            {
                if (_configVista != null)
                {
                    return _configVista;
                }

                return _configVista = (IPolicyConfigVista) new CPolicyConfigVista();
            }
        }

        public bool switchTo(string deviceId, ERole role)
        {
            var result = PolicyConfig.SetDefaultEndpoint(deviceId, role);
            if (result == HRESULT.S_OK)
            {
                return true;
            }

            return false;
        }
    }
}