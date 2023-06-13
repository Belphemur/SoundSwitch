using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;
using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Common.Framework.Audio.Collection
{
    public class DeviceReadOnlyCollection<T> : IReadOnlyCollection<T> where T : DeviceInfo
    {
        private readonly DataFlow _dataFlow;
        private readonly Dictionary<string, T> _byId = new();
        private readonly Dictionary<string, T> _byName = new();

        public DeviceReadOnlyCollection(IEnumerable<T> deviceInfos, DataFlow dataFlow)
        {
            _dataFlow = dataFlow;
            foreach (var item in deviceInfos)
            {
                if (item == null)
                {
                    return;
                }

                _byId[item.Id] = item;
                _byName[item.NameClean] = item;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _byId.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Intersect with another list of <see cref="DeviceInfo"/>
        /// </summary>
        /// <param name="second"></param>
        /// <returns></returns>
        public IEnumerable<T> IntersectWith(IEnumerable<DeviceInfo> second)
        {
            return second
                   .Where(info => info.Type == _dataFlow)
                   .Select(info =>
                   {
                       if (_byId.TryGetValue(info.Id, out var found))
                       {
                           return found;
                       }

                       if (_byName.TryGetValue(info.NameClean, out found))
                       {
                           return found;
                       }

                       return null;
                   })
                   .Where(info => info != null)
                   .Distinct();
        }

        public int Count => _byId.Count;
    }
}