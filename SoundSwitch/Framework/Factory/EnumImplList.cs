/********************************************************************
* Copyright (C) 2015-2017 Antoine Aflalo
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* as published by the Free Software Foundation; either version 2
* of the License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
********************************************************************/

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
        where TEnum : Enum, IConvertible
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