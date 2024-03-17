using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Threading;
using NAudio.CoreAudioApi;
using SoundControl.Configuration;

namespace SoundControl.Model
{
	internal class SoundDevice
	{
		public SoundDevice()
		{
			hotKeyAndMessage = new(this);
			volumeControl = new(this);
			switchDefaultAudioDevice = new(this);
			popupControl = new(this);
		}

		private static SoundDevice _instance; // 싱글턴

		public readonly HotKeyAndMessage hotKeyAndMessage;
		public readonly VolumeControl volumeControl;
		public readonly SwitchDefaultAudioDevice switchDefaultAudioDevice;
		public readonly PopupControl popupControl;

		private readonly MMDeviceEnumerator mMDeviceEnumerator = new();


		public static SoundDevice GetInstance
		{
			get
			{
				_instance ??= new();
				return _instance;
			}
		}


		public class HotKeyAndMessage
		{
			public HotKeyAndMessage(SoundDevice parent)
			{
				this.parent = parent;

				windowsHook = new(this);
				regHotKey = new(this);
				winMessage = new(this);
			}

			public readonly SoundDevice parent;

			public readonly WindowsHook windowsHook;
			public readonly RegHotKey regHotKey;
			public readonly WinMessage winMessage;


			/*	키보드 후크, 특정 프로그램 포커스 상태에서 사용 불가(핫키와 다름), 우선순위 1
			 *	키 등록 없이 모든 입력 감지
			 *	리턴 값에 따라 키 입력 전파/중지 가능
			 */
			public class WindowsHook
			{
				public WindowsHook(HotKeyAndMessage parent)
				{
					this.parent = parent;

					lowLevelKeyboardProc = LowLevelKeyboardProc1;
				}

				public readonly HotKeyAndMessage parent;

				private readonly Win32Api.WindowsHook.LowLevelKeyboardProc lowLevelKeyboardProc;
				private IntPtr hHook;

				private bool enabled = false;

				public IntPtr LowLevelKeyboardProc1(int nCode, [In] IntPtr wParam, [In] IntPtr lParam)
				{
					if (wParam.ToInt32() == Win32Api.WindowsHook.WM_KEYDOWN)
					{
						//int virtualKeyCode = Marshal.ReadInt32(lParam);
						Win32Api.WindowsHook.KBDLLHOOKSTRUCT keyEventInfo = (Win32Api.WindowsHook.KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(Win32Api.WindowsHook.KBDLLHOOKSTRUCT))!;

						if (parent.HotKeyHandling(keyEventInfo.vkCode))
						{
							Debug.WriteLine("");
							Debug.WriteLine("WindowsHook {");
							Debug.WriteLine($"\t{nameof(nCode)} = {nCode}");
							Debug.WriteLine($"\t{nameof(wParam)}:X = {wParam.ToInt32():X}");
							Debug.WriteLine($"\t{nameof(lParam)}.vkCode:X = {keyEventInfo.vkCode:X}");
							Debug.WriteLine("}");

							return (IntPtr)1;
						}
					}

					return Win32Api.WindowsHook.CallNextHookEx(hHook, nCode, wParam, lParam);
				}

				public void SetWindowsHookEx()
				{
					hHook = Win32Api.WindowsHook.SetWindowsHookExW(Win32Api.WindowsHook.WH_KEYBOARD_LL, lowLevelKeyboardProc, IntPtr.Zero, 0);

					enabled = true;
				}

				public void UnhookWindowsHookEx()
				{
					if (enabled)
					{
						Win32Api.WindowsHook.UnhookWindowsHookEx(hHook);

						enabled = false;
					}
				}
			}

			/*	글로벌 핫키, 특정 프로그램 포커스 상태에서 사용 불가(후크와 다름), 우선순위 2
			 *	키마다 개별로 등록
			 *	handled 참조 값에 관계없이 다른 프로그램에 키 입력 전달 안됨
			 */
			public class RegHotKey
			{
				public RegHotKey(HotKeyAndMessage parent)
				{
					this.parent = parent;
				}

				public readonly HotKeyAndMessage parent;


				private IntPtr windowHandle;
				private HwndSource hwndSource;

				private bool enabled = false;

				// HwndSourceHook Delegate
				// https://docs.microsoft.com/en-us/dotnet/api/system.windows.interop.hwndsourcehook?view=windowsdesktop-6.0
				public IntPtr HwndSourceHook1(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) // wParam = HOTKEY_ID
				{
					if (msg == Win32Api.RegHotKey.WM_HOTKEY)
					{
						uint virtualKeyCode = (uint)lParam >> 16;

						Debug.WriteLine("");
						Debug.WriteLine("RegHotKey {");
						Debug.WriteLine($"\t{nameof(hwnd)} = {hwnd}");
						Debug.WriteLine($"\t{nameof(msg)}:X = {msg:X}");
						Debug.WriteLine($"\t{nameof(wParam)} = {wParam}");
						Debug.WriteLine($"\t{nameof(lParam)}.vkCode:X = {virtualKeyCode:X}");
						Debug.WriteLine("}");

						//if (wParam.ToInt32() == HOTKEY_ID)
						//{
						//	int vkey = ((int)lParam >> 16) & 0xFFFF;
						//	//if (vkey == HOTKEY_1)
						//	//{
						//	//	Debug.WriteLine(vkey);
						//	//}
						//	Debug.WriteLine($"{vkey:X}");
						//}
						handled = parent.HotKeyHandling(virtualKeyCode);
					}
					//else
					//{
					//	Debug.WriteLine("test {");
					//	Debug.WriteLine($"{nameof(hwnd)} = {hwnd}");
					//	Debug.WriteLine($"{nameof(msg)}:X = {msg:X}");
					//	Debug.WriteLine($"{nameof(wParam)} = {wParam}");
					//	//Debug.WriteLine($"{nameof(lParam)}.convert:X = {(int)lParam >> 16:X}");
					//	Debug.WriteLine("}");
					//}

					return IntPtr.Zero;
				}

				public void RegisterHotKey(System.Windows.Window window)
				{
					windowHandle = new WindowInteropHelper(window).Handle;
					hwndSource = HwndSource.FromHwnd(windowHandle);
					hwndSource.AddHook(HwndSourceHook1);

					//RegisterHotKey(windowHandle, HOTKEY_ID, 0x0000, VK_VOLUME_MUTE);
					Win32Api.RegHotKey.RegisterHotKey(windowHandle, Win32Api.RegHotKey.HOTKEY_ID, 0x0000, Win32Api.VK_VOLUME_DOWN);
					Win32Api.RegHotKey.RegisterHotKey(windowHandle, Win32Api.RegHotKey.HOTKEY_ID, 0x0000, Win32Api.VK_VOLUME_UP);

					enabled = true;
				}

				public void UnregisterHotKey()
				{
					if (enabled)
					{
						hwndSource.RemoveHook(HwndSourceHook1);

						Win32Api.RegHotKey.UnregisterHotKey(windowHandle, Win32Api.RegHotKey.HOTKEY_ID);

						enabled = false;
					}
				}
			}


			// 윈도우 메시지 리스너
			public class WinMessage
			{
				public WinMessage(HotKeyAndMessage parent)
				{
					this.parent = parent;
				}

				public readonly HotKeyAndMessage parent;

				private IntPtr windowHandle;
				private HwndSource hwndSource;

				private bool enabled = false;

				// HwndSourceHook Delegate
				// https://docs.microsoft.com/en-us/dotnet/api/system.windows.interop.hwndsourcehook?view=windowsdesktop-6.0
				public IntPtr HwndSourceHook1(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
				{
					if (msg == Win32Api.WinMessage.WM_APP)
					{
						Debug.WriteLine("");
						Debug.WriteLine("WinMessage {");
						Debug.WriteLine($"{nameof(hwnd)} = {hwnd}");
						Debug.WriteLine($"{nameof(msg)}:X = {msg:X}");
						Debug.WriteLine($"{nameof(wParam)} = {wParam}");
						Debug.WriteLine($"{nameof(lParam)} = {lParam}");
						Debug.WriteLine("}");

						handled = parent.parent.switchDefaultAudioDevice.SwitchDevice();
					}

					return IntPtr.Zero;
				}

				public void AddHook(System.Windows.Window window)
				{
					windowHandle = new WindowInteropHelper(window).Handle;
					hwndSource = HwndSource.FromHwnd(windowHandle);
					hwndSource.AddHook(HwndSourceHook1);

					string filePath = $"{Directory.GetParent(Environment.ProcessPath!)}/hwnd.txt";
					File.WriteAllText(filePath, windowHandle.ToString());

					enabled = true;
				}

				public void RemoveHook()
				{
					if (enabled)
					{
						hwndSource.RemoveHook(HwndSourceHook1);

						enabled = false;
					}
				}
			}


			// [후크, 핫키]의 키입력 처리
			private bool HotKeyHandling(uint virtualKeyCode)
			{
				VolumeControl.VolumeContolType volumeControlType;

				switch (virtualKeyCode)
				{
					//case Win32Api.WindowsHook.VK_VOLUME_MUTE:
					//	volumeControlType = VolumeControl.VolumeContolType.VolumeMute;
					//	break;
					case Win32Api.VK_VOLUME_DOWN:
						volumeControlType = VolumeControl.VolumeContolType.VolumeDown;
						break;
					case Win32Api.VK_VOLUME_UP:
						volumeControlType = VolumeControl.VolumeContolType.VolumeUp;
						break;

					default:
						return false;
				}

				return parent.volumeControl.ControlVolume(volumeControlType); // 볼륨 제어
			}
		}

		// 볼륨 제어
		public class VolumeControl
		{
			public VolumeControl(SoundDevice parent)
			{
				this.parent = parent;
			}

			public readonly SoundDevice parent;

			public event EventHandler<VolumeChangedEventArgs> VolumeChanged;

			private int currentVolumeStep;

			public class VolumeChangedEventArgs : EventArgs
			{
				public VolumeChangedEventArgs(string deviceName, double volumeLevel)
				{
					DeviceName = deviceName;
					VolumeLevel = volumeLevel;
				}

				public string DeviceName { get; }
				public double VolumeLevel { get; }
			}
			public void OnVolumeChanged(string deviceName, double VolumeLevel)
			{
				VolumeChanged?.Invoke(null, new VolumeChangedEventArgs(deviceName, VolumeLevel));
			}

			public enum VolumeContolType
			{
				VolumeMute,
				VolumeDown,
				VolumeUp
			}

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
			public bool CalculateVolumeLevel()
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

			/*	볼륨 레벨 동기화
			 *	볼륨 변경
			 *	시스템 볼륨 OSD or 커스텀 볼륨 팝업 표시
			 */
			public bool ControlVolume(VolumeContolType volumeContolType)
			{
				// 볼륨 레벨 동기화
				MMDevice defaultAudioDevice = parent.mMDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console); // DataFlow.All => Exception
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
				parent.popupControl.ShowPopup(defaultAudioDevice.AudioEndpointVolume.MasterVolumeLevelScalar);

				return true;
			}
		}

		/*	기본 재생 장치 변경
		 *		트리거: 윈도우 메시지 리스너
		 */
		public class SwitchDefaultAudioDevice
		{
			public SwitchDefaultAudioDevice(SoundDevice parent)
			{
				this.parent = parent;

				mMDeviceEnumerator.RegisterEndpointNotificationCallback(mMNotificationClient); // 리스너 등록

				mMNotificationClient.SetOnDefaultDeviceChangedProcess(() =>
				{
					if (task == null || task.Status == TaskStatus.RanToCompletion)
					{
						task = Task.Run(async () =>
						{
							await Task.Delay(200);
							DeviceChenagedProcess();
						});
					}
				});
			}

			private readonly SoundDevice parent;

			private readonly MMDeviceEnumerator mMDeviceEnumerator = new();
			private readonly MMNotificationClient mMNotificationClient = new();
			private Task task;

			/*	기본 재생 장치 변경
			 *	볼륨 팝업 표시
			 *	시스템 사운드 음소거 해제
			 *	확인차 소리 재생(시스템 사운드)
			 */
			public bool SwitchDevice()
			{
				// 재생 장치 변경
				ConfigData.Audio configAudio = Config.GetData.Audio;
				MMDevice defaultDevice = parent.mMDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
				string targetDeviceName = defaultDevice.FriendlyName.StartsWith(configAudio.Device1Name) ? configAudio.Device2Name : configAudio.Device1Name;

				foreach (MMDevice device in parent.mMDeviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
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
				
				return true;
			}

			public void DeviceChenagedProcess()
			{
				// 볼륨 팝업 표시
				parent.popupControl.ShowPopup();

				// 테스트 비프 소리 재생
				ConfigData.TestBeep configTestBeep = Config.GetData.Audio.TestBeep;

				if (configTestBeep.UseSystemBeep)
				{
					System.Media.SystemSounds.Beep.Play();
				}
				else
				{
					Task.Run(() => Console.Beep(configTestBeep.Frequency, configTestBeep.DurationMillisec)); // Task.Run: 끝날 때까지 멈춤 방지
				}
			}
		}


		public class PopupControl
		{
			public PopupControl(SoundDevice parent)
			{
				this.parent = parent;

				systemVolumeOsd = new(this);
				customVolumePopup = new(this);
			}

			public readonly SoundDevice parent;

			public readonly SystemVolumeOsd systemVolumeOsd;
			public readonly CustomVolumePopup customVolumePopup;


			/*	시스템 볼륨 OSD
			 *		볼륨 제어 키 입력 + 재생 가능한 미디어가 있는 상태에서 멀티미디어 키 입력시 표시 됨
			 *		기본 지속시간 15.0초 + 페이드 아웃 0.75초
			 */
			// 키 입력 생성
			public class SystemVolumeOsd
			{
				public SystemVolumeOsd(PopupControl parent)
				{
					this.parent = parent;
				}

				public readonly PopupControl parent;


				private bool hidden = true;
				private IntPtr _hwnd = IntPtr.Zero;

				private DispatcherTimer _showTimeoutTimer;


				private readonly Win32Api.SystemVolumeOSD.INPUT[] inputarray = new Win32Api.SystemVolumeOSD.INPUT[]
				{
					new Win32Api.SystemVolumeOSD.INPUT
					{
						type = Win32Api.SystemVolumeOSD.INPUT_KEYBOARD,
						inputUnion = new()
						{
							ki = new()
							{
								wVk = Win32Api.VK_VOLUME_MUTE,
								dwFlags = Win32Api.SystemVolumeOSD.KEYEVENTF_KEYDOWN
							}
						}
					},
					new Win32Api.SystemVolumeOSD.INPUT
					{
						type = Win32Api.SystemVolumeOSD.INPUT_KEYBOARD,
						inputUnion = new()
						{
							ki = new()
							{
								wVk = Win32Api.VK_VOLUME_MUTE,
								dwFlags = Win32Api.SystemVolumeOSD.KEYEVENTF_KEYUP
							}
						}
					},
					new Win32Api.SystemVolumeOSD.INPUT
					{
						type = Win32Api.SystemVolumeOSD.INPUT_KEYBOARD,
						inputUnion = new()
						{
							ki = new()
							{
								wVk = Win32Api.VK_VOLUME_MUTE,
								dwFlags = Win32Api.SystemVolumeOSD.KEYEVENTF_KEYDOWN
							}
						}
					},
					new Win32Api.SystemVolumeOSD.INPUT
					{
						type = Win32Api.SystemVolumeOSD.INPUT_KEYBOARD,
						inputUnion = new()
						{
							ki = new()
							{
								wVk = Win32Api.VK_VOLUME_MUTE,
								dwFlags = Win32Api.SystemVolumeOSD.KEYEVENTF_KEYUP
							}
						}
					}
				};

				public IntPtr Hwnd
				{
					get
					{
						if (_hwnd == IntPtr.Zero)
						{
							IntPtr foundHwnd = IntPtr.Zero;
							while ((foundHwnd = Win32Api.SystemVolumeOSD.FindWindowExW(IntPtr.Zero, foundHwnd, "NativeHWNDHost", "")) != IntPtr.Zero)
							{
								if (Win32Api.SystemVolumeOSD.FindWindowExW(foundHwnd, IntPtr.Zero, "DirectUIHWND", "") != IntPtr.Zero)
								{
									_hwnd = foundHwnd;
									break;
								}
							}
						}
						return _hwnd;
					}
				}

				public DispatcherTimer ShowTimeoutTimer
				{
					get
					{
						if (_showTimeoutTimer == null)
						{
							_showTimeoutTimer = new()
							{
								Interval = TimeSpan.FromMilliseconds(Config.GetData.Popup.TimeoutMillisec)
							};
							_showTimeoutTimer.Tick += (sender, eventArgs) =>
							{
								Debug.WriteLine($"{nameof(_showTimeoutTimer)} elapsed");

								(sender as DispatcherTimer)?.Stop();

								Hide(); // 숨기기
							};
						}
						return _showTimeoutTimer;
					}
				}


				/*	볼륨 OSD 표시, 음소거 키 입력으로 트리거
				 *	숨기기는 자체 타임아웃 숨기기와 동일한 방식
				 */
				public void Show()
				{
					if (hidden)
					{
						_ = Win32Api.SystemVolumeOSD.SendInput((uint)inputarray.Length, inputarray, Win32Api.SystemVolumeOSD.INPUT.Size); // 볼륨 OSD 표시

						hidden = false;
					}

					if (ShowTimeoutTimer.IsEnabled)
					{
						ShowTimeoutTimer.Stop();
					}
					ShowTimeoutTimer.Start();
				}

				public void Hide()
				{
					if (!hidden)
					{
						Win32Api.SystemVolumeOSD.ShowWindow(Hwnd, Win32Api.SystemVolumeOSD.SW_HIDE);

						hidden = true;
					}

				}


				//// 작동함
				//public void Test()
				//{
				//	Win32Api.SystemVolumeOSD.keybd_event(Win32Api.VK_VOLUME_MUTE, 0, 0, 0);
				//	Win32Api.SystemVolumeOSD.keybd_event(Win32Api.VK_VOLUME_MUTE, 0, 0x0002, 0);
				//	Win32Api.SystemVolumeOSD.keybd_event(Win32Api.VK_VOLUME_MUTE, 0, 0, 0);
				//	Win32Api.SystemVolumeOSD.keybd_event(Win32Api.VK_VOLUME_MUTE, 0, 0x0002, 0);
				//}
			}


			public class CustomVolumePopup
			{
				public CustomVolumePopup(PopupControl parent)
				{
					this.parent = parent;
				}

				public PopupControl parent;

				// 커스텀 볼륨 팝업 표시
				public void Show(double volumeLevel = -1)
				{
					MMDevice defaultDevice = parent.parent.mMDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);

					Match m = Regex.Match(defaultDevice.FriendlyName, @".*(?=\((?:(?<group1>\()|[^()]|(?<-group1>\)))*\)$)");
					string deviceName = m.Value;

					if (volumeLevel < 0)
					{
						volumeLevel = defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar;
					}

					parent.parent.volumeControl.OnVolumeChanged(deviceName, volumeLevel * 100);
				}
			}


			public void ShowPopup(double volumeLevel = -1)
			{
				_ = Win32Api.PopupControl.SHQueryUserNotificationState(out Win32Api.PopupControl.QUERY_USER_NOTIFICATION_STATE result);
				Debug.WriteLine(result.ToString());

				if (result == Win32Api.PopupControl.QUERY_USER_NOTIFICATION_STATE.QUNS_RUNNING_D3D_FULL_SCREEN)
				{
					systemVolumeOsd.Show();
				}
				else
				{
					customVolumePopup.Show(volumeLevel);
				}
			}
		}



#if DEBUG
#pragma warning disable CS1998 // 이 비동기 메서드에는 'await' 연산자가 없으며 메서드가 동시에 실행됩니다.
		public async void Test()
#pragma warning restore CS1998 // 이 비동기 메서드에는 'await' 연산자가 없으며 메서드가 동시에 실행됩니다.
		{
			Debug.WriteLine("test {");

			//SwitchDefaultAudioDevice.SwitchDevice();

			popupControl.customVolumePopup.Show();
			popupControl.systemVolumeOsd.Show();
			_ = Task.Run(() => Console.Beep(100, 2000));

			//await VolumeControl.SystemVolumeOsd.Test();

			Debug.WriteLine("} test");
		}
#endif
	}
}
