using System;
using System.Collections.Generic;
using System.Threading;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.UI.Menu.Component;
using SoundSwitch.UI.Menu.Form;

namespace SoundSwitch.Framework.QuickMenu
{
    public class QuickMenuManager
    {
        private static QuickMenuManager _instance;

        public static QuickMenuManager Instance
        {
            get { return _instance ??= new QuickMenuManager(); }
        }

        private QuickMenu<DeviceFullInfo> _quickMenu;
        private SynchronizationContext _syncContext;

        public void Setup()
        {
            // Grab the synchronization context of the UI thread!
            _syncContext = SynchronizationContext.Current;
            if (_syncContext is not System.Windows.Forms.WindowsFormsSynchronizationContext)
                throw new InvalidOperationException("BannerManager must be called in the context of the UI thread.");
        }


        /// <summary>
        /// Display the quick menu to the user
        /// </summary>
        /// <param name="payloads"></param>
        /// <param name="selectionChanged"></param>
        public void DisplayMenu(IEnumerable<IconMenuItem<DeviceFullInfo>.DataContainer> payloads, Action<QuickMenu<DeviceFullInfo>.MenuClickedEvent> selectionChanged)
        {
            _syncContext.Post(state =>
            {
                if (_quickMenu == null)
                {
                    _quickMenu = new QuickMenu<DeviceFullInfo>();
                    _quickMenu.SelectionChanged += (sender, @event) => selectionChanged(@event); 
                    _quickMenu.Disposed += (_, _) => _quickMenu = null;
                }

                _quickMenu.SetData(payloads);
            }, null);
        }
    }
}