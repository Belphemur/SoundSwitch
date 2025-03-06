using System.Globalization;
using System.Linq;
using Serilog;
using SoundSwitch.Framework.Factory;
using SoundSwitch.Localization.Factory.Lang;

namespace SoundSwitch.Localization.Factory
{
    public class LanguageFactory : AbstractFactory<Language, ILang>
    {
        public LanguageFactory() : base(new EnumImplList<Language, ILang>
        {
            new EnglishLang(),
            new FrenchLang(),
            new GermanLang(),
            new SpanishLang(),
            new NorwegianLang(),
            new PortugueseBRLang(),
            new ItalianLang(),
            new PolishLang(),
            new RussianLang(),
            new ChineseLang(),
            new KoreanLang(),
            new DutchLang(),
            new CroatianLang(),
            new ChineseTraditionalLang(),
            new SlovenianLang(),
            new JapaneseLang(),
            new HebrewLang(),
            new Czech(),
            new Turkish(),
            new Arabic(),
            new Thai(),
            new Serbian(),
            new Danish(),
            new Ukrainian(),
            new Bulgarian(),
            new Swedish(),
            new Tamil()
        })
        {
        }

        /// <summary>
        /// Get the language of Windows
        /// </summary>
        /// <returns></returns>
        public Language GetWindowsLanguage()
        {
            var uiLang = CultureInfo.CurrentUICulture;
            try
            {
                return AllImplementations.Values.Where(value => value.CultureInfo.Equals(uiLang)).Select(value => value.TypeEnum).FirstOrDefault();
            }
            catch (CultureNotFoundException e)
            {
                Log.Error(e, "Couldn't find the language @{lang}", uiLang);
                return Language.English;
            }
        }
    }
}
