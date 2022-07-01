using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using SoundControl.Configuration;

namespace SoundControl.Model
{
	internal class SoundDevice
	{
		private static readonly MMDeviceEnumerator mMDeviceEnumerator = new();

		public class VolumeControl
		{
			/*	볼륨 계산
			 *		데시벨(dB)인 로그함수의 역함수인 지수함수로 계산
			 *		기본 공식, x >= 0, x <= 1, y 최대값: 1, y 최소값과 최대값의 차이: {base}배
			 *			y = pow(base, x) / base
			 *	
			 *		[ ] y 최소값을 0으로 선형 보정
			 *			y = (pow(base, x) / base) + ((x - 1) / base)
			 *		[ ] y 최소값을 a로 선형 보정
			 *			y = (pow(base, x) / base) + ((x - 1) / base) + ((-x + 1) * a)
			 *		[v] y 최소값을 0으로 선형 배율 보정
			 *			y = ((pow(base, x) / base) - (1 / base)) * (1 / (1 - (1 / base)))
			 */
			public static bool CalculateVolumeLevel()
			{
				ConfigData.Volume volume = Config.GetData.Volume;
				if (!volume.Level.Calculated)
				{
					volume.Level.List = new double[volume.StepCount];
					int expBase = volume.Level.ExponentiationBase;
					double minLevel = volume.Level.MinLevel;
					double maxLevel = volume.Level.MaxLevel;
					double middleLevel = maxLevel - minLevel;

					for (int i = 0; i < volume.StepCount; i++)
					{
						double x = i / (volume.StepCount - 1.0);
						//double calcValue = (Math.Pow(expBase, x) / expBase) + ((x - 1.0) / expBase);
						double calcValue = ((Math.Pow(expBase, x) / expBase) - (1.0 / expBase)) * (1.0 / (1.0 - (1.0 / expBase)));
						double volumeLevel = minLevel + (middleLevel * calcValue);
						volume.Level.List[i] = double.Parse(volumeLevel.ToString("G3")); // 정밀도 유효숫자 3
					}

					return volume.Level.Calculated = true;
				}
				return false;
			}

			public static event EventHandler<VolumeChangedEventArgs> VolumeChanged;

			private static int currentVolumeStep; // = 0

			public class VolumeChangedEventArgs : EventArgs
			{
				public VolumeChangedEventArgs(double volumeLevel)
				{
					VolumeLevel = volumeLevel;
				}

				public double VolumeLevel { get; }
			}
			public static void OnVolumeChanged(double VolumeLevel)
			{
				VolumeChanged?.Invoke(null, new VolumeChangedEventArgs(VolumeLevel));
			}

			public enum VolumeContolType
			{
				VolumeMute,
				VolumeDown,
				VolumeUp
			}

			/*	볼륨 레벨 동기화
			 *	볼륨 변경
			 *	시스템 볼륨 OSD or 커스텀 볼륨 팝업 표시
			 */
			public static bool ControlVolume(VolumeContolType volumeContolType)
			{
				// 볼륨 레벨 동기화
				MMDevice defaultAudioDevice = mMDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console); // DataFlow.All => Exception
				double currentVolumeLevelScalar = double.Parse(defaultAudioDevice.AudioEndpointVolume.MasterVolumeLevelScalar.ToString("G3")); // 정밀도 유효숫자 3
				Debug.WriteLine($"{nameof(currentVolumeLevelScalar)} = {currentVolumeLevelScalar}");
				Debug.WriteLine($"{nameof(Config.GetData.Volume.Level)} = {Config.GetData.Volume.Level.List[currentVolumeStep]}");

				if (Config.GetData.Volume.Level.List[currentVolumeStep] != currentVolumeLevelScalar)
				{
					for (int i = 0; i < Config.GetData.Volume.Level.List.Length && Config.GetData.Volume.Level.List[i] <= currentVolumeLevelScalar; i++)
					{
						currentVolumeStep = i;
					}
					Debug.WriteLine($"{nameof(currentVolumeStep)} = {currentVolumeStep}");
				}

				// 볼륨 변경
				switch (volumeContolType)
				{
					case VolumeContolType.VolumeMute:
						break;
					case VolumeContolType.VolumeDown:
						currentVolumeStep = currentVolumeStep > 0 ? currentVolumeStep - 1 : currentVolumeStep;
						break;
					case VolumeContolType.VolumeUp:
						currentVolumeStep = currentVolumeStep < Config.GetData.Volume.Level.List.Length - 1 ? currentVolumeStep + 1 : currentVolumeStep;
						break;

					default:
						break;
				}
				defaultAudioDevice.AudioEndpointVolume.MasterVolumeLevelScalar = (float)Config.GetData.Volume.Level.List[currentVolumeStep];

				// 볼륨 팝업 표시
				ShowVolumePopup(defaultAudioDevice.AudioEndpointVolume.MasterVolumeLevelScalar);
				// todo 시스템 볼륨 osd 표시 전환

				return true;
			}

			// 커스텀 볼륨 팝업 표시
			public static void ShowVolumePopup(double volumeLevel = -1)
			{
				if (volumeLevel < 0)
				{
					volumeLevel = mMDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console).AudioEndpointVolume.MasterVolumeLevelScalar;
				}
				OnVolumeChanged(volumeLevel * 100);
			}
		}

		public class SwitchDefaultAudioDevice
		{
			/*	기본 재생 장치 변경
			 *	볼륨 팝업 표시
			 *	시스템 사운드 음소거 해제
			 *	확인차 소리 재생(시스템 사운드)
			 */
			public static bool SwitchDevice()
			{
				// 재생 장치 변경
				ConfigData.Audio configAudio = Config.GetData.Audio;
				MMDevice defaultDevice = mMDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
				string targetDeviceName = defaultDevice.FriendlyName.StartsWith(configAudio.Device1Name) ? configAudio.Device2Name : configAudio.Device1Name;

				foreach (MMDevice device in mMDeviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
				{
					if (device.FriendlyName.StartsWith(targetDeviceName))
					{
						CoreAudioApi.PolicyConfigClient policyConfigClient = new();
						policyConfigClient.SetDefaultEndpoint(device.ID, CoreAudioApi.ERole.eConsole);
						policyConfigClient.SetDefaultEndpoint(device.ID, CoreAudioApi.ERole.eMultimedia);
						policyConfigClient.SetDefaultEndpoint(device.ID, CoreAudioApi.ERole.eCommunications);

						// 시스템 사운드 음소거 해제
						if (configAudio.UnmuteSystemSound)
						{
							SessionCollection audioSessions = device.AudioSessionManager.Sessions;
							for (int i = 0; i < audioSessions.Count; i++)
							{
								if (audioSessions[i].IsSystemSoundsSession && audioSessions[i].SimpleAudioVolume.Mute)
								{
									Debug.WriteLine("unmute SystemSoundsSession");
									audioSessions[i].SimpleAudioVolume.Mute = false;
								}
							}
						}
						break;
					}
				}

				// 볼륨 팝업 표시
				VolumeControl.ShowVolumePopup();

				System.Media.SystemSounds.Beep.Play();

				return true;
			}
		}
	}
}
