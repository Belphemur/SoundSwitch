// This is the main DLL file.

#include "stdafx.h"
#include "Audio.EndPoint.Controller.h"

#include "Audio.EndPoint.Controller.Wrapper.h"

using namespace System;
using namespace AudioEndPointControllerWrapper;


List<AudioDeviceWrapper^>^ AudioEndPointControllerWrapper::AudioController::getAvailableAudioDevices()
{
	
	std::list<AudioDevice*>* audioDeviceList = getAudioDevices(DEVICE_STATE_ACTIVE);
	auto result = AudioController::convertNativeList(audioDeviceList);
	delete audioDeviceList;
	return result;
}

List<AudioDeviceWrapper^>^ AudioEndPointControllerWrapper::AudioController::getAllAudioDevices()
{
	std::list<AudioDevice*>* audioDeviceList = getAudioDevices(DEVICE_STATEMASK_ALL);
	auto result = AudioController::convertNativeList(audioDeviceList);
	delete audioDeviceList;
	return result;
}

List<AudioDeviceWrapper^>^ AudioEndPointControllerWrapper::AudioController::convertNativeList(std::list<AudioDevice*>* audioDeviceList)
{
	List<AudioDeviceWrapper^>^ list = gcnew List<AudioDeviceWrapper^>();
	for (std::list<AudioDevice*>::iterator i = audioDeviceList->begin(); i != audioDeviceList->end(); i++)
	{
		AudioDeviceWrapper^ wrapper = gcnew AudioDeviceWrapper(*i);
		list->Add(wrapper);
	}
	return list;
}
