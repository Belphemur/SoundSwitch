using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Moq;
using NUnit.Framework;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Model;

namespace SoundSwitch.Tests
{
    [TestFixture]
    public class ModelUnitTest
    {
        private static void SetConfigurationMoq(IMock<ISoundSwitchConfiguration> configurationMoq)
        {
            var fieldInfo = typeof(AppConfigs).GetRuntimeProperty("Configuration");
            fieldInfo.SetValue(null, configurationMoq.Object);
        }

        [Test]
        public void TestAddingPlaybackDeviceToSelected()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> { Name = "Configuration mock" };

            //Setup
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceList).Returns(new HashSet<string>());
            SetConfigurationMoq(configurationMoq);

            //Action
            var eventCalled = false;
            AppModel.Instance.SelectedPlaybackDeviceChanged += (sender, changed) => eventCalled = true;
            Assert.True(AppModel.Instance.AddPlaybackDevice("test"));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedPlaybackDeviceList);
            configurationMoq.Verify(c => c.Save());
            Assert.That(configurationMoq.Object.SelectedPlaybackDeviceList.Count == 1);
            Assert.That(configurationMoq.Object.SelectedPlaybackDeviceList.Contains("test"));
            Assert.That(eventCalled, "SelectedPlaybackDeviceChanged not called");
        }

        [Test]
        public void TestAddingAlreadyPresentPlaybackDeviceToSelected()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> { Name = "Configuration mock" };

            //Setup
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceList).Returns(new HashSet<string> { "test" });
            SetConfigurationMoq(configurationMoq);

            //Action
            var eventCalled = false;
            AppModel.Instance.SelectedPlaybackDeviceChanged += (sender, changed) => eventCalled = true;
            Assert.False(AppModel.Instance.AddPlaybackDevice("test"));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedPlaybackDeviceList);
            Assert.That(configurationMoq.Object.SelectedPlaybackDeviceList.Count == 1);
            Assert.That(!eventCalled, "SelectedPlaybackDeviceChanged called");
        }

        [Test]
        public void TestRemovePresentPlaybackDevice()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> { Name = "Configuration mock" };

            //Setup
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceList).Returns(new HashSet<string> { "test" });
            SetConfigurationMoq(configurationMoq);

            //Action
            var eventCalled = false;
            AppModel.Instance.SelectedPlaybackDeviceChanged += (sender, changed) => eventCalled = true;
            Assert.True(AppModel.Instance.RemovePlaybackDevice("test"));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedPlaybackDeviceList);
            configurationMoq.Verify(c => c.Save());
            Assert.That(configurationMoq.Object.SelectedPlaybackDeviceList.Count == 0);
            Assert.That(!configurationMoq.Object.SelectedPlaybackDeviceList.Contains("test"));
            Assert.That(eventCalled, "SelectedPlaybackDeviceChanged not called");
        }

        [Test]
        public void TestRemoveNotPresentPlaybackDevice()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> { Name = "Configuration mock" };

            //Setup
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceList).Returns(new HashSet<string>());
            SetConfigurationMoq(configurationMoq);

            //Action
            var eventCalled = false;
            AppModel.Instance.SelectedPlaybackDeviceChanged += (sender, changed) => eventCalled = true;
            Assert.False(AppModel.Instance.RemovePlaybackDevice("test"));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedPlaybackDeviceList);
            Assert.That(!eventCalled, "SelectedPlaybackDeviceChanged called");
        }

        [Test, Category("AudioDevice")]
        public void TestActiveAudioDeviceWithSelectedList()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> { Name = "Configuration mock" };

            //Setup
            var allActiveDevice =
                AudioEndPointControllerWrapper.AudioController.GetActivePlaybackDevices()
                    .Select(a => a.FriendlyName)
                    .ToList();
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceList).Returns(new HashSet<string>(allActiveDevice));
            SetConfigurationMoq(configurationMoq);

            //Action
            Assert.That(
                AppModel.Instance.AvailablePlaybackDevices.Select(a => a.FriendlyName)
                    .Intersect(allActiveDevice)
                    .Count() == allActiveDevice.Count);

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedPlaybackDeviceList);
        }

        [Test, Category("AudioDevice")]
        public void TestActiveAudioDeviceWithSelectedListWithUnexistingOne()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> { Name = "Configuration mock" };

            //Setup
            var allActiveDevice =
                AudioEndPointControllerWrapper.AudioController.GetActivePlaybackDevices()
                    .Select(a => a.FriendlyName)
                    .ToList();
            allActiveDevice.Add("ThisCan'tBePresent");
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceList).Returns(new HashSet<string>(allActiveDevice));
            SetConfigurationMoq(configurationMoq);

            //Action
            Assert.That(
                AppModel.Instance.AvailablePlaybackDevices.Select(a => a.FriendlyName)
                    .Intersect(allActiveDevice)
                    .Count() == allActiveDevice.Count - 1);

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedPlaybackDeviceList);
        }


    }
}