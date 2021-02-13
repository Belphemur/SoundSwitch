using System.Globalization;

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
        public string Label => "Deutsch";
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
        public CultureInfo CultureInfo => CultureInfo.GetCultureInfo("zh-Hans");

        public Language TypeEnum => Language.Chinese;
        public string Label => "中文 (简体)";
    }


    public class ChineseTraditionalLang : ILang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public CultureInfo CultureInfo => CultureInfo.GetCultureInfo("zh-Hant");

        public Language TypeEnum => Language.ChineseTrad;
        public string Label => "中文 (正體)";
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

    public class KoreanLang : ILang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public CultureInfo CultureInfo => CultureInfo.GetCultureInfo("ko");

        public Language TypeEnum => Language.Korean;
        public string Label => "한국어";
    }

    public class DutchLang : ILang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public CultureInfo CultureInfo => CultureInfo.GetCultureInfo("nl");

        public Language TypeEnum => Language.Dutch;
        public string Label => "Nederlands";
    }


    public class CroatianLang : ILang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public CultureInfo CultureInfo => CultureInfo.GetCultureInfo("hr");

        public Language TypeEnum => Language.Croatian;
        public string Label => "Hrvatski";
    }

    public class SlovenianLang : ILang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public CultureInfo CultureInfo => CultureInfo.GetCultureInfo("sl");

        public Language TypeEnum => Language.Slovenian;
        public string Label => "Slovenščina";
    }
}