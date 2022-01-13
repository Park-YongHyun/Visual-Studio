using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ServerWatcher
{
	class Win32Api
	{
		// FlashWindowEx function (winuser.h)
		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-flashwindowex
		[DllImport("user32.dll")]
		public static extern Int32 FlashWindowEx(in FLASHWINFO pwfi);

		// FLASHWINFO structure (winuser.h)
		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-flashwinfo
		[StructLayout(LayoutKind.Sequential)]
		public struct FLASHWINFO
		{
			public UInt32 cbSize;
			public IntPtr hwnd;
			/// <summary>0: stop flag<br/>1: 창 캡션 깜빡임<br/>2: 트레이 깜빡임<br/>4: stop flag에 중지<br/>8: 창 활성시 중지</summary>
			public Int32 dwFlags;
			public UInt32 uCount;
			public Int32 dwTimeout;
		}

		public static void FlashWindow(IntPtr handle)
		{
			FLASHWINFO pfwi = new();

			pfwi.cbSize = Convert.ToUInt32(Marshal.SizeOf(typeof(FLASHWINFO)));
			pfwi.hwnd = handle;
			pfwi.dwFlags = 0b1111;
			pfwi.uCount = UInt32.MaxValue;
			pfwi.dwTimeout = 0;

			FlashWindowEx(in pfwi);
		}
	}
}
