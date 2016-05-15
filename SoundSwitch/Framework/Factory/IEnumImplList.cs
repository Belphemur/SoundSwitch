using System;
using System.Collections.Generic;

namespace SoundSwitch.Framework.Factory
{
    public interface IEnumImplList<TEnum, TEnumImpl> : IList<TEnumImpl> where TEnum : struct, IConvertible where TEnumImpl : IEnumImpl<TEnum>
    {
        /// <summary>
        ///     Convert the list into a ReadOnlyDictionary using the TypeEnum as Key
        /// </summary>
        /// <returns></returns>
        IReadOnlyDictionary<TEnum, TEnumImpl> ToReadOnlyDictionary();
    }
}