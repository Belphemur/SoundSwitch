using System;

namespace SoundSwitch.Framework.Profile.Trigger
{
    public static class TriggerEnumExtensions
    {
        /// <summary>
        /// Switch on the given enum
        /// </summary>
        /// <param name="enum"></param>
        /// <param name="hotkey"></param>
        /// <param name="window"></param>
        /// <param name="process"></param>
        /// <param name="steam"></param>
        /// <param name="startup"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void Switch(this TriggerFactory.Enum @enum, Action hotkey, Action window, Action process, Action steam, Action startup)
        {
            switch (@enum)
            {
                case TriggerFactory.Enum.HotKey:
                    hotkey();
                    break;
                case TriggerFactory.Enum.Window:
                    window();
                    break;
                case TriggerFactory.Enum.Process:
                    process();
                    break;
                case TriggerFactory.Enum.Steam:
                    steam();
                    break;
                case TriggerFactory.Enum.Startup:
                    startup();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@enum), @enum, null);
            }
        }

        /// <summary>
        /// Match on the enum
        /// </summary>
        /// <param name="enum"></param>
        /// <param name="hotkey"></param>
        /// <param name="window"></param>
        /// <param name="process"></param>
        /// <param name="steam"></param>
        /// <param name="startup"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Match<T>(this TriggerFactory.Enum @enum, Func<T> hotkey, Func<T> window, Func<T> process, Func<T> steam, Func<T> startup)
        {
            return @enum switch
            {
                TriggerFactory.Enum.HotKey  => hotkey(),
                TriggerFactory.Enum.Window  => window(),
                TriggerFactory.Enum.Process => process(),
                TriggerFactory.Enum.Steam   => steam(),
                TriggerFactory.Enum.Startup => startup(),
                _                           => throw new ArgumentOutOfRangeException(nameof(@enum), @enum, null)
            };
        }
    }
}