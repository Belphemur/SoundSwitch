﻿using System.Globalization;
using SoundSwitch.Framework.Factory;

namespace SoundSwitch.Localization.Factory.Lang
{
    public class EnglishLang : ILang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public CultureInfo CultureInfo => CultureInfo.GetCultureInfo("en");

        public Language TypeEnum => Language.English;
        public string Label => "English";
    }
    
    public class FrenchLang : ILang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public CultureInfo CultureInfo => CultureInfo.GetCultureInfo("fr");

        public Language TypeEnum => Language.French;
        public string Label => "Français";
    }
    
    public class GermanLang : ILang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public CultureInfo CultureInfo => CultureInfo.GetCultureInfo("de");

        public Language TypeEnum => Language.German;
        public string Label => "Deutsche";
    }
    public class SpanishLang : ILang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public CultureInfo CultureInfo => CultureInfo.GetCultureInfo("es");

        public Language TypeEnum => Language.Spanish;
        public string Label => "Español";
    }
    public class NorwegianLang : ILang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public CultureInfo CultureInfo => CultureInfo.GetCultureInfo("nb");

        public Language TypeEnum => Language.Norwegian;
        public string Label => "Norsk";
    }
    public class PortugueseBRLang : ILang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public CultureInfo CultureInfo => CultureInfo.GetCultureInfo("pt-BR");

        public Language TypeEnum => Language.Portuguese;
        public string Label => "Português (BR)";
    }
    public class ItalianLang : ILang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public CultureInfo CultureInfo => CultureInfo.GetCultureInfo("it-IT");

        public Language TypeEnum => Language.Italian;
        public string Label => "Italiano";
    }
    public class ChineseLang : ILang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public CultureInfo CultureInfo => CultureInfo.GetCultureInfo("zh-CHS");

        public Language TypeEnum => Language.Chinese;
        public string Label => "汉语";
    }
    public class PolishLang : ILang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public CultureInfo CultureInfo => CultureInfo.GetCultureInfo("pl-PL");

        public Language TypeEnum => Language.Polish;
        public string Label => "Polski";
    }
    public class RussianLang : ILang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public CultureInfo CultureInfo => CultureInfo.GetCultureInfo("ru-RU");

        public Language TypeEnum => Language.Russian;
        public string Label => "Pусский";
    }
}