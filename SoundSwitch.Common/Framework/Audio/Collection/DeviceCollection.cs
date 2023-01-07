using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
                continue;
            }

            _byId[item.Id] = item;
            _byName[GetNameKey(item)] = item;
        }
    }

    /// <summary>
    /// Generate a key that merge together the type of device and their name.
    /// </summary>
    /// <remarks>Recording and Playback device can share the same name, this is done to avoid conflict between the two</remarks>
    /// <param name="item"></param>
    /// <returns></returns>
    private string GetNameKey(T item) => $"{item.Type}-{item.NameClean}";

    public IEnumerator<T> GetEnumerator()
    {
        return _byId.Values.OrderBy(info => info.DiscoveredAt).GetEnumerator();
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
        _byName.TryAdd(GetNameKey(item), item);
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

        return _byId.ContainsKey(item.Id) || _byName.ContainsKey(GetNameKey(item));
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
        var removeName = _byName.Remove(GetNameKey(item), out var removedByName);

        //If we found it by name, remove it also with it's id
        //this avoid the case that a device is removed because of name matching
        //but it's still used since we iterate the _byId
        if (removeName)
        {
            _byId.Remove(removedByName.Id);
        }

        return removeId || removeName;
    }

    public int Count => _byId.Count;
    public bool IsReadOnly => false;
}