using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AudioEndPointControllerWrapper;
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
            var fieldInfo = typeof (AppConfigs).GetRuntimeProperty("Configuration");
            fieldInfo.SetValue(null, configurationMoq.Object);
        }

        [Test]
        public void TestAddingPlaybackDeviceToSelected()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};

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
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};

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
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};

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

        [Test]
        public void TestUnionSelectedDeviceWithActiveDevice()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};

            var audioMoqI = new Mock<IAudioDevice> {Name = "first audio dev"};
            audioMoqI.SetupGet(a => a.FriendlyName).Returns("Speakers (Test device)");
            var audioMoqII = new Mock<IAudioDevice> {Name = "secound audio dev"};
            audioMoqII.SetupGet(a => a.FriendlyName).Returns("Headphones (Test device)");

            var listerMoq = new Mock<IAudioDeviceLister> {Name = "Lister"};
            listerMoq.Setup(lister => lister.GetPlaybackDevices())
                .Returns(new List<IAudioDevice> {audioMoqI.Object, audioMoqII.Object});

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