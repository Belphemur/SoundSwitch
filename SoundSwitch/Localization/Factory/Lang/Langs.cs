using System.Globalization;

namespace SoundSwitch.Localization.Factory.Lang
{
    public abstract class BaseLang : ILang
    {
        public abstract Language TypeEnum { get; }
        public string Label => CultureInfo.NativeName;
        public abstract CultureInfo CultureInfo { get; }
        public virtual bool IsRightToLeft { get; }
    }

    public class EnglishLang : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("en");

        public override Language TypeEnum => Language.English;
    }

    public class FrenchLang : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("fr");

        public override Language TypeEnum => Language.French;
    }

    public class GermanLang : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("de");

        public override Language TypeEnum => Language.German;
    }

    public class SpanishLang : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("es");

        public override Language TypeEnum => Language.Spanish;
    }

    public class NorwegianLang : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("nb");

        public override Language TypeEnum => Language.Norwegian;
    }

    public class PortugueseBRLang : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("pt-BR");

        public override Language TypeEnum => Language.Portuguese;
    }

    public class ItalianLang : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("it-IT");

        public override Language TypeEnum => Language.Italian;
    }

    public class ChineseLang : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("zh-Hans");

        public override Language TypeEnum => Language.Chinese;
    }


    public class ChineseTraditionalLang : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("zh-Hant");

        public override Language TypeEnum => Language.ChineseTrad;
    }

    public class PolishLang : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("pl-PL");

        public override Language TypeEnum => Language.Polish;
    }

    public class RussianLang : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("ru-RU");

        public override Language TypeEnum => Language.Russian;
    }

    public class KoreanLang : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("ko");

        public override Language TypeEnum => Language.Korean;
    }

    public class DutchLang : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("nl");

        public override Language TypeEnum => Language.Dutch;
    }


    public class CroatianLang : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("hr");

        public override Language TypeEnum => Language.Croatian;
    }

    public class SlovenianLang : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("sl");

        public override Language TypeEnum => Language.Slovenian;
    }

    public class JapaneseLang : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("ja-JP");

        public override Language TypeEnum => Language.Japanese;
    }

    public class HebrewLang : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("he");

        public override Language TypeEnum => Language.Hebrew;

        public override bool IsRightToLeft => true;
    }

    public class Czech : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("cs");

        public override Language TypeEnum => Language.Czech;
    }
    public class Turkish : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("tr");

        public override Language TypeEnum => Language.Turkish;
    }

    public class Arabic : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("ar");

        public override Language TypeEnum => Language.Arabic;
        public override bool IsRightToLeft => true;
    }
    
    public class Thai : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("th");

        public override Language TypeEnum => Language.Thai;
    }
    
        
    public class Serbian : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("sr");

        public override Language TypeEnum => Language.Serbian;
    }
    
    public class Danish : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("da");

        public override Language TypeEnum => Language.Danish;
    }
    
    
    public class Ukrainian : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("uk");

        public override Language TypeEnum => Language.Ukrainian;
    }
    
    public class Bulgarian : BaseLang
    {
        /// <summary>
        /// Culture info of this language
        /// </summary>
        public override CultureInfo CultureInfo => CultureInfo.GetCultureInfo("bg");

        public override Language TypeEnum => Language.Bulgarian;
    }
}
