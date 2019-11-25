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

namespace SoundSwitch.Localization
{
    /// <summary>
    /// Provides methods to parse languages.
    /// </summary>
    public static class LanguageParser
    {
        /// <summary>
        /// Parses the given Language to it's CultureInfo representation.
        /// </summary>
        /// <param name="language">The Language to parse.</param>
        /// <returns>The Language as CultureInfo. If the given Language isn't defined, English will be returned.</returns>
        public static CultureInfo ParseLanguage(Language language)
        {
            switch (language)
            {
                case Language.Français:
                    return CultureInfo.GetCultureInfo("fr");
                case Language.Deutsche:
                    return CultureInfo.GetCultureInfo("de");
                case Language.Español:
                    return CultureInfo.GetCultureInfo("es");
                case Language.Norsk:
                    return CultureInfo.GetCultureInfo("nb");
                case Language.Português:
                    return CultureInfo.GetCultureInfo("pt-BR");
                case Language.Italiano:
                    return CultureInfo.GetCultureInfo("it-IT");
                case Language.Chinese:
                    return CultureInfo.GetCultureInfo("zh-CHS");
                default:
                    return CultureInfo.GetCultureInfo("en");

            }
        }

        /// <summary>
        /// Parses the given CultureInfo to it's Language representation.
        /// </summary>
        /// <param name="cultureInfo">The CultureInfo to parse.</param>
        /// <returns>The CultureInfo as Language. If the given CultureInfo isn't defined, English will be returned.</returns>
        public static Language ParseLanguage(CultureInfo cultureInfo)
        {
            switch (cultureInfo.TwoLetterISOLanguageName)
            {
                case "fr":
                    return Language.Français;
                case "de":
                    return Language.Deutsche;
                case "es":
                    return Language.Español;
                case "nb":
                    return Language.Norsk;
                case "pt":
                    return Language.Português;
                case "it":
                    return Language.Italiano;
                case "zh":
                    return Language.Chinese;
                default:
                    return Language.English;
            }
        }
    }
}
