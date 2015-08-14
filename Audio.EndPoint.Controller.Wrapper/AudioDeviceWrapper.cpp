#include "stdafx.h"
#include "AudioDeviceWrapper.h"

using namespace System;

void AudioEndPointControllerWrapper::AudioDeviceWrapper::SetAsDefault()
{
	this->_audioDevice->SetDefault();
}
