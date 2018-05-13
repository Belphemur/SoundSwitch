using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;
using Serilog;

namespace SoundSwitch.Framework.Audio.Device
{
    public class DisposableMMDeviceCollection : IEnumerable<MMDevice>, IDisposable
    {
        public int Count => _collection.Count;
        private readonly MMDeviceCollection _collection;

        public DisposableMMDeviceCollection(MMDeviceCollection collection)
        {
            this._collection = collection;
        }

        public IEnumerator<MMDevice> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

       

        public void Dispose()
        {
            foreach (var mmddevice in _collection.ToList())
            {
                try
                {
                    mmddevice.Dispose();
                }
                catch (ObjectDisposedException e)
                {
                    Log.Logger.Error(e, "Can't dispose mmdevice");
                }
            }
        }
    }
}