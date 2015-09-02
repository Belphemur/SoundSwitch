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
            var audioMoqI = new Mock<IAudioDevice> { Name = "first audio dev" };
            audioMoqI.SetupGet(a => a.FriendlyName).Returns("Speakers (Test device)");

            //Setup
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceList).Returns(new HashSet<string>());
            SetConfigurationMoq(configurationMoq);

            //Action
            var eventCalled = false;
            AppModel.Instance.SelectedPlaybackDeviceChanged += (sender, changed) => eventCalled = true;
            Assert.True(AppModel.Instance.SelectDevice(audioMoqI.Object));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedPlaybackDeviceList);
            configurationMoq.Verify(c => c.Save());
            audioMoqI.VerifyGet(a => a.FriendlyName);
            Assert.That(configurationMoq.Object.SelectedPlaybackDeviceList.Count == 1);
            Assert.That(configurationMoq.Object.SelectedPlaybackDeviceList.Contains("Speakers (Test device)"));
            Assert.That(eventCalled, "SelectedPlaybackDeviceChanged not called");
        }

        [Test]
        public void TestAddingAlreadyPresentPlaybackDeviceToSelected()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};
            var audioMoqI = new Mock<IAudioDevice> { Name = "first audio dev" };
            audioMoqI.SetupGet(a => a.FriendlyName).Returns("Speakers (Test device)");

            //Setup
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceList).Returns(new HashSet<string> { "Speakers (Test device)" });
            SetConfigurationMoq(configurationMoq);

            //Action
            var eventCalled = false;
            AppModel.Instance.SelectedPlaybackDeviceChanged += (sender, changed) => eventCalled = true;
            Assert.False(AppModel.Instance.SelectDevice(audioMoqI.Object));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedPlaybackDeviceList);
            audioMoqI.VerifyGet(a => a.FriendlyName);
            Assert.That(configurationMoq.Object.SelectedPlaybackDeviceList.Count == 1);
            Assert.That(!eventCalled, "SelectedPlaybackDeviceChanged called");
        }

        [Test]
        public void TestRemovePresentPlaybackDevice()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};
            var audioMoqI = new Mock<IAudioDevice> { Name = "first audio dev" };
            audioMoqI.SetupGet(a => a.FriendlyName).Returns("Speakers (Test device)");
            //Setup
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceList).Returns(new HashSet<string> { "Speakers (Test device)" });
            SetConfigurationMoq(configurationMoq);

            //Action
            var eventCalled = false;
            AppModel.Instance.SelectedPlaybackDeviceChanged += (sender, changed) => eventCalled = true;
            Assert.True(AppModel.Instance.UnselectDevice(audioMoqI.Object));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedPlaybackDeviceList);
            configurationMoq.Verify(c => c.Save());
            audioMoqI.VerifyGet(a => a.FriendlyName);
            Assert.That(configurationMoq.Object.SelectedPlaybackDeviceList.Count == 0);
            Assert.That(eventCalled, "SelectedPlaybackDeviceChanged not called");
        }

        [Test]
        public void TestRemoveNotPresentPlaybackDevice()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};
            var audioMoqI = new Mock<IAudioDevice> { Name = "first audio dev" };
            audioMoqI.SetupGet(a => a.FriendlyName).Returns("Speakers (Test device)");
            //Setup
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceList).Returns(new HashSet<string>());
            SetConfigurationMoq(configurationMoq);

            //Action
            var eventCalled = false;
            AppModel.Instance.SelectedPlaybackDeviceChanged += (sender, changed) => eventCalled = true;
            Assert.False(AppModel.Instance.UnselectDevice(audioMoqI.Object));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedPlaybackDeviceList);
            audioMoqI.VerifyGet(a => a.FriendlyName);
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
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceList).Returns(new HashSet<string> { "Speakers (Test device)" });
            SetConfigurationMoq(configurationMoq);
            AppModel.Instance.ActiveAudioDeviceLister = listerMoq.Object;

            //Action
            Assert.That(AppModel.Instance.AvailablePlaybackDevices.Count == 1);
            Assert.That(AppModel.Instance.AvailablePlaybackDevices.Contains(audioMoqI.Object));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedPlaybackDeviceList);
            audioMoqI.VerifyGet(a => a.FriendlyName);
            audioMoqII.VerifyGet(a => a.FriendlyName);
            listerMoq.Verify(l => l.GetPlaybackDevices());
        }

        [Test]
        public void TestCycleConsoleAudioDevice()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> { Name = "Configuration mock" };
            configurationMoq.SetupGet(conf => conf.LastActiveDevice).Returns("");

            var audioMoqI = new Mock<IAudioDevice> { Name = "first audio dev" };
            audioMoqI.SetupGet(a => a.FriendlyName).Returns("Speakers (Test device)");
            audioMoqI.Setup(a => a.IsDefault(It.Is<Role>(role => role == Role.Console))).Returns(true).Verifiable();
            var audioMoqII = new Mock<IAudioDevice> { Name = "secound audio dev" };
            audioMoqII.SetupGet(a => a.FriendlyName).Returns("Headphones (Test device)");

            var listerMoq = new Mock<IAudioDeviceLister> { Name = "Lister" };
            listerMoq.Setup(lister => lister.GetPlaybackDevices())
                .Returns(new List<IAudioDevice> { audioMoqI.Object, audioMoqII.Object });

            //Setup
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceList).Returns(new HashSet<string> { "Speakers (Test device)", "Headphones (Test device)" });
            SetConfigurationMoq(configurationMoq);
            AppModel.Instance.ActiveAudioDeviceLister = listerMoq.Object;
            IAudioDevice audioDevice = null;
            AppModel.Instance.DefaultPlaybackDeviceChanged += (sender, @event) => audioDevice = @event.AudioDevice;

            //Action
            Assert.That(AppModel.Instance.CycleActiveDevice());

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedPlaybackDeviceList);
            audioMoqI.VerifyGet(a => a.FriendlyName);
            audioMoqII.VerifyGet(a => a.FriendlyName);
            listerMoq.Verify(l => l.GetPlaybackDevices());
            configurationMoq.VerifyGet(config => config.LastActiveDevice);
            configurationMoq.VerifySet(config => config.LastActiveDevice = "Headphones (Test device)");
            audioMoqII.Verify(a => a.SetAsDefault(It.Is<Role>(role => role == Role.Console)));
            Assert.That(audioMoqII.Object.Equals(audioDevice));
        }

        [Test]
        public void TestCycleCommunicationsAudioDevice()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> { Name = "Configuration mock" };
            configurationMoq.SetupGet(conf => conf.LastActiveDevice).Returns("");
            configurationMoq.SetupGet(conf => conf.ChangeCommunications).Returns(true);

            var audioMoqI = new Mock<IAudioDevice> { Name = "first audio dev" };
            audioMoqI.SetupGet(a => a.FriendlyName).Returns("Speakers (Test device)");
            audioMoqI.Setup(a => a.IsDefault(It.Is<Role>(role => role == Role.Console))).Returns(true).Verifiable();
            audioMoqI.Setup(a => a.IsDefault(It.Is<Role>(role => role == Role.Communications))).Returns(true).Verifiable();
            var audioMoqII = new Mock<IAudioDevice> { Name = "secound audio dev" };
            audioMoqII.SetupGet(a => a.FriendlyName).Returns("Headphones (Test device)");

            var listerMoq = new Mock<IAudioDeviceLister> { Name = "Lister" };
            listerMoq.Setup(lister => lister.GetPlaybackDevices())
                .Returns(new List<IAudioDevice> { audioMoqI.Object, audioMoqII.Object });

            //Setup
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceList).Returns(new HashSet<string> { "Speakers (Test device)", "Headphones (Test device)" });
            SetConfigurationMoq(configurationMoq);
            AppModel.Instance.ActiveAudioDeviceLister = listerMoq.Object;
            IAudioDevice audioDevice = null;
            AppModel.Instance.DefaultPlaybackDeviceChanged += (sender, @event) => audioDevice = @event.AudioDevice;

            //Action
            Assert.That(AppModel.Instance.CycleActiveDevice());

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedPlaybackDeviceList);
            audioMoqI.VerifyGet(a => a.FriendlyName);
            audioMoqII.VerifyGet(a => a.FriendlyName);
            listerMoq.Verify(l => l.GetPlaybackDevices());
            configurationMoq.VerifyGet(config => config.LastActiveDevice);
            configurationMoq.VerifySet(config => config.LastActiveDevice = "Headphones (Test device)");
            audioMoqII.Verify(a => a.SetAsDefault(It.Is<Role>(role => role == Role.Console)));
            audioMoqII.Verify(a => a.SetAsDefault(It.Is<Role>(role => role == Role.Communications)));
            Assert.That(audioMoqII.Object.Equals(audioDevice));
        }

        [Test]
        public void TestCycleAudioDeviceIsACycleThatReturnToFirstWhenReachEnd()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> { Name = "Configuration mock" };
            configurationMoq.SetupGet(conf => conf.LastActiveDevice).Returns("");

            var audioMoqI = new Mock<IAudioDevice> { Name = "first audio dev" };
            audioMoqI.SetupGet(a => a.FriendlyName).Returns("Speakers (Test device)");
            var audioMoqII = new Mock<IAudioDevice> { Name = "secound audio dev" };
            audioMoqII.SetupGet(a => a.FriendlyName).Returns("Headphones (Test device)");
            audioMoqII.Setup(a => a.IsDefault(It.Is<Role>(role => role == Role.Console))).Returns(true).Verifiable();

            var listerMoq = new Mock<IAudioDeviceLister> { Name = "Lister" };
            listerMoq.Setup(lister => lister.GetPlaybackDevices())
                .Returns(new List<IAudioDevice> { audioMoqI.Object, audioMoqII.Object });

            //Setup
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceList).Returns(new HashSet<string> { "Speakers (Test device)", "Headphones (Test device)" });
            SetConfigurationMoq(configurationMoq);
            AppModel.Instance.ActiveAudioDeviceLister = listerMoq.Object;
            IAudioDevice audioDevice = null;
            AppModel.Instance.DefaultPlaybackDeviceChanged += (sender, @event) => audioDevice = @event.AudioDevice;

            //Action
            Assert.That(AppModel.Instance.CycleActiveDevice());

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedPlaybackDeviceList);
            audioMoqI.VerifyGet(a => a.FriendlyName);
            audioMoqII.VerifyGet(a => a.FriendlyName);
            listerMoq.Verify(l => l.GetPlaybackDevices());
            configurationMoq.VerifyGet(config => config.LastActiveDevice);
            configurationMoq.VerifySet(config => config.LastActiveDevice = "Speakers (Test device)");
            audioMoqI.Verify(a => a.SetAsDefault(It.Is<Role>(role => role == Role.Console)));
            Assert.That(audioMoqI.Object.Equals(audioDevice));
        }

        [Test]
        public void TestSetActiveDeviceNull()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};
            configurationMoq.SetupGet(conf => conf.LastActiveDevice).Returns("");


            //Setup
            SetConfigurationMoq(configurationMoq);
            bool deviceChangeEventCalled = false;
            bool errorTriggeredEventCalled = false;
            AppModel.Instance.DefaultPlaybackDeviceChanged += (sender, @event) => deviceChangeEventCalled = true;
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
            var configurationMoq = new Mock<ISoundSwitchConfiguration> { Name = "Configuration mock" };
            configurationMoq.SetupGet(conf => conf.LastActiveDevice).Returns("");

            var audioMoqI = new Mock<IAudioDevice> { Name = "first audio dev" };
            audioMoqI.SetupGet(a => a.FriendlyName).Returns("Speakers (Test device)");

            //Setup
            SetConfigurationMoq(configurationMoq);
            IAudioDevice audioDevice = null;
            AppModel.Instance.DefaultPlaybackDeviceChanged += (sender, @event) => audioDevice = @event.AudioDevice;

            //Action
            AppModel.Instance.SetActiveDevice(audioMoqI.Object);

            //Asserts
            configurationMoq.VerifySet(config => config.LastActiveDevice = "Speakers (Test device)");
            audioMoqI.Verify(a => a.SetAsDefault(It.Is<Role>(role => role == Role.Console)));
            Assert.That(audioMoqI.Object.Equals(audioDevice));
        }
    }
}