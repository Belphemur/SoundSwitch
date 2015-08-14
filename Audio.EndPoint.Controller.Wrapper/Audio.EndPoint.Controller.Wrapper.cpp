// This is the main DLL file.

#include "stdafx.h"
#include "Audio.EndPoint.Controller.h"

#include "Audio.EndPoint.Controller.Wrapper.h"

using namespace System;
using namespace AudioEndPointControllerWrapper;


List<AudioDeviceWrapper^>^ AudioEndPointControllerWrapper::AudioController::getAvailableAudioDevices()
{
	
	std::list<AudioDevice> audioDeviceList = getAudioDevices(DEVICE_STATE_ACTIVE);
	return AudioEndPointControllerWrapper::AudioController::convertNativeList(audioDeviceList);
}

List<AudioDeviceWrapper^>^ AudioEndPointControllerWrapper::AudioController::getAllAudioDevices()
{
	std::list<AudioDevice> audioDeviceList = getAudioDevices(DEVICE_STATEMASK_ALL);
	return AudioEndPointControllerWrapper::AudioController::convertNativeList(audioDeviceList);
}

List<AudioDeviceWrapper^>^ AudioEndPointControllerWrapper::AudioController::convertNativeList(std::list<AudioDevice> audioDeviceList)
{
	List<AudioDeviceWrapper^>^ list = gcnew List<AudioDeviceWrapper^>();
	for (std::list<AudioDevice>::iterator i = audioDeviceList.begin(); i != audioDeviceList.end(); i++)
	{
		AudioDevice nativeValue = *i;
		AudioDeviceWrapper^ wrapper = gcnew AudioDeviceWrapper(&nativeValue);
		list->Add(wrapper);
	}
	return list;
}
