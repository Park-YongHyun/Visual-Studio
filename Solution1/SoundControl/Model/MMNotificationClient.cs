using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundControl.Model
{
	// 참조: https://itecnote.com/tecnote/c-handling-changed-audio-device-event-in-c/
	internal class MMNotificationClient : NAudio.CoreAudioApi.Interfaces.IMMNotificationClient
	{
		private Action OnDefaultDeviceChangedProcess { get; set; }

		public void SetOnDefaultDeviceChangedProcess(Action process)
		{
			OnDefaultDeviceChangedProcess = process;
		}

		public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
		{
			Debug.WriteLine(nameof(OnDefaultDeviceChanged));
			Debug.WriteLine($"\t{nameof(flow)} = {flow}");
			Debug.WriteLine($"\t{nameof(role)} = {role}");
			Debug.WriteLine($"\t{nameof(defaultDeviceId)} = {defaultDeviceId}");

			if (role == Role.Console) OnDefaultDeviceChangedProcess();
		}

		public void OnDeviceAdded(string pwstrDeviceId)
		{
			Debug.WriteLine(nameof(OnDeviceAdded));
			Debug.WriteLine($"\t{nameof(pwstrDeviceId)} = {pwstrDeviceId}");
		}

		public void OnDeviceRemoved(string deviceId)
		{
			Debug.WriteLine(nameof(OnDeviceRemoved));
			Debug.WriteLine($"\t{nameof(deviceId)} = {deviceId}");
		}

		public void OnDeviceStateChanged(string deviceId, DeviceState newState)
		{
			Debug.WriteLine(nameof(OnDeviceStateChanged));
			Debug.WriteLine($"\t{nameof(deviceId)} = {deviceId}");
			Debug.WriteLine($"\t{nameof(newState)} = {newState}");
		}

		public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
		{
			Debug.WriteLine(nameof(OnPropertyValueChanged));
			Debug.WriteLine($"\t{nameof(pwstrDeviceId)} = {pwstrDeviceId}");
			Debug.WriteLine($"\t{nameof(key)} = {key}");
		}
	}
}
