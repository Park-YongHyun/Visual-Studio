using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace SoundControl
{
	class Win32Api
	{
		private static readonly MMDeviceEnumerator mMDeviceEnumerator = new();

		// 볼륨 제어
		public class VolumeControl
		{
			// Virtual-Key Codes
			// https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
			private const int VK_VOLUME_MUTE = 0xAD;    // Volume Mute key
			private const int VK_VOLUME_DOWN = 0xAE;    // Volume Down key
			private const int VK_VOLUME_UP = 0xAF;      // Volume Up key

			public static event EventHandler<VolumeChangedEventArgs> VolumeChanged;

			private static int currentVolumeStep; // = 0

			public class VolumeChangedEventArgs : EventArgs
			{
				public VolumeChangedEventArgs(int volumeLevel)
				{
					VolumeLevel = volumeLevel;
				}

				public int VolumeLevel { get; }
			}
			public static void OnVolumeChanged(int VolumeLevel)
			{
				VolumeChanged?.Invoke(null, new VolumeChangedEventArgs(VolumeLevel));
			}

			/*	키보드 후크, 특정 프로그램에서 사용 불가(핫키와 다름), 우선순위 1
			 *	키 등록 없이 모든 입력 감지
			 */
			public class WindowsHook
			{
				// SetWindowsHookExW function (winuser.h)
				// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowshookexw
				[DllImport("user32.dll")]
				private static extern IntPtr SetWindowsHookExW([In] int idHook, [In] LowLevelKeyboardProc lpfn, [In] IntPtr hmod, [In] uint dwThreadId);

				// UnhookWindowsHookEx function (winuser.h)
				// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-unhookwindowshookex
				[DllImport("user32.dll")]
				private static extern bool UnhookWindowsHookEx([In] IntPtr hhk);

				// CallNextHookEx function (winuser.h)
				// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-callnexthookex
				[DllImport("user32.dll")]
				private static extern IntPtr CallNextHookEx([In, Optional] IntPtr hhk, [In] int nCode, [In] IntPtr wParam, [In] IntPtr lParam);

				private const int WH_KEYBOARD_LL = 13;
#pragma warning disable IDE0051 // 사용되지 않는 private 멤버 제거
				private const int WM_KEYDOWN = 0x0100;
				private const int WM_KEYUP = 0x0101;
#pragma warning restore IDE0051 // 사용되지 않는 private 멤버 제거

				// SetWindowsHookExW(default)
				// HOOKPROC callback function (winuser.h)
				// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nc-winuser-hookproc
				//private delegate IntPtr Hookproc(int code, [In] IntPtr wParam, [In] IntPtr lParam);

				// SetWindowsHookExW(WH_KEYBOARD_LL)
				// LowLevelKeyboardProc callback function
				// https://docs.microsoft.com/en-us/previous-versions/windows/desktop/legacy/ms644985(v=vs.85)
				private delegate IntPtr LowLevelKeyboardProc(int code, [In] IntPtr wParam, [In] IntPtr lParam);

				// KBDLLHOOKSTRUCT structure (winuser.h)
				// https://docs.microsoft.com/ko-kr/windows/win32/api/winuser/ns-winuser-kbdllhookstruct?redirectedfrom=MSDN
				private struct KBDLLHOOKSTRUCT
				{
					public uint vkCode;
					public uint scanCode;
					public uint flags;
					public uint time;
					public IntPtr dwExtraInfo;
				}

				private static readonly LowLevelKeyboardProc hookProc = Hookproc1;
				private static IntPtr hHook;

				private static bool enabled = false;

				public static IntPtr Hookproc1(int code, [In] IntPtr wParam, [In] IntPtr lParam)
				{
					if (wParam.ToInt32() == WM_KEYDOWN)
					{
						//int virtualKeyCode = Marshal.ReadInt32(lParam);
						KBDLLHOOKSTRUCT keyEventInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT))!;

						if (HotKeyHandling((int)keyEventInfo.vkCode))
						{
							Debug.WriteLine("");
							Debug.WriteLine("WindowsHook {");
							Debug.WriteLine($"{nameof(code)} = {code}");
							Debug.WriteLine($"{nameof(wParam)}:X = {wParam.ToInt32():X}");
							Debug.WriteLine($"{nameof(lParam)}.vkCode:X = {keyEventInfo.vkCode:X}");
							Debug.WriteLine("}");

							return (IntPtr)1;
						}
					}

					return CallNextHookEx(hHook, code, wParam, lParam);
				}

				public static void SetWindowsHookEx()
				{
					hHook = SetWindowsHookExW(WH_KEYBOARD_LL, hookProc, IntPtr.Zero, 0);

					enabled = true;
				}

				public static void UnhookWindowsHookEx()
				{
					if (enabled)
					{
						UnhookWindowsHookEx(hHook);

						enabled = false;
					}
				}
			}

			/*	글로벌 핫키, 특정 프로그램에서 사용 불가(후크와 다름), 우선순위 2
			 *	키마다 개별로 등록
			 *	handled 반환 값에 관계없이 다른 프로그램에 키 입력 전달 안됨
			 */
			public class RegHotKey
			{
				// RegisterHotKey function (winuser.h)
				// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey
				[DllImport("user32.dll")]
				private static extern bool RegisterHotKey([In, Optional] IntPtr hWnd, [In] int id, [In] uint fsModifiers, [In] uint vk);

				// UnregisterHotKey function (winuser.h)
				// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-unregisterhotkey
				[DllImport("user32.dll")]
				private static extern bool UnregisterHotKey([In, Optional] IntPtr hWnd, [In] int id);

				private const int WM_HOTKEY = 0x0312;

				private const int HOTKEY_ID = 9000;

				private static IntPtr windowHandle;
				private static HwndSource hwndSource;

				private static bool enabled = false;

				// HwndSourceHook Delegate
				// https://docs.microsoft.com/en-us/dotnet/api/system.windows.interop.hwndsourcehook?view=net-5.0
				public static IntPtr HwndSourceHook1(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
				{
					if (msg == WM_HOTKEY)
					{
						int virtualKeyCode = (int)lParam >> 16;

						Debug.WriteLine("");
						Debug.WriteLine("RegHotKey {");
						Debug.WriteLine($"{nameof(hwnd)} = {hwnd}");
						Debug.WriteLine($"{nameof(msg)}:X = {msg:X}");
						Debug.WriteLine($"{nameof(wParam)} = {wParam}"); // HOTKEY_ID
						Debug.WriteLine($"{nameof(lParam)}.vkCode:X = {virtualKeyCode:X}");
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
						handled = HotKeyHandling(virtualKeyCode);
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

				public static void RegisterHotKey(System.Windows.Window window)
				{
					windowHandle = new WindowInteropHelper(window).Handle;
					hwndSource = HwndSource.FromHwnd(windowHandle);
					hwndSource.AddHook(HwndSourceHook1);

					//RegisterHotKey(windowHandle, HOTKEY_ID, 0x0000, VK_VOLUME_MUTE);
					RegisterHotKey(windowHandle, HOTKEY_ID, 0x0000, VK_VOLUME_DOWN);
					RegisterHotKey(windowHandle, HOTKEY_ID, 0x0000, VK_VOLUME_UP);

					enabled = true;
				}

				public static void UnregisterHotKey()
				{
					if (enabled)
					{
						hwndSource.RemoveHook(HwndSourceHook1);

						UnregisterHotKey(windowHandle, HOTKEY_ID);

						enabled = false;
					}
				}
			}

			/*	볼륨 레벨 동기화
			 *	볼륨 변경
			 *	시스템 볼륨 OSD or 커스텀 볼륨 팝업 표시
			 */
			public static bool HotKeyHandling(int virtualKeyCode)
			{
				switch (virtualKeyCode)
				{
					//case VK_VOLUME_MUTE:
					case VK_VOLUME_DOWN:
					case VK_VOLUME_UP:
						break;

					default:
						return false;
				}

				// 볼륨 레벨 동기화
				MMDevice defaultAudioDevice = mMDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console); // DataFlow.All => Exception
				double currentVolumeLevelScalar = Math.Round(defaultAudioDevice.AudioEndpointVolume.MasterVolumeLevelScalar, 5);
				Debug.WriteLine($"{nameof(currentVolumeLevelScalar)} = {currentVolumeLevelScalar}");
				Debug.WriteLine($"{nameof(Config.GetRoot.Volume.Level)} = {Config.GetRoot.Volume.Level.List[currentVolumeStep]}");

				if (Config.GetRoot.Volume.Level.List[currentVolumeStep] != currentVolumeLevelScalar)
				{
					for (int i = 0; i < Config.GetRoot.Volume.Level.List.Length && Config.GetRoot.Volume.Level.List[i] <= currentVolumeLevelScalar; i++)
					{
						currentVolumeStep = i;
					}
					Debug.WriteLine($"{nameof(currentVolumeStep)} = {currentVolumeStep}");
				}

				// 볼륨 변경
				switch (virtualKeyCode)
				{
					case VK_VOLUME_MUTE:
						break;
					case VK_VOLUME_DOWN:
						currentVolumeStep = currentVolumeStep > 0 ? currentVolumeStep - 1 : currentVolumeStep;
						break;
					case VK_VOLUME_UP:
						currentVolumeStep = currentVolumeStep < Config.GetRoot.Volume.Level.List.Length - 1 ? currentVolumeStep + 1 : currentVolumeStep;
						break;

					default:
						break;
				}
				defaultAudioDevice.AudioEndpointVolume.MasterVolumeLevelScalar = (float)Config.GetRoot.Volume.Level.List[currentVolumeStep];

				// 볼륨 팝업 표시
				ShowVolumePopup(defaultAudioDevice.AudioEndpointVolume.MasterVolumeLevelScalar);

				return true;
			}

			// 커스텀 볼륨 팝업 표시
			public static void ShowVolumePopup(double volumeLevel = -1)
			{
				if (volumeLevel < 0)
				{
					volumeLevel = mMDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console).AudioEndpointVolume.MasterVolumeLevelScalar;
				}
				OnVolumeChanged((int)Math.Round(volumeLevel * 100, 0));
			}
		}

		/*	기본 재생 장치 변경 트리거
		 *	윈도우 메시지 리스너
		 */
		public class SwitchDefaultAudioDevice
		{
			public class WinMessage
			{
				private static IntPtr windowHandle;
				private static HwndSource hwndSource;

				private static bool enabled = false;

				private const int WM_APP = 0x8000;

				// HwndSourceHook Delegate
				// https://docs.microsoft.com/en-us/dotnet/api/system.windows.interop.hwndsourcehook?view=net-5.0
				public static IntPtr HwndSourceHook1(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
				{
					if (msg == WM_APP)
					{
						Debug.WriteLine("");
						Debug.WriteLine("WinMessage {");
						Debug.WriteLine($"{nameof(hwnd)} = {hwnd}");
						Debug.WriteLine($"{nameof(msg)}:X = {msg:X}");
						Debug.WriteLine($"{nameof(wParam)} = {wParam}");
						Debug.WriteLine($"{nameof(lParam)} = {lParam}");
						Debug.WriteLine("}");

						handled = MessageHandling();
					}

					return IntPtr.Zero;
				}

				public static void AddHook(System.Windows.Window window)
				{
					windowHandle = new WindowInteropHelper(window).Handle;
					hwndSource = HwndSource.FromHwnd(windowHandle);
					hwndSource.AddHook(HwndSourceHook1);

					string filePath = $"{Directory.GetParent(Environment.ProcessPath!)}/hwnd.txt";
					File.WriteAllText(filePath, windowHandle.ToString());

					enabled = true;
				}

				public static void RemoveHook()
				{
					if (enabled)
					{
						hwndSource.RemoveHook(HwndSourceHook1);

						enabled = false;
					}
				}
			}

			/*	기본 재생 장치 변경
			 *	볼륨 팝업 표시
			 *	시스템 사운드 음소거 해제
			 *	확인차 소리 재생(시스템 사운드)
			 */
			public static bool MessageHandling()
			{
				// 재생 장치 변경
				Config.Audio configAudio = Config.GetRoot.Audio;
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

#if DEBUG
		public static void Test()
		{
			SwitchDefaultAudioDevice.MessageHandling();
		}
#endif
	}
}
