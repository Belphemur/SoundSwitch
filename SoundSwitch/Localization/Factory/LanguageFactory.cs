using System.Globalization;
using System.Linq;
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
			new KoreanLang()
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

            return AllImplementations.Values.Where(value => value.CultureInfo.Equals(uiLang)).Select(value => value.TypeEnum).FirstOrDefault();
        }
    }
}