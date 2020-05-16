using System;
using System.Collections.Generic;
using System.ComponentModel;
using Resolve.HotKeys;

namespace SoundSwitch.Common.WinApi.Keyboard
{
    public class HotKeyManager : IDisposable
    {
        private readonly Dictionary<HotKey, Resolve.HotKeys.HotKey> _hotKeys = new Dictionary<HotKey, Resolve.HotKeys.HotKey>();

        /// <summary>
        /// Register a hotkey and its action when pressed
        /// </summary>
        /// <param name="hotKey"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool Register(HotKey hotKey, Action action)
        {
            if (_hotKeys.ContainsKey(hotKey))
                return false;

            var hotKeyHandle = new Resolve.HotKeys.HotKey(hotKey.Keys, (ModifierKey) hotKey.Modifier);
            try
            {
                hotKeyHandle.Register();
                hotKeyHandle.Pressed += (sender, args) => action();
            }
            catch (Win32Exception)
            {
                return false;
            }

            _hotKeys.Add(hotKey, hotKeyHandle);
            return true;
        }

        /// <summary>
        /// UnRegister given hotkey if present
        /// </summary>
        /// <param name="hotKey"></param>
        /// <returns></returns>
        public bool UnRegister(HotKey hotKey)
        {
            if (!_hotKeys.TryGetValue(hotKey, out var hotKeyHandle))
                return false;
            try
            {
                hotKeyHandle.Dispose();
            }
            catch (Win32Exception)
            {
                return false;
            }

            _hotKeys.Remove(hotKey);
            return true;
        }

        public void Dispose()
        {
            foreach (var hotKeysValue in _hotKeys.Values)
            {
                try
                {
                    hotKeysValue.Dispose();
                }
                catch (Exception)
                {
                    //Ignored
                }
            }
        }
    }
}