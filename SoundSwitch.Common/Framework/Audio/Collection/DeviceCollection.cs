using System;
using System.Collections;
using System.Collections.Generic;
using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Common.Framework.Audio.Collection;

public class DeviceCollection<T> : ICollection<T> where T : DeviceInfo
{
    private readonly Dictionary<string, T> _byId = new();
    private readonly Dictionary<string, T> _byName = new();

    public DeviceCollection(IEnumerable<T> deviceInfos)
    {
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

    public void Add(T item)
    {
        if (item == null)
        {
            throw new ArgumentNullException();
        }

        _byId.TryAdd(item.Id, item);
        _byName.TryAdd(item.NameClean, item);
    }

    public void Clear()
    {
        _byId.Clear();
        _byName.Clear();
    }

    public bool Contains(T item)
    {
        if (item == null)
        {
            throw new ArgumentNullException();
        }

        return _byId.ContainsKey(item.Id) || _byName.ContainsKey(item.NameClean);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _byId.Values.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        if (item == null)
        {
            throw new ArgumentNullException();
        }

        var removeId = _byId.Remove(item.Id);
        var removeName = _byName.Remove(item.NameClean);

        return removeId || removeName;
    }

    public int Count => _byId.Count;
    public bool IsReadOnly => false;
}