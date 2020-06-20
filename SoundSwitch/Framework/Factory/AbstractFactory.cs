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
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace SoundSwitch.Framework.Factory
{
    /// <summary>
    /// Used to build factory based on Enums
    /// </summary>
    /// <typeparam name="TEnum">The Enum defining the type</typeparam>
    /// <typeparam name="TImplementation">The implementation of the enum</typeparam>
    public abstract class AbstractFactory<TEnum, TImplementation> where TImplementation : IEnumImpl<TEnum>
        where TEnum : System.Enum, IConvertible
    {
        protected AbstractFactory(IEnumImplList<TEnum, TImplementation> enumImplList)
        {
            AllImplementations = enumImplList.ToReadOnlyDictionary();
        }

        public IReadOnlyDictionary<TEnum, TImplementation> AllImplementations { get; }

        /// <summary>
        /// Get the implementation for the given Enum
        /// </summary>
        /// <param name="eEnum"></param>
        /// <returns></returns>
        public TImplementation Get(TEnum eEnum)
        {
            TImplementation value;
            if (!DataSource().TryGetValue(eEnum, out value))
            {
                throw new InvalidEnumArgumentException();
            }
            return value;
        }

        protected virtual IReadOnlyDictionary<TEnum, TImplementation> DataSource()
        {
            return AllImplementations;
        }

        /// <summary>
        /// Configure the list control DataSource, ValueMember and DisplayMember
        /// </summary>
        /// <param name="list"></param>
        public void ConfigureListControl(ListControl list)
        {
            list.DataSource =
                DataSource().Values.Select(
                    implementation => new DisplayEnumObject<TEnum>(implementation))
                    .ToArray();
            list.ValueMember = nameof(DisplayEnumObject<TEnum>.Enum);
            list.DisplayMember = nameof(DisplayEnumObject<TEnum>.Display);
        }
    }
}