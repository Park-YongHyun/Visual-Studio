using SoundControl.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Threading;

namespace SoundControl.Model
{
	class Win32Api
	{
		// 볼륨 제어
		public class VolumeControl
		{
			// Virtual-Key Codes
			// https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
			private const int VK_VOLUME_MUTE = 0xAD;    // Volume Mute key
			private const int VK_VOLUME_DOWN = 0xAE;    // Volume Down key
			private const int VK_VOLUME_UP = 0xAF;      // Volume Up key

			/*	키보드 후크, 특정 프로그램 포커스 상태에서 사용 불가(핫키와 다름), 우선순위 1
			 *	키 등록 없이 모든 입력 감지
			 *	리턴 값에 따라 키 입력 전파/중지 가능
			 */
			public class WindowsHook
			{
				private const int WH_KEYBOARD_LL = 13;

#pragma warning disable IDE0051 // 사용되지 않는 private 멤버 제거
				private const int WM_KEYDOWN = 0x0100;
				private const int WM_KEYUP = 0x0101;
#pragma warning restore IDE0051 // 사용되지 않는 private 멤버 제거

				// KBDLLHOOKSTRUCT structure (winuser.h)
				// https://docs.microsoft.com/ko-kr/windows/win32/api/winuser/ns-winuser-kbdllhookstruct?redirectedfrom=MSDN
				[StructLayout(LayoutKind.Sequential)]
				private class KBDLLHOOKSTRUCT
				{
					public uint vkCode;
					public uint scanCode;
					public uint flags;
					public uint time;
					public UIntPtr dwExtraInfo;
				}

				// SetWindowsHookExW function (winuser.h)
				// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowshookexw
				[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
				private static extern IntPtr SetWindowsHookExW([In] int idHook, [In] LowLevelKeyboardProc lpfn, [In] IntPtr hmod, [In] uint dwThreadId);

				// UnhookWindowsHookEx function (winuser.h)
				// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-unhookwindowshookex
				[DllImport("user32.dll", SetLastError = true)]
				private static extern bool UnhookWindowsHookEx([In] IntPtr hhk);

				// CallNextHookEx function (winuser.h)
				// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-callnexthookex
				[DllImport("user32.dll")]
				private static extern IntPtr CallNextHookEx([In, Optional] IntPtr hhk, [In] int nCode, [In] IntPtr wParam, [In] IntPtr lParam);

				// SetWindowsHookExW(default)
				// HOOKPROC callback function (winuser.h)
				// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nc-winuser-hookproc
				//private delegate IntPtr Hookproc(int code, [In] IntPtr wParam, [In] IntPtr lParam);

				// SetWindowsHookExW(WH_KEYBOARD_LL)
				// LowLevelKeyboardProc callback function
				// https://docs.microsoft.com/en-us/previous-versions/windows/desktop/legacy/ms644985(v=vs.85)
				private delegate IntPtr LowLevelKeyboardProc([In] int nCode, [In] IntPtr wParam, [In] IntPtr lParam);


				private static readonly LowLevelKeyboardProc lowLevelKeyboardProc = LowLevelKeyboardProc1;
				private static IntPtr hHook;

				private static bool enabled = false;

				public static IntPtr LowLevelKeyboardProc1([In] int nCode, [In] IntPtr wParam, [In] IntPtr lParam)
				{
					if (wParam.ToInt32() == WM_KEYDOWN)
					{
						//int virtualKeyCode = Marshal.ReadInt32(lParam);
						KBDLLHOOKSTRUCT keyEventInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT))!;

						if (HotKeyHandling(keyEventInfo.vkCode))
						{
							Debug.WriteLine("");
							Debug.WriteLine("WindowsHook {");
							Debug.WriteLine($"{nameof(nCode)} = {nCode}");
							Debug.WriteLine($"{nameof(wParam)}:X = {wParam.ToInt32():X}");
							Debug.WriteLine($"{nameof(lParam)}.vkCode:X = {keyEventInfo.vkCode:X}");
							Debug.WriteLine("}");

							return (IntPtr)1;
						}
					}

					return CallNextHookEx(hHook, nCode, wParam, lParam);
				}

				public static void SetWindowsHookEx()
				{
					hHook = SetWindowsHookExW(WH_KEYBOARD_LL, lowLevelKeyboardProc, IntPtr.Zero, 0);

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

			/*	글로벌 핫키, 특정 프로그램 포커스 상태에서 사용 불가(후크와 다름), 우선순위 2
			 *	키마다 개별로 등록
			 *	handled 참조 값에 관계없이 다른 프로그램에 키 입력 전달 안됨
			 */
			public class RegHotKey
			{
				private const int WM_HOTKEY = 0x0312;

				private const int HOTKEY_ID = 9000;

				// RegisterHotKey function (winuser.h)
				// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey
				[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
				private static extern bool RegisterHotKey([In, Optional] IntPtr hWnd, [In] int id, [In] uint fsModifiers, [In] uint vk);

				// UnregisterHotKey function (winuser.h)
				// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-unregisterhotkey
				[DllImport("user32.dll", SetLastError = true)]
				[return: MarshalAs(UnmanagedType.Bool)]
				private static extern bool UnregisterHotKey([In, Optional] IntPtr hWnd, [In] int id);


				private static IntPtr windowHandle;
				private static HwndSource hwndSource;

				private static bool enabled = false;

				// HwndSourceHook Delegate
				// https://docs.microsoft.com/en-us/dotnet/api/system.windows.interop.hwndsourcehook?view=windowsdesktop-6.0
				public static IntPtr HwndSourceHook1(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
				{
					if (msg == WM_HOTKEY)
					{
						uint virtualKeyCode = (uint)lParam >> 16;

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

			// [후크, 핫키]의 키입력 처리
			public static bool HotKeyHandling(uint virtualKeyCode)
			{
				SoundDevice.VolumeControl.VolumeContolType volumeControlType;

				switch (virtualKeyCode)
				{
					//case VK_VOLUME_MUTE:
					//	volumeControlType = SoundDevice.VolumeControl.VolumeContolType.VolumeMute;
					//	break;
					case VK_VOLUME_DOWN:
						volumeControlType = SoundDevice.VolumeControl.VolumeContolType.VolumeDown;
						break;
					case VK_VOLUME_UP:
						volumeControlType = SoundDevice.VolumeControl.VolumeContolType.VolumeUp;
						break;

					default:
						return false;
				}

				return SoundDevice.VolumeControl.ControlVolume(volumeControlType); // 볼륨 제어
			}

			/*	시스템 볼륨 OSD
			 *		볼륨 제어 키 입력 + 재생 가능한 미디어가 있는 상태에서 멀티미디어 키 입력시 표시 됨
			 *		기본 지속시간 15.0초 + 페이드 아웃 0.75초
			 */
			// 키 입력 생성
			public class SystemVolumeOSD
			{
				private const int INPUT_KEYBOARD = 1;

				private const int KEYEVENTF_KEYDOWN = 0x0000;
				private const int KEYEVENTF_KEYUP = 0x0002;

				private const int SW_MINIMIZE = 6;
				private const int SW_RESTORE = 9;

				// INPUT structure (winuser.h)
				// https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-input
				[StructLayout(LayoutKind.Sequential)]
				private struct INPUT
				{
					[StructLayout(LayoutKind.Explicit)]
					public struct InputUnion
					{
						// MOUSEINPUT structure (winuser.h)
						// https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-mouseinput
						[StructLayout(LayoutKind.Sequential)]
						public struct MOUSEINPUT
						{
							public int dx;
							public int dy;
							public uint mouseData;
							public uint dwFlags;
							public uint time;
							public UIntPtr dwExtraInfo;
						}
						// KEYBDINPUT structure (winuser.h)
						// https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-keybdinput
						[StructLayout(LayoutKind.Sequential)]
						public struct KEYBDINPUT
						{
							public ushort wVk;
							public ushort wScan;
							public uint dwFlags;
							public uint time;
							public UIntPtr dwExtraInfo;
						}
						// HARDWAREINPUT structure (winuser.h)
						// https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-hardwareinput
						[StructLayout(LayoutKind.Sequential)]
						public struct HARDWAREINPUT
						{
							public uint uMsg;
							public ushort wParamL;
							public ushort wParamH;
						}

						[FieldOffset(0)]
						public MOUSEINPUT mi;
						[FieldOffset(0)]
						public KEYBDINPUT ki;
						[FieldOffset(0)]
						public HARDWAREINPUT hi;
					}

					public uint type;
					public InputUnion inputUnion;
					public static int Size
					{
						get { return Marshal.SizeOf(typeof(INPUT)); }
					}
				}

				// SendInput function (winuser.h)
				// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendinput
				[DllImport("user32.dll")]
				private static extern uint SendInput([In] uint cInputs, [In] INPUT[] pInputs, [In] int cbSize);

				// FindWindowExW function (winuser.h)
				// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-findwindowexw
				[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
				private static extern IntPtr FindWindowExW([In, Optional] IntPtr hWndParent, [In, Optional] IntPtr hWndChildAfter, [In, Optional] string lpszClass, [In, Optional] string lpszWindow);

				// ShowWindow function (winuser.h)
				// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow
				[DllImport("user32.dll")]
				private static extern bool ShowWindow([In] IntPtr hWnd, [In] int nCmdShow);


				private static bool hidden = true;
				private static bool minimized = true;
				private static IntPtr _hwnd = IntPtr.Zero;

				private static DispatcherTimer _selfShowTimeoutTimer;
				private static DispatcherTimer _restoreAfterWaitTimer;
				private static DispatcherTimer _showTimeoutTimer;

				private static readonly INPUT[] inputarray = new INPUT[]
				{
					new INPUT
					{
						type = INPUT_KEYBOARD,
						inputUnion = new()
						{
							ki = new()
							{
								wVk = VK_VOLUME_MUTE,
								dwFlags = KEYEVENTF_KEYDOWN
							}
						}
					},
					new INPUT
					{
						type = INPUT_KEYBOARD,
						inputUnion = new()
						{
							ki = new()
							{
								wVk = VK_VOLUME_MUTE,
								dwFlags = KEYEVENTF_KEYUP
							}
						}
					},
					new INPUT
					{
						type = INPUT_KEYBOARD,
						inputUnion = new()
						{
							ki = new()
							{
								wVk = VK_VOLUME_MUTE,
								dwFlags = KEYEVENTF_KEYDOWN
							}
						}
					},
					new INPUT
					{
						type = INPUT_KEYBOARD,
						inputUnion = new()
						{
							ki = new()
							{
								wVk = VK_VOLUME_MUTE,
								dwFlags = KEYEVENTF_KEYUP
							}
						}
					}
				};

				public static IntPtr Hwnd
				{
					get
					{
						if (_hwnd == IntPtr.Zero)
						{
							IntPtr foundHwnd = IntPtr.Zero;
							while ((foundHwnd = FindWindowExW(IntPtr.Zero, foundHwnd, "NativeHWNDHost", "")) != IntPtr.Zero)
							{
								if (FindWindowExW(foundHwnd, IntPtr.Zero, "DirectUIHWND", "") != IntPtr.Zero)
								{
									_hwnd = foundHwnd;
									//break;
								}
							}
						}
						return _hwnd;
					}
				}

				public static DispatcherTimer SelfShowTimeoutTimer
				{
					get
					{
						if (_selfShowTimeoutTimer == null)
						{
							_selfShowTimeoutTimer = new()
							{
								Interval = TimeSpan.FromMilliseconds(15000 - Config.GetData.Popup.TimeoutMilliseconds)
							};
							_selfShowTimeoutTimer.Tick += (sender, eventArgs) =>
							{
								Debug.WriteLine($"{nameof(_selfShowTimeoutTimer)} elapsed");

								(sender as DispatcherTimer)?.Stop();

								RestoreAfterWaitTimer.Start();

								hidden = true;
							};
						}
						return _selfShowTimeoutTimer;
					}
				}
				public static DispatcherTimer RestoreAfterWaitTimer
				{
					get
					{
						if (_restoreAfterWaitTimer == null)
						{
							_restoreAfterWaitTimer = new()
							{
								Interval = TimeSpan.FromMilliseconds(1000 + Config.GetData.Popup.TimeoutMilliseconds)
							};
							_restoreAfterWaitTimer.Tick += (sender, eventArgs) =>
							{
								Debug.WriteLine($"{nameof(_restoreAfterWaitTimer)} elapsed");

								(sender as DispatcherTimer)?.Stop();

								Restore();
							};
						}
						return _restoreAfterWaitTimer;
					}
				}
				public static DispatcherTimer ShowTimeoutTimer
				{
					get
					{
						if (_showTimeoutTimer == null)
						{
							_showTimeoutTimer = new()
							{
								Interval = TimeSpan.FromMilliseconds(Config.GetData.Popup.TimeoutMilliseconds)
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
				 *	최소화로 숨김
				 *	볼륨 OSD 자체 표시 타임아웃 고려
				 *	자체 타임아웃 이후 최소화 복원
				 */
				public static void Show()
				{
					if (hidden)
					{
						_ = SendInput((uint)inputarray.Length, inputarray, INPUT.Size); // 볼륨 OSD 표시

						if (RestoreAfterWaitTimer.IsEnabled) RestoreAfterWaitTimer.Stop();
						SelfShowTimeoutTimer.Start();

						hidden = false;
					}

					Restore();

					if (ShowTimeoutTimer.IsEnabled)
					{
						ShowTimeoutTimer.Stop();
					}
					ShowTimeoutTimer.Start();
				}

				public static void Hide()
				{
					if (!minimized)
					{
						ShowWindow(Hwnd, SW_MINIMIZE);

						minimized = true;
					}

				}

				public static void Restore()
				{
					if (minimized)
					{
						ShowWindow(Hwnd, SW_RESTORE);

						minimized = false;
					}
				}


				//[DllImport("user32.dll")]
				//static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

				//// 작동함
				//public static void Test2()
				//{
				//	keybd_event(VK_VOLUME_MUTE, 0, 0, 0);
				//	keybd_event(VK_VOLUME_MUTE, 0, 0x0002, 0);
				//	keybd_event(VK_VOLUME_MUTE, 0, 0, 0);
				//	keybd_event(VK_VOLUME_MUTE, 0, 0x0002, 0);
				//}


				// QUERY_USER_NOTIFICATION_STATE enumeration (shellapi.h)
				// https://docs.microsoft.com/en-us/windows/win32/api/shellapi/ne-shellapi-query_user_notification_state
				private enum QUERY_USER_NOTIFICATION_STATE
				{
					QUNS_NOT_PRESENT = 1,
					QUNS_BUSY = 2,
					QUNS_RUNNING_D3D_FULL_SCREEN = 3,
					QUNS_PRESENTATION_MODE = 4,
					QUNS_ACCEPTS_NOTIFICATIONS = 5,
					QUNS_QUIET_TIME = 6,
					QUNS_APP = 7
				}

				// SHQueryUserNotificationState function (shellapi.h)
				// https://docs.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-shqueryusernotificationstate
				[DllImport("shell32.dll")]
				private static extern int SHQueryUserNotificationState([Out] out QUERY_USER_NOTIFICATION_STATE pquns);


				public static async Task Test()
				{
					await Task.Delay(2000);

					_ = SHQueryUserNotificationState(out QUERY_USER_NOTIFICATION_STATE result);
					Debug.WriteLine(result.ToString());
				}
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
				// https://docs.microsoft.com/en-us/dotnet/api/system.windows.interop.hwndsourcehook?view=windowsdesktop-6.0
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

						handled = SoundDevice.SwitchDefaultAudioDevice.SwitchDevice();
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
		}

#if DEBUG
		public static async void Test()
		{
			Debug.WriteLine("test {");

			//SwitchDefaultAudioDevice.SwitchDevice();

			//VolumeControl.SystemVolumeOSD.Show();

			await VolumeControl.SystemVolumeOSD.Test();

			Debug.WriteLine("} test");
		}
#endif
	}
}
