using CSCore.CoreAudioAPI; // https://github.com/filoe/cscore

namespace UnmuteSystemSounds
{
	class Program
	{
		static void Main(string[] args)
		{
			// 모든 활성 장치의 시스템 사운드 음소거 해제
			using var deviceEnumerator = new MMDeviceEnumerator();
			using var devices = deviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);
			foreach (var device in devices)
			{
				using var sessionManager = AudioSessionManager2.FromMMDevice(device);
				using var sessionEnumerator = sessionManager.GetSessionEnumerator();
				foreach (var session in sessionEnumerator)
				{
					using var sessionControl = session.QueryInterface<AudioSessionControl2>();
					if (sessionControl.IsSystemSoundSession == true)
					{
						using var sessionVolume = session.QueryInterface<SimpleAudioVolume>();
						sessionVolume.IsMuted = false;
					}
				}
			}
		}
	}
}
