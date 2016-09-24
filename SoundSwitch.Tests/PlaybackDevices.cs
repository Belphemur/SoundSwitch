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
using SoundSwitch.Framework.DeviceCyclerManager;
using SoundSwitch.Model;
using Assert = NUnit.Framework.Assert;

namespace SoundSwitch.Tests
{
    [TestFixture]
    public class ModelPlaybackDeviceTests
    {
        [Test]
        public void TestAddingPlaybackDeviceToSelected()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};
            var audioMoqI = new Mock<IAudioDevice> {Name = "first audio dev"};
            audioMoqI.SetupGet(a => a.Id).Returns("Speakers (Test device)");
            audioMoqI.SetupGet(a => a.Type).Returns(AudioDeviceType.Playback);

            //Setup
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceListId).Returns(new HashSet<string>());
            TestHelpers.SetConfigurationMoq(configurationMoq);

            //Action
            var eventCalled = false;
            AppModel.Instance.SelectedDeviceChanged += (sender, changed) => eventCalled = true;
            Assert.True(AppModel.Instance.SelectDevice(audioMoqI.Object));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedPlaybackDeviceListId);
            configurationMoq.Verify(c => c.Save());
            audioMoqI.VerifyGet(a => a.Id);
            audioMoqI.VerifyGet(a => a.Type);
            Assert.That(configurationMoq.Object.SelectedPlaybackDeviceListId.Count == 1);
            Assert.That(configurationMoq.Object.SelectedPlaybackDeviceListId.Contains("Speakers (Test device)"));
            Assert.That(eventCalled, "SelectedDeviceChanged not called");
        }

        [Test]
        public void TestAddingAlreadyPresentPlaybackDeviceToSelected()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};
            var audioMoqI = new Mock<IAudioDevice> {Name = "first audio dev"};
            audioMoqI.SetupGet(a => a.Id).Returns("Speakers (Test device)");
            audioMoqI.SetupGet(a => a.Type).Returns(AudioDeviceType.Playback);

            //Setup
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceListId)
                .Returns(new HashSet<string> {"Speakers (Test device)"});
            TestHelpers.SetConfigurationMoq(configurationMoq);

            //Action
            var eventCalled = false;
            AppModel.Instance.SelectedDeviceChanged += (sender, changed) => eventCalled = true;
            Assert.False(AppModel.Instance.SelectDevice(audioMoqI.Object));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedPlaybackDeviceListId);
            audioMoqI.VerifyGet(a => a.Id);
            audioMoqI.VerifyGet(a => a.Type);
            Assert.That(configurationMoq.Object.SelectedPlaybackDeviceListId.Count == 1);
            Assert.That(!eventCalled, "SelectedDeviceChanged called");
        }

        [Test]
        public void TestRemovePresentPlaybackDevice()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};
            var audioMoqI = new Mock<IAudioDevice> {Name = "first audio dev"};
            audioMoqI.SetupGet(a => a.Id).Returns("Speakers (Test device)");
            audioMoqI.SetupGet(a => a.Type).Returns(AudioDeviceType.Playback);

            //Setup
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceListId)
                .Returns(new HashSet<string> {"Speakers (Test device)"});
            TestHelpers.SetConfigurationMoq(configurationMoq);

            //Action
            var eventCalled = false;
            AppModel.Instance.SelectedDeviceChanged += (sender, changed) => eventCalled = true;
            Assert.True(AppModel.Instance.UnselectDevice(audioMoqI.Object));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedPlaybackDeviceListId);
            configurationMoq.Verify(c => c.Save());
            audioMoqI.VerifyGet(a => a.Id);
            audioMoqI.VerifyGet(a => a.Type);
            Assert.That(configurationMoq.Object.SelectedPlaybackDeviceListId.Count == 0);
            Assert.That(eventCalled, "SelectedDeviceChanged not called");
        }

        [Test]
        public void TestRemoveNotPresentPlaybackDevice()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};
            var audioMoqI = new Mock<IAudioDevice> {Name = "first audio dev"};
            audioMoqI.SetupGet(a => a.Id).Returns("Speakers (Test device)");
            audioMoqI.SetupGet(a => a.Type).Returns(AudioDeviceType.Playback);

            //Setup
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceListId).Returns(new HashSet<string>());
            TestHelpers.SetConfigurationMoq(configurationMoq);

            //Action
            var eventCalled = false;
            AppModel.Instance.SelectedDeviceChanged += (sender, changed) => eventCalled = true;
            Assert.False(AppModel.Instance.UnselectDevice(audioMoqI.Object));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedPlaybackDeviceListId);
            audioMoqI.VerifyGet(a => a.Id);
            audioMoqI.VerifyGet(a => a.Type);
            Assert.That(!eventCalled, "SelectedDeviceChanged called");
        }

        [Test]
        public void TestUnionSelectedPlaybackDeviceWithActiveDevice()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};

            var audioMoqI = new Mock<IAudioDevice> {Name = "first audio dev"};
            audioMoqI.SetupGet(a => a.Id).Returns("Speakers (Test device)");
            var audioMoqII = new Mock<IAudioDevice> {Name = "secound audio dev"};
            audioMoqII.SetupGet(a => a.Id).Returns("Headphones (Test device)");

            var listerMoq = new Mock<IAudioDeviceLister> {Name = "Lister"};
            listerMoq.Setup(lister => lister.GetPlaybackDevices())
                .Returns(new List<IAudioDevice> {audioMoqI.Object, audioMoqII.Object});

            //Setup
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceListId)
                .Returns(new HashSet<string> {"Speakers (Test device)"});
            TestHelpers.SetConfigurationMoq(configurationMoq);
            AppModel.Instance.ActiveAudioDeviceLister = listerMoq.Object;

            //Action
            Assert.That(AppModel.Instance.AvailablePlaybackDevices.Count == 1);
            Assert.That(AppModel.Instance.AvailablePlaybackDevices.Contains(audioMoqI.Object));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedPlaybackDeviceListId);
            audioMoqI.VerifyGet(a => a.Id);
            audioMoqII.VerifyGet(a => a.Id);
            listerMoq.Verify(l => l.GetPlaybackDevices());
        }

        [Test]
        public void TestCycleConsoleAudioDevice()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};

            var audioMoqI = new Mock<IAudioDevice> {Name = "first audio dev"};
            audioMoqI.SetupGet(a => a.Id).Returns("Speakers (Test device)");
            audioMoqI.Setup(a => a.IsDefault(It.Is<Role>(role => role == Role.Console))).Returns(true).Verifiable();
            var audioMoqII = new Mock<IAudioDevice> {Name = "secound audio dev"};
            audioMoqII.SetupGet(a => a.Id).Returns("Headphones (Test device)");
            audioMoqII.SetupGet(a => a.Id).Returns("Headphones (Test device)");

            var listerMoq = new Mock<IAudioDeviceLister> {Name = "Lister"};
            listerMoq.Setup(lister => lister.GetPlaybackDevices())
                .Returns(new List<IAudioDevice> {audioMoqI.Object, audioMoqII.Object});

            //Setup
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceListId)
                .Returns(new HashSet<string> {"Speakers (Test device)", "Headphones (Test device)"});
            configurationMoq.Setup(configuration => configuration.CyclerType).Returns(DeviceCyclerTypeEnum.Available);
            configurationMoq.Setup(configuration => configuration.ChangeCommunications).Returns(false);
            TestHelpers.SetConfigurationMoq(configurationMoq);
            AppModel.Instance.ActiveAudioDeviceLister = listerMoq.Object;

            //Action
            Assert.That(AppModel.Instance.CycleActiveDevice(AudioDeviceType.Playback));

            //Asserts
            configurationMoq.VerifyGet(c => c.SelectedPlaybackDeviceListId);
            audioMoqI.VerifyGet(a => a.Id);
            audioMoqII.VerifyGet(a => a.Id);
            listerMoq.Verify(l => l.GetPlaybackDevices());
            audioMoqI.Verify(a => a.SetAsDefault(It.Is<Role>(role => role == Role.Console)));
            audioMoqI.Verify(a => a.SetAsDefault(It.Is<Role>(role => role == Role.Multimedia)));
        }

        [Test]
        public void TestCycleCommunicationsAudioDevice()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};
            configurationMoq.SetupGet(conf => conf.ChangeCommunications).Returns(true);

            var audioMoqI = new Mock<IAudioDevice> {Name = "first audio dev"};
            audioMoqI.SetupGet(a => a.Id).Returns("Speakers (Test device)");
            audioMoqI.Setup(a => a.IsDefault(It.Is<Role>(role => role == Role.Console))).Returns(true).Verifiable();
            audioMoqI.Setup(a => a.IsDefault(It.Is<Role>(role => role == Role.Communications)))
                .Returns(true)
                .Verifiable();
            var audioMoqII = new Mock<IAudioDevice> {Name = "secound audio dev"};
            audioMoqII.SetupGet(a => a.Id).Returns("Headphones (Test device)");
            audioMoqII.SetupGet(a => a.Id).Returns("Headphones (Test device)");

            var listerMoq = new Mock<IAudioDeviceLister> {Name = "Lister"};
            listerMoq.Setup(lister => lister.GetPlaybackDevices())
                .Returns(new List<IAudioDevice> {audioMoqI.Object, audioMoqII.Object});

            //Setup
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceListId)
                .Returns(new HashSet<string> {"Speakers (Test device)", "Headphones (Test device)"});
            configurationMoq.Setup(configuration => configuration.CyclerType).Returns(DeviceCyclerTypeEnum.Available);
            TestHelpers.SetConfigurationMoq(configurationMoq);
            AppModel.Instance.ActiveAudioDeviceLister = listerMoq.Object;

            //Action
            Assert.That(AppModel.Instance.CycleActiveDevice(AudioDeviceType.Playback));

            //Asserts
            configurationMoq.VerifyGet(configuration => configuration.SelectedPlaybackDeviceListId);
            configurationMoq.VerifyGet(configuration => configuration.CyclerType);
            audioMoqI.VerifyGet(a => a.Id);
            audioMoqII.VerifyGet(a => a.Id);
            listerMoq.Verify(l => l.GetPlaybackDevices());
            audioMoqII.Verify(a => a.SetAsDefault(It.Is<Role>(role => role == Role.All)));
        }

        [Test]
        public void TestCycleAudioDeviceIsACycleThatReturnToFirstWhenReachEnd()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};

            var audioMoqI = new Mock<IAudioDevice> {Name = "first audio dev"};
            audioMoqI.SetupGet(a => a.Id).Returns("Speakers (Test device)");
            audioMoqI.SetupGet(a => a.Id).Returns("Speakers (Test device)");
            var audioMoqII = new Mock<IAudioDevice> {Name = "secound audio dev"};
            audioMoqII.SetupGet(a => a.Id).Returns("Headphones (Test device)");
            audioMoqII.Setup(a => a.IsDefault(It.Is<Role>(role => role == Role.Console))).Returns(true).Verifiable();

            var listerMoq = new Mock<IAudioDeviceLister> {Name = "Lister"};
            listerMoq.Setup(lister => lister.GetPlaybackDevices())
                .Returns(new List<IAudioDevice> {audioMoqI.Object, audioMoqII.Object});

            //Setup
            configurationMoq.Setup(c => c.SelectedPlaybackDeviceListId)
                .Returns(new HashSet<string> {"Speakers (Test device)", "Headphones (Test device)"});
            TestHelpers.SetConfigurationMoq(configurationMoq);
            AppModel.Instance.ActiveAudioDeviceLister = listerMoq.Object;

            //Action
            Assert.That(AppModel.Instance.CycleActiveDevice(AudioDeviceType.Playback));

            //Asserts
            listerMoq.Verify(l => l.GetPlaybackDevices());
            audioMoqI.Verify(a => a.SetAsDefault(It.Is<Role>(role => role == Role.Console)));
            audioMoqI.Verify(a => a.SetAsDefault(It.Is<Role>(role => role == Role.Multimedia)));
        }

        [Test]
        public void TestSetActiveDeviceNull()
        {
            var configurationMoq = new Mock<ISoundSwitchConfiguration> {Name = "Configuration mock"};


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

            var audioMoqI = new Mock<IAudioDevice> {Name = "first audio dev"};
            audioMoqI.SetupGet(a => a.Id).Returns("Speakers (Test device)");
            audioMoqI.SetupGet(a => a.Id).Returns("Speakers (Test device)");
            audioMoqI.SetupGet(a => a.Type).Returns(AudioDeviceType.Playback);

            //Setup
            TestHelpers.SetConfigurationMoq(configurationMoq);


            //Action
            AppModel.Instance.SetActiveDevice(audioMoqI.Object);

            //Asserts
            audioMoqI.VerifyGet(a => a.Type);
            audioMoqI.Verify(a => a.SetAsDefault(It.Is<Role>(role => role == Role.Console)));
            audioMoqI.Verify(a => a.SetAsDefault(It.Is<Role>(role => role == Role.Multimedia)));
        }

        [Test]
        public void TestUnderlyingLibraryPlaybackDevices()
        {
            AudioController.GetAllPlaybackDevices();
        }
    }
}