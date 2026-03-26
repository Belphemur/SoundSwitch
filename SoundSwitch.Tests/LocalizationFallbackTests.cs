using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Localization;

namespace SoundSwitch.Tests;

[TestFixture]
public class LocalizationFallbackTests
{
    private static readonly CultureInfo EnglishCulture = CultureInfo.GetCultureInfo("en");
    private static readonly CultureInfo FrenchCulture = CultureInfo.GetCultureInfo("fr");
    private static readonly CultureInfo ArabicCulture = CultureInfo.GetCultureInfo("ar");
    private const char LeftToRightEmbedding = '\u202A';
    private const char PopDirectionalFormatting = '\u202C';

    [TestCase(typeof(AboutStrings))]
    [TestCase(typeof(SettingsStrings))]
    [TestCase(typeof(TrayIconStrings))]
    [TestCase(typeof(UpdateDownloadStrings))]
    public void ResourceManager_UsesEnglishFallbackProvider(Type resourceType)
    {
        GetResourceManager(resourceType).Should().BeOfType<LocalizedStringProvider>();
    }

    [Test]
    public void MissingLocalizedSettingsStrings_FallBackToEnglish()
    {
        var resourceManager = GetResourceManager(typeof(SettingsStrings));
        const string missingFrenchKey = "switchProfile";

        var frenchSet = resourceManager.GetResourceSet(FrenchCulture, true, false);
        frenchSet.Should().NotBeNull();
        frenchSet!.GetObject(missingFrenchKey).Should().BeNull();

        var englishValue = resourceManager.GetString(missingFrenchKey, EnglishCulture);
        var localizedValue = resourceManager.GetString(missingFrenchKey, FrenchCulture);

        englishValue.Should().NotBeNull();
        localizedValue.Should().Be(englishValue);
    }

    [Test]
    public void MissingLocalizedSettingsStrings_UseVisibleLtrFallbackInRtlCulture()
    {
        var resourceManager = GetResourceManager(typeof(SettingsStrings));
        const string emptyArabicKey = "openSettings";

        var arabicSet = resourceManager.GetResourceSet(ArabicCulture, true, false);
        arabicSet.Should().NotBeNull();
        arabicSet!.GetObject(emptyArabicKey).Should().Be(string.Empty);

        var englishValue = resourceManager.GetString(emptyArabicKey, EnglishCulture);
        var localizedValue = resourceManager.GetString(emptyArabicKey, ArabicCulture);

        englishValue.Should().NotBeNull();
        localizedValue.Should().Be($"{LeftToRightEmbedding}{englishValue}{PopDirectionalFormatting}");
    }

    private static ResourceManager GetResourceManager(Type resourceType)
    {
        var property = resourceType.GetProperty("ResourceManager", BindingFlags.NonPublic | BindingFlags.Static);
        property.Should().NotBeNull();

        var resourceManager = property!.GetValue(null) as ResourceManager;
        resourceManager.Should().NotBeNull();

        return resourceManager!;
    }
}
