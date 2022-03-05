using TreeViewFileExplorer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Interop;
using System.Security.Cryptography;

namespace TreeViewFileExplorer
{
	public class FileSystemEntryNode : ViewModelBase
	{
		public FileSystemEntryNode(string name, Icon icon = null, bool isExpanded = false)
		{
			Name = name;
			IconBitmapSource = SetIconBitmapSource(icon);
			IsExpanded = isExpanded;
		}

		// model
		private string _name;

		// view
		private BitmapSource _iconBitmapSource;
		private bool _isExpanded;
		private Visibility _connectingLineVisibility = Visibility.Visible;

		public ObservableCollection<FileSystemEntryNode> Children { get; set; } = new();

		private static readonly Dictionary<string, BitmapSource> iconBitmapSources = new();
		private static readonly ImageConverter imageConverter = new();
		private static readonly SHA1 hashSHA1 = SHA1.Create();

		public string Name
		{
			get => _name;
			set => SetProperty(ref _name, value);
		}

		/*	아이콘 이미지 소스 중복 방지
		 *		딕셔너리<문자열, 비트맵 소스>에 보관
		 *			키: 아이콘 --> 비트맵 --> 바이트 배열 --> SHA1 해쉬(바이트 배열) --> 헥스 문자열
		 */
		public BitmapSource IconBitmapSource
		{
			get => _iconBitmapSource;
			private set => SetProperty(ref _iconBitmapSource, value);
		}
		public BitmapSource SetIconBitmapSource(Icon icon)
		{
			if (icon == null) return null;

			string hexString = Convert.ToHexString(hashSHA1.ComputeHash((byte[])imageConverter.ConvertTo(icon.ToBitmap(), typeof(byte[]))!));

			if (!iconBitmapSources.ContainsKey(hexString))
			{
				iconBitmapSources.Add(hexString, Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));

				Debug.WriteLine($"icon count: {iconBitmapSources.Count}");
			}

			return iconBitmapSources[hexString];
		}

		public Visibility ConnectingLineVisibility
		{
			get => _connectingLineVisibility;
			set => SetProperty(ref _connectingLineVisibility, value);
		}

		public bool IsExpanded
		{
			get => _isExpanded;
			set => SetProperty(ref _isExpanded, value);
		}

		/*	모든 펼쳐진 노드의 하위 노드 연결선 갱신
		 *	
		 *	함수(노드 콜렉션)
		 *	{
		 *		foreach 노드 콜렉션
		 *		{
		 *			if 노드가 끝이 아님
		 *			{
		 *				연결선 보이기
		 *			}
		 *			else
		 *			{
		 *				연결선 숨기기
		 *			}
		 *			
		 *			if 노드가 확장됨
		 *			{
		 *				(재귀) 함수(하위 노드 콜렉션)
		 *			}
		 *		}
		 *	}
		 */
		public void RefreshAllConnectingLines()
		{
			RefreshAllConnectingLines(Children);
		}
		public static void RefreshAllConnectingLines(ObservableCollection<FileSystemEntryNode> fileSystemEntryNodes)
		{
			int lastIndex = fileSystemEntryNodes.Count - 1;
			for (int i = 0; i < fileSystemEntryNodes.Count; i++)
			{
				if (i < lastIndex)
				{
					if (fileSystemEntryNodes[i].ConnectingLineVisibility != Visibility.Visible)
						fileSystemEntryNodes[i].ConnectingLineVisibility = Visibility.Visible;
				}
				else
				{
					if (fileSystemEntryNodes[i].ConnectingLineVisibility != Visibility.Collapsed)
						fileSystemEntryNodes[i].ConnectingLineVisibility = Visibility.Collapsed;
				}

				if (fileSystemEntryNodes[i].IsExpanded)
					fileSystemEntryNodes[i].RefreshAllConnectingLines();
			}
		}
	}
}
