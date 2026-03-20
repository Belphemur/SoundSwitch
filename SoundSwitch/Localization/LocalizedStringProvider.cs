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

using System.Globalization;
using System.Reflection;
using System.Resources;

namespace SoundSwitch.Localization;

/// <summary>
/// Provides strongly-typed resource access with an explicit fallback to the English resource set
/// when a localized resource key is missing from the selected culture.
/// </summary>
internal sealed class LocalizedStringProvider(string baseName, Assembly assembly) : ResourceManager(baseName, assembly)
{
    private static readonly CultureInfo EnglishCulture = CultureInfo.GetCultureInfo("en");
    private const char LeftToRightEmbedding = '\u202A';
    private const char PopDirectionalFormatting = '\u202C';

    /// <inheritdoc />
    public override string GetString(string name)
    {
        return GetString(name, null);
    }

    /// <inheritdoc />
    public override string GetString(string name, CultureInfo culture)
    {
        var value = base.GetString(name, culture);
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var effectiveCulture = culture ?? CultureInfo.CurrentUICulture;
        if (effectiveCulture.TwoLetterISOLanguageName.Equals(EnglishCulture.TwoLetterISOLanguageName, System.StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        var fallbackValue = base.GetString(name, EnglishCulture);
        if (fallbackValue is null || !effectiveCulture.TextInfo.IsRightToLeft)
        {
            return fallbackValue;
        }

        return $"{LeftToRightEmbedding}{fallbackValue}{PopDirectionalFormatting}";
    }
}