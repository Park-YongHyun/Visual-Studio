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
	internal class Win32Api
	{
		// Virtual-Key Codes
		// https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
		public const int VK_VOLUME_MUTE = 0xAD; // Volume Mute key
		public const int VK_VOLUME_DOWN = 0xAE; // Volume Down key
		public const int VK_VOLUME_UP = 0xAF; // Volume Up key


		public class WindowsHook
		{

			public const int WH_KEYBOARD_LL = 13;

			public const int WM_KEYDOWN = 0x0100;
			public const int WM_KEYUP = 0x0101;

			// KBDLLHOOKSTRUCT structure (winuser.h)
			// https://docs.microsoft.com/ko-kr/windows/win32/api/winuser/ns-winuser-kbdllhookstruct?redirectedfrom=MSDN
			[StructLayout(LayoutKind.Sequential)]
			public struct KBDLLHOOKSTRUCT
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
			public static extern IntPtr SetWindowsHookExW([In] int idHook, [In] LowLevelKeyboardProc lpfn, [In] IntPtr hmod, [In] uint dwThreadId);

			// UnhookWindowsHookEx function (winuser.h)
			// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-unhookwindowshookex
			[DllImport("user32.dll", SetLastError = true)]
			public static extern bool UnhookWindowsHookEx([In] IntPtr hhk);

			// CallNextHookEx function (winuser.h)
			// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-callnexthookex
			[DllImport("user32.dll")]
			public static extern IntPtr CallNextHookEx([In, Optional] IntPtr hhk, [In] int nCode, [In] IntPtr wParam, [In] IntPtr lParam);

			// SetWindowsHookExW(default)
			// HOOKPROC callback function (winuser.h)
			// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nc-winuser-hookproc
			//public delegate IntPtr Hookproc(int code, [In] IntPtr wParam, [In] IntPtr lParam);

			// SetWindowsHookExW(WH_KEYBOARD_LL)
			// LowLevelKeyboardProc callback function
			// https://docs.microsoft.com/en-us/previous-versions/windows/desktop/legacy/ms644985(v=vs.85)
			public delegate IntPtr LowLevelKeyboardProc(int nCode, [In] IntPtr wParam, [In] IntPtr lParam);
		}


		public class RegHotKey
		{
			public const int WM_HOTKEY = 0x0312;

			public const int HOTKEY_ID = 9000;

			// RegisterHotKey function (winuser.h)
			// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey
			[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			public static extern bool RegisterHotKey([In, Optional] IntPtr hWnd, [In] int id, [In] uint fsModifiers, [In] uint vk);

			// UnregisterHotKey function (winuser.h)
			// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-unregisterhotkey
			[DllImport("user32.dll", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool UnregisterHotKey([In, Optional] IntPtr hWnd, [In] int id);
		}


		public class WinMessage
		{
			public const int WM_APP = 0x8000;

			// HwndSourceHook Delegate
			// https://docs.microsoft.com/en-us/dotnet/api/system.windows.interop.hwndsourcehook?view=windowsdesktop-6.0
		}


		public class SystemVolumeOSD
		{
			public const int INPUT_KEYBOARD = 1;

			public const int KEYEVENTF_KEYDOWN = 0x0000;
			public const int KEYEVENTF_KEYUP = 0x0002;

			public const int SW_HIDE = 0;

			// INPUT structure (winuser.h)
			// https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-input
			[StructLayout(LayoutKind.Sequential)]
			public struct INPUT
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
			public static extern uint SendInput([In] uint cInputs, [In] INPUT[] pInputs, [In] int cbSize);

			// FindWindowExW function (winuser.h)
			// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-findwindowexw
			[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			public static extern IntPtr FindWindowExW([In, Optional] IntPtr hWndParent, [In, Optional] IntPtr hWndChildAfter, [In, Optional] string lpszClass, [In, Optional] string lpszWindow);

			// ShowWindow function (winuser.h)
			// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow
			[DllImport("user32.dll")]
			public static extern bool ShowWindow([In] IntPtr hWnd, [In] int nCmdShow);


			//[DllImport("user32.dll")]
			//public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
		}


		public class PopupControl
		{
			// QUERY_USER_NOTIFICATION_STATE enumeration (shellapi.h)
			// https://docs.microsoft.com/en-us/windows/win32/api/shellapi/ne-shellapi-query_user_notification_state
			public enum QUERY_USER_NOTIFICATION_STATE
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
			public static extern int SHQueryUserNotificationState([Out] out QUERY_USER_NOTIFICATION_STATE pquns);
		}
	}
}
