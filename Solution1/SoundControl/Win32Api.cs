using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace SoundControl
{
	class Win32Api
	{
		// Virtual-Key Codes
		// https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
		private const int VK_VOLUME_MUTE = 0xAD;    // Volume Mute key
		private const int VK_VOLUME_DOWN = 0xAE;    // Volume Down key
		private const int VK_VOLUME_UP = 0xAF;      // Volume Up key

		private static readonly MMDeviceEnumerator mMDeviceEnumerator = new();

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

			// SetWindowsHookExW(default)
			// HOOKPROC callback function (winuser.h)
			// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nc-winuser-hookproc
			//private delegate IntPtr LowLevelKeyboardProc(int code, [In] IntPtr wParam, [In] IntPtr lParam);

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
				//int virtualKeyCode = Marshal.ReadInt32(lParam);
				KBDLLHOOKSTRUCT keyEventInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

				Debug.WriteLine("");
				Debug.WriteLine("WindowsHook {");
				Debug.WriteLine($"{nameof(code)} = {code}");
				Debug.WriteLine($"{nameof(wParam)}:X = {wParam.ToInt32():X}"); // keydown: 0x100, keyup: 0x101
				Debug.WriteLine($"{nameof(lParam)}.convert:X = {keyEventInfo.vkCode:X}");
				Debug.WriteLine("}");

				if (wParam.ToInt32() == 0x100 && HotKeyHandling((int)keyEventInfo.vkCode)) return (IntPtr)1;

				//if (code >= 0 && wParam == (IntPtr)WM_KEYDOWN)
				//{
				//	int vkCode = Marshal.ReadInt32(lParam);
				//	//if (vkCode.ToString() == "162")
				//	//{

				//	//}
				//	return (IntPtr)1;
				//}
				//else
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
					Debug.WriteLine($"{nameof(wParam)} = {wParam}");
					Debug.WriteLine($"{nameof(lParam)}.convert:X = {virtualKeyCode:X}");
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
				}
			}
		}

		/*	현재 볼륨 레벨 동기화
		 *	볼륨 크기 변경
		 *	시스템 볼륨 OSD or 커스텀 볼륨 팝업 표시 후 빠르게 숨기기
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
			using MMDevice defaultAudioDevice = mMDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console); // DataFlow.All => Exception
			double currentVolumeLevelScalar = Math.Round(defaultAudioDevice.AudioEndpointVolume.MasterVolumeLevelScalar, 5);
			Debug.WriteLine($"{nameof(currentVolumeLevelScalar)} = {currentVolumeLevelScalar}");
			Debug.WriteLine($"{nameof(Config.GetRoot.Volume.Level)} = {Config.GetRoot.Volume.Level.List[currentVolumeStep]}");

			if (Config.GetRoot.Volume.Level.List[currentVolumeStep] != currentVolumeLevelScalar)
			{
				for (int i = 0; i < Config.GetRoot.Volume.Level.List.Length; i++)
				{
					currentVolumeStep = i;
					if (currentVolumeLevelScalar <= Config.GetRoot.Volume.Level.List[i]) break;
				}
				Debug.WriteLine($"{nameof(currentVolumeStep)} = {currentVolumeStep}");
			}

			// 볼륨 조절
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

			OnVolumeChanged((int)Math.Round(Config.GetRoot.Volume.Level.List[currentVolumeStep] * 100, 3));

			return true;
		}


		//public class SetWinPos
		//{
		//	// SetWindowPos function (winuser.h)
		//	// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos
		//	[DllImport("user32.dll")]
		//	private static extern bool SetWindowPos([In] IntPtr hWnd, [In, Optional] IntPtr hWndInsertAfter, [In] int X, [In] int Y, [In] int cx, [In] int cy, [In] uint uFlags);

		//	private static readonly IntPtr HWND_TOPMOST = new(-1);
		//	private const int SWP_NOSIZE = 0x0001;
		//	private const int SWP_NOMOVE = 0x0002;


		//	private static IntPtr windowHandle;

		//	public static void SetWindowPos(System.Windows.Window window)
		//	{
		//		windowHandle = new WindowInteropHelper(window).Handle;

		//		bool result = SetWindowPos(windowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | 0x0040);
		//		Debug.WriteLine($"SetWindowPos = {result}");
		//	}
		//}


		//public static void Test()
		//{

		//}
	}
}
