using NUnit.Framework;
using SoundSwitch.Localization.Factory;
using SoundSwitch.Localization.Factory.Lang;

namespace SoundSwitch.Tests
{
    [TestFixture]
    public class LanguageTests
    {
        /// <summary>
        /// Check that we can load the culture info of all the <see cref="ILang"/>
        /// </summary>
        [Test]
        public void LanguageLoading()
        {
            var langFactory = new LanguageFactory();
            Assert.DoesNotThrow(() =>
            {
                foreach (var lang in langFactory.AllImplementations.Values)
                {
                    var _ = lang.Label;
                }
            });
        }
    }
}