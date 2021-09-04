using System;
using System.Collections.Generic;
using System.Threading;
using SoundSwitch.UI.Menu.Component;
using SoundSwitch.UI.Menu.Form;

namespace SoundSwitch.UI.Menu
{
    public class QuickMenuManager<T>
    {
        public static QuickMenuManager<T> Instance { get; } = new();

        private QuickMenu<T>? _quickMenu;
        private SynchronizationContext? _syncContext;

        public void Setup()
        {
            // Grab the synchronization context of the UI thread!
            _syncContext = SynchronizationContext.Current;
            if (_syncContext is not System.Windows.Forms.WindowsFormsSynchronizationContext)
                throw new InvalidOperationException($"{nameof(QuickMenuManager<T>)} must be called in the context of the UI thread.");
        }


        /// <summary>
        /// Display the quick menu to the user
        /// </summary>
        /// <param name="payloads"></param>
        /// <param name="selectionChanged"></param>
        public void DisplayMenu(IEnumerable<IconMenuItem<T>.DataContainer> payloads, Action<QuickMenu<T>.MenuClickedEvent> selectionChanged)
        {
            if (_syncContext == null)
            {
                throw new InvalidOperationException($"{nameof(QuickMenuManager<T>)}.{nameof(Setup)}() needs to be called before displaying a menu.");
            }

            _syncContext.Post(_ =>
            {
                if (_quickMenu == null)
                {
                    _quickMenu = new QuickMenu<T>();
                    _quickMenu.Disposed += (_, _) => _quickMenu = null;
                }
                _quickMenu.ClearEventHandlers();
                _quickMenu.SelectionChanged += (sender, @event) => selectionChanged(@event);

                _quickMenu.SetData(payloads);
            }, null);
        }
    }
}