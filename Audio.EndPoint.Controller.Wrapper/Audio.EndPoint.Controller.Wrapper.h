// Audio.EndPoint.Controller.Wrapper.h

#pragma once
#include "AudioDeviceWrapper.h"

using namespace System;
using namespace System::Collections::Generic;

namespace AudioEndPointControllerWrapper {

	public ref class AudioController
	{
	public:
		static List<AudioDeviceWrapper^>^ getAvailableAudioDevices();
		static List<AudioDeviceWrapper^>^ getAllAudioDevices();
	private:
		static List<AudioDeviceWrapper^>^ convertNativeList(std::list<AudioDevice*>* audioDeviceList);
	};
}
