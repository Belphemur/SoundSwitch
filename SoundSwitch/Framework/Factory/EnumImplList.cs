using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SoundSwitch.Framework.Factory
{
    /// <summary>
    ///     Used to represent the list of enum buildable by a factory
    /// </summary>
    /// <typeparam name="TEnum">Enumeration (Enum)</typeparam>
    /// <typeparam name="TEnumImpl">Implementation of the Enumeration</typeparam>
    public class EnumImplList<TEnum, TEnumImpl> : List<TEnumImpl>, IEnumImplList<TEnum, TEnumImpl> where TEnumImpl : IEnumImpl<TEnum>
        where TEnum : struct, IConvertible
    {
        /// <summary>
        ///     Convert the list into a ReadOnlyDictionary using the TypeEnum as Key
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<TEnum, TEnumImpl> ToReadOnlyDictionary()
        {
            return new ReadOnlyDictionary<TEnum, TEnumImpl>(this.ToDictionary(enumImpl => enumImpl.TypeEnum));
        }
    }
}