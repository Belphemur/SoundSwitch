#pragma once
#include "AudioDevice.h"

using namespace System;
namespace AudioEndPointControllerWrapper {
	public ref class AudioDeviceWrapper
	{
	private:
		AudioDevice *_audioDevice;
	public:
		AudioDeviceWrapper(AudioDevice *device) : _audioDevice(device) {}
		~AudioDeviceWrapper() { this->!AudioDeviceWrapper(); }
		!AudioDeviceWrapper() { delete _audioDevice; }
		property String^ FriendlyName {
			String^ get()
			{
				return gcnew String(_audioDevice->FriendlyName);
			}
		}

		property String^ Description {
			String^ get()
			{
				return gcnew String(_audioDevice->Description);
			}
		}

		property String^ Id {
			String^ get()
			{
				return gcnew String(_audioDevice->Id);
			}
		}

		property String^ InterfaceFriendlyName {
			String^ get()
			{
				return gcnew String(_audioDevice->InterfaceFriendlyName);
			}
		}

		property bool IsDefault {
			bool get()
			{
				return _audioDevice->IsDefault;
			}
		}

		property int State {
			int get()
			{
				return _audioDevice->State;
			}
		}


		void SetAsDefault();
	};
}

