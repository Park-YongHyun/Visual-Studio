using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TreeViewFileExplorer
{
	internal class Win32Api
	{
		public class FileSystemIcon
		{
			// SHGetFileInfoW function (shellapi.h)
			// https://docs.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-shgetfileinfow
			[DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
			private static extern IntPtr SHGetFileInfoW([In] string pszPath, uint dwFileAttributes, [In, Out] ref SHFILEINFOW psfi, uint cbFileInfo, uint uFlags);

			// DestroyIcon function (winuser.h)
			// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-destroyicon
			[DllImport("User32.dll")]
			private static extern bool DestroyIcon([In] IntPtr hIcon);

			// SHFILEINFOW structure (shellapi.h)
			// https://docs.microsoft.com/en-us/windows/win32/api/shellapi/ns-shellapi-shfileinfow
			[StructLayout(LayoutKind.Sequential)]
			private struct SHFILEINFOW
			{
				public IntPtr hIcon;
				public int iIcon;
				public uint dwAttributes;
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
				public string szDisplayName;
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
				public string szTypeName;
			}

			// SHGetFileInfoW
			private const uint SHGFI_LARGEICON = 0x000000000; // 32x32
			private const uint SHGFI_SMALLICON = 0x000000001; // 16x16
			private const uint SHGFI_ICON = 0x000000100;

			public static Icon GetIcon(string path, bool largeIcon = false)
			{
				Icon icon = null;
				SHFILEINFOW shfileinfo = new();

				SHGetFileInfoW(path, 0, ref shfileinfo, (uint)Marshal.SizeOf(shfileinfo), (largeIcon ? SHGFI_LARGEICON : SHGFI_SMALLICON) | SHGFI_ICON);

				if (shfileinfo.hIcon != IntPtr.Zero)
				{
					icon = (Icon)Icon.FromHandle(shfileinfo.hIcon).Clone();

					DestroyIcon(shfileinfo.hIcon);
				}

				return icon;
			}
		}
	}
}
