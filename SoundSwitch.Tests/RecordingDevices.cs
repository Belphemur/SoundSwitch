/********************************************************************
* Copyright (C) 2015 Antoine Aflalo
* 
* This program is free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* as published by the Free Software Foundation; either version 2
* of the License, or (at your option) any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
********************************************************************/

using System;
using System.Collections.Generic;
using AudioEndPointControllerWrapper;
using Moq;
using NUnit.Framework;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Model;

namespace SoundSwitch.Tests
{
    [TestFixture]
    public class ModelRecordingDeviceTests
    {
        [Test]
        public void TestAddingRecordingDeviceToSelected()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};
            var audioMoqI = new Mock<IAudioDevice> {Name = "first audio dev"};
            audioMoqI.SetupGet(a => a.FriendlyName).Returns("Speakers (Test device)");
            audioMoqI.SetupGet(a => a.Type).Returns(AudioDeviceType.Recording);

            //Setup
            configurationMoq.Setup(c => c.SelectedRecordingDeviceList).Returns(new HashSet<string>());
            TestHelpers.SetConfigurationMoq(configurationMoq);

            //Action
            var eventCalled = false;
            AppModel.Instance.SelectedDeviceChanged += (sender, changed) => eventCalled = true;
            Assert.True(AppModel.Instance.SelectDevice(audioMoqI.Object));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedRecordingDeviceList);
            configurationMoq.Verify(c => c.Save());
            audioMoqI.VerifyGet(a => a.FriendlyName);
            audioMoqI.VerifyGet(a => a.Type);
            Assert.That(configurationMoq.Object.SelectedRecordingDeviceList.Count == 1);
            Assert.That(configurationMoq.Object.SelectedRecordingDeviceList.Contains("Speakers (Test device)"));
            Assert.That(eventCalled, "SelectedDeviceChanged not called");
        }

        [Test]
        public void TestAddingAlreadyPresentRecordingDeviceToSelected()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};
            var audioMoqI = new Mock<IAudioDevice> {Name = "first audio dev"};
            audioMoqI.SetupGet(a => a.FriendlyName).Returns("Speakers (Test device)");
            audioMoqI.SetupGet(a => a.Type).Returns(AudioDeviceType.Recording);

            //Setup
            configurationMoq.Setup(c => c.SelectedRecordingDeviceList)
                .Returns(new HashSet<string> {"Speakers (Test device)"});
            TestHelpers.SetConfigurationMoq(configurationMoq);

            //Action
            var eventCalled = false;
            AppModel.Instance.SelectedDeviceChanged += (sender, changed) => eventCalled = true;
            Assert.False(AppModel.Instance.SelectDevice(audioMoqI.Object));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedRecordingDeviceList);
            audioMoqI.VerifyGet(a => a.FriendlyName);
            audioMoqI.VerifyGet(a => a.Type);
            Assert.That(configurationMoq.Object.SelectedRecordingDeviceList.Count == 1);
            Assert.That(!eventCalled, "SelectedDeviceChanged called");
        }

        [Test]
        public void TestRemovePresentRecordingDevice()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};
            var audioMoqI = new Mock<IAudioDevice> {Name = "first audio dev"};
            audioMoqI.SetupGet(a => a.FriendlyName).Returns("Speakers (Test device)");
            audioMoqI.SetupGet(a => a.Type).Returns(AudioDeviceType.Recording);

            //Setup
            configurationMoq.Setup(c => c.SelectedRecordingDeviceList)
                .Returns(new HashSet<string> {"Speakers (Test device)"});
            TestHelpers.SetConfigurationMoq(configurationMoq);

            //Action
            var eventCalled = false;
            AppModel.Instance.SelectedDeviceChanged += (sender, changed) => eventCalled = true;
            Assert.True(AppModel.Instance.UnselectDevice(audioMoqI.Object));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedRecordingDeviceList);
            configurationMoq.Verify(c => c.Save());
            audioMoqI.VerifyGet(a => a.FriendlyName);
            audioMoqI.VerifyGet(a => a.Type);
            Assert.That(configurationMoq.Object.SelectedRecordingDeviceList.Count == 0);
            Assert.That(eventCalled, "SelectedDeviceChanged not called");
        }

        [Test]
        public void TestRemoveNotPresentRecordingDevice()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};
            var audioMoqI = new Mock<IAudioDevice> {Name = "first audio dev"};
            audioMoqI.SetupGet(a => a.FriendlyName).Returns("Speakers (Test device)");
            audioMoqI.SetupGet(a => a.Type).Returns(AudioDeviceType.Recording);

            //Setup
            configurationMoq.Setup(c => c.SelectedRecordingDeviceList).Returns(new HashSet<string>());
            TestHelpers.SetConfigurationMoq(configurationMoq);

            //Action
            var eventCalled = false;
            AppModel.Instance.SelectedDeviceChanged += (sender, changed) => eventCalled = true;
            Assert.False(AppModel.Instance.UnselectDevice(audioMoqI.Object));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedRecordingDeviceList);
            audioMoqI.VerifyGet(a => a.FriendlyName);
            audioMoqI.VerifyGet(a => a.Type);
            Assert.That(!eventCalled, "SelectedDeviceChanged called");
        }

        [Test]
        public void TestUnionSelectedRecordingDeviceWithActiveDevice()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};

            var audioMoqI = new Mock<IAudioDevice> {Name = "first audio dev"};
            audioMoqI.SetupGet(a => a.FriendlyName).Returns("Speakers (Test device)");
            var audioMoqII = new Mock<IAudioDevice> {Name = "secound audio dev"};
            audioMoqII.SetupGet(a => a.FriendlyName).Returns("Headphones (Test device)");

            var listerMoq = new Mock<IAudioDeviceLister> {Name = "Lister"};
            listerMoq.Setup(lister => lister.GetRecordingDevices())
                .Returns(new List<IAudioDevice> {audioMoqI.Object, audioMoqII.Object});

            //Setup
            configurationMoq.Setup(c => c.SelectedRecordingDeviceList)
                .Returns(new HashSet<string> {"Speakers (Test device)"});
            TestHelpers.SetConfigurationMoq(configurationMoq);
            AppModel.Instance.ActiveAudioDeviceLister = listerMoq.Object;

            //Action
            Assert.That(AppModel.Instance.AvailableRecordingDevices.Count == 1);
            Assert.That(AppModel.Instance.AvailableRecordingDevices.Contains(audioMoqI.Object));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedRecordingDeviceList);
            audioMoqI.VerifyGet(a => a.FriendlyName);
            audioMoqII.VerifyGet(a => a.FriendlyName);
            listerMoq.Verify(l => l.GetRecordingDevices());
        }

        [Test]
        public void TestCycleConsoleAudioDevice()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};
            configurationMoq.SetupGet(conf => conf.LastRecordingActive).Returns("");

            var audioMoqI = new Mock<IAudioDevice> {Name = "first audio dev"};
            audioMoqI.SetupGet(a => a.FriendlyName).Returns("Speakers (Test device)");
            audioMoqI.Setup(a => a.IsDefault(It.Is<Role>(role => role == Role.Console))).Returns(true).Verifiable();
            var audioMoqII = new Mock<IAudioDevice> {Name = "secound audio dev"};
            audioMoqII.SetupGet(a => a.FriendlyName).Returns("Headphones (Test device)");
            audioMoqII.SetupGet(a => a.Type).Returns(AudioDeviceType.Recording);

            var listerMoq = new Mock<IAudioDeviceLister> {Name = "Lister"};
            listerMoq.Setup(lister => lister.GetRecordingDevices())
                .Returns(new List<IAudioDevice> {audioMoqI.Object, audioMoqII.Object});

            //Setup
            configurationMoq.Setup(c => c.SelectedRecordingDeviceList)
                .Returns(new HashSet<string> {"Speakers (Test device)", "Headphones (Test device)"});
            TestHelpers.SetConfigurationMoq(configurationMoq);
            AppModel.Instance.ActiveAudioDeviceLister = listerMoq.Object;
            IAudioDevice audioDevice = null;
            AppModel.Instance.DefaultDeviceChanged += (sender, @event) => audioDevice = @event.AudioDevice;

            //Action
            Assert.That(AppModel.Instance.CycleActiveDevice(AudioDeviceType.Recording));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedRecordingDeviceList);
            audioMoqI.VerifyGet(a => a.FriendlyName);
            audioMoqII.VerifyGet(a => a.FriendlyName);
            listerMoq.Verify(l => l.GetRecordingDevices());
            configurationMoq.VerifyGet(config => config.LastRecordingActive);
            configurationMoq.VerifySet(config => config.LastRecordingActive = "Headphones (Test device)");
            audioMoqII.Verify(a => a.SetAsDefault(It.Is<Role>(role => role == Role.Console)));
            Assert.That(audioMoqII.Object.Equals(audioDevice));
        }

        [Test]
        public void TestCycleCommunicationsAudioDevice()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};
            configurationMoq.SetupGet(conf => conf.LastRecordingActive).Returns("");
            configurationMoq.SetupGet(conf => conf.ChangeCommunications).Returns(true);

            var audioMoqI = new Mock<IAudioDevice> {Name = "first audio dev"};
            audioMoqI.SetupGet(a => a.FriendlyName).Returns("Speakers (Test device)");
            audioMoqI.Setup(a => a.IsDefault(It.Is<Role>(role => role == Role.Console))).Returns(true).Verifiable();
            audioMoqI.Setup(a => a.IsDefault(It.Is<Role>(role => role == Role.Communications)))
                .Returns(true)
                .Verifiable();
            audioMoqI.SetupGet(a => a.Type).Returns(AudioDeviceType.Recording);
            var audioMoqII = new Mock<IAudioDevice> {Name = "secound audio dev"};
            audioMoqII.SetupGet(a => a.FriendlyName).Returns("Headphones (Test device)");
            audioMoqII.SetupGet(a => a.Type).Returns(AudioDeviceType.Recording);
          

            var listerMoq = new Mock<IAudioDeviceLister> {Name = "Lister"};
            listerMoq.Setup(lister => lister.GetRecordingDevices())
                .Returns(new List<IAudioDevice> {audioMoqI.Object, audioMoqII.Object});

            //Setup
            configurationMoq.Setup(c => c.SelectedRecordingDeviceList)
                .Returns(new HashSet<string> {"Speakers (Test device)", "Headphones (Test device)"});
            TestHelpers.SetConfigurationMoq(configurationMoq);
            AppModel.Instance.ActiveAudioDeviceLister = listerMoq.Object;
            IAudioDevice audioDevice = null;
            AppModel.Instance.DefaultDeviceChanged += (sender, @event) => audioDevice = @event.AudioDevice;

            //Action
            Assert.That(AppModel.Instance.CycleActiveDevice(AudioDeviceType.Recording));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedRecordingDeviceList);
            audioMoqI.VerifyGet(a => a.FriendlyName);
            audioMoqII.VerifyGet(a => a.FriendlyName);
            listerMoq.Verify(l => l.GetRecordingDevices());
            configurationMoq.VerifyGet(config => config.LastRecordingActive);
            configurationMoq.VerifySet(config => config.LastRecordingActive = "Headphones (Test device)");
            audioMoqII.Verify(a => a.SetAsDefault(It.Is<Role>(role => role == Role.Console)));
            audioMoqII.Verify(a => a.SetAsDefault(It.Is<Role>(role => role == Role.Communications)));
            Assert.That(audioMoqII.Object.Equals(audioDevice));
        }

        [Test]
        public void TestCycleAudioDeviceIsACycleThatReturnToFirstWhenReachEnd()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};
            configurationMoq.SetupGet(conf => conf.LastRecordingActive).Returns("");

            var audioMoqI = new Mock<IAudioDevice> {Name = "first audio dev"};
            audioMoqI.SetupGet(a => a.FriendlyName).Returns("Speakers (Test device)");
            var audioMoqII = new Mock<IAudioDevice> {Name = "secound audio dev"};
            audioMoqII.SetupGet(a => a.FriendlyName).Returns("Headphones (Test device)");
            audioMoqII.Setup(a => a.IsDefault(It.Is<Role>(role => role == Role.Console))).Returns(true).Verifiable();
            audioMoqI.SetupGet(a => a.Type).Returns(AudioDeviceType.Recording);
            audioMoqII.SetupGet(a => a.Type).Returns(AudioDeviceType.Recording);

            var listerMoq = new Mock<IAudioDeviceLister> {Name = "Lister"};
            listerMoq.Setup(lister => lister.GetRecordingDevices())
                .Returns(new List<IAudioDevice> {audioMoqI.Object, audioMoqII.Object});

            //Setup
            configurationMoq.Setup(c => c.SelectedRecordingDeviceList)
                .Returns(new HashSet<string> {"Speakers (Test device)", "Headphones (Test device)"});
            TestHelpers.SetConfigurationMoq(configurationMoq);
            AppModel.Instance.ActiveAudioDeviceLister = listerMoq.Object;
            IAudioDevice audioDevice = null;
            AppModel.Instance.DefaultDeviceChanged += (sender, @event) => audioDevice = @event.AudioDevice;

            //Action
            Assert.That(AppModel.Instance.CycleActiveDevice(AudioDeviceType.Recording));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedRecordingDeviceList);
            audioMoqI.VerifyGet(a => a.FriendlyName);
            audioMoqII.VerifyGet(a => a.FriendlyName);
            listerMoq.Verify(l => l.GetRecordingDevices());
            configurationMoq.VerifyGet(config => config.LastRecordingActive);
            configurationMoq.VerifySet(config => config.LastRecordingActive = "Speakers (Test device)");
            audioMoqI.Verify(a => a.SetAsDefault(It.Is<Role>(role => role == Role.Console)));
            Assert.That(audioMoqI.Object.Equals(audioDevice));
        }

        [Test]
        public void TestSetActiveDeviceNull()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};
            configurationMoq.SetupGet(conf => conf.LastRecordingActive).Returns("");


            //Setup
            TestHelpers.SetConfigurationMoq(configurationMoq);
            bool deviceChangeEventCalled = false;
            bool errorTriggeredEventCalled = false;
            AppModel.Instance.DefaultDeviceChanged += (sender, @event) => deviceChangeEventCalled = true;
            AppModel.Instance.ErrorTriggered +=
                (sender, @event) => errorTriggeredEventCalled = @event.Exception is NullReferenceException;

            //Action
            AppModel.Instance.SetActiveDevice(null);

            //Asserts
            Assert.That(!deviceChangeEventCalled);
            Assert.That(errorTriggeredEventCalled);
        }

        [Test]
        public void TestSetAudioDeviceSetAudioDeviceAsDefault()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};
            configurationMoq.SetupGet(conf => conf.LastRecordingActive).Returns("");

            var audioMoqI = new Mock<IAudioDevice> {Name = "first audio dev"};
            audioMoqI.SetupGet(a => a.FriendlyName).Returns("Speakers (Test device)");
            audioMoqI.SetupGet(a => a.Type).Returns(AudioDeviceType.Recording);

            //Setup
            TestHelpers.SetConfigurationMoq(configurationMoq);
            IAudioDevice audioDevice = null;
            AppModel.Instance.DefaultDeviceChanged += (sender, @event) => audioDevice = @event.AudioDevice;

            //Action
            AppModel.Instance.SetActiveDevice(audioMoqI.Object);

            //Asserts
            configurationMoq.VerifySet(config => config.LastRecordingActive = "Speakers (Test device)");
            audioMoqI.VerifyGet(a => a.Type);
            audioMoqI.Verify(a => a.SetAsDefault(It.Is<Role>(role => role == Role.Console)));
            Assert.That(audioMoqI.Object.Equals(audioDevice));
        }
    }
}