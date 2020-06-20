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

namespace SoundSwitch.Framework.Factory
{
    public interface IEnumImplList<TEnum, TEnumImpl> : IList<TEnumImpl> where TEnum : Enum, IConvertible where TEnumImpl : IEnumImpl<TEnum>
    {
        /// <summary>
        /// Convert the list into a ReadOnlyDictionary using the TypeEnum as Key
        /// </summary>
        /// <returns></returns>
        IReadOnlyDictionary<TEnum, TEnumImpl> ToReadOnlyDictionary();
    }
}