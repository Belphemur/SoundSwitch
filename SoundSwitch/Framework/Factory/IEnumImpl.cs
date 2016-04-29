using System;
using SoundSwitch.Framework.TooltipInfoManager.TootipInfo;

namespace SoundSwitch.Framework.Factory
{
    public interface IEnumImpl<out TEnum> where TEnum : struct, IConvertible
    {
        /// <summary>
        ///     Type of the Tooltip Info
        /// </summary>
        TEnum TypeEnum { get; }

        /// <summary>
        ///     Displaying label
        /// </summary>
        string Label { get; }
    }
}