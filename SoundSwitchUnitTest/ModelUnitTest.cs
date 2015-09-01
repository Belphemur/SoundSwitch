using System.Collections.Generic;
using System.Reflection;
using Moq;
using NUnit.Framework;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Model;

namespace SoundSwitchUnitTest
{
    [TestFixture, Category("AppModel")]
    public class ModelUnitTest
    {
        private static void SetConfigurationMoq(IMock<ISoundSwitchConfiguration> configurationMoq)
        {
            var fieldInfo = typeof (AppConfigs).GetRuntimeProperty("Configuration");
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
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};

            //Setup
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceList).Returns(new HashSet<string> {"test"});
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
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceList).Returns(new HashSet<string> {"test"});
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


    }
}