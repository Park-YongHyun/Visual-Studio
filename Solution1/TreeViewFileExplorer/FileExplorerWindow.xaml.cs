using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TreeViewFileExplorer
{
	/// <summary>
	/// FileExplorerWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class FileExplorerWindow : Window
	{
		public FileExplorerWindow()
		{
			InitializeComponent();
		}

		/*	트리뷰 연결선 배치
		 *	기본 트리뷰 아이템 레이아웃에 연결선 요소를 이동
		 */
		private async void ConnectingLine_Loaded(object sender, RoutedEventArgs e)
		{
			await Task.Delay(1); // 대량의 스크롤 과부하 때 System.InvalidOperationException: '상호 종속 보기의 결과로 무한 루프가 나타납니다.' 예외 발생 방지

			DependencyObject parent = VisualTreeHelper.GetParent(sender as DependencyObject);
			DependencyObject target = VisualTreeHelper.GetParent(parent); // TreeViewItem / Grid / Bd:Border / PART_Header:ContentPresenter

			if (target is TreeViewItem) return; // 이미 이동한 경우

			target = VisualTreeHelper.GetParent(target); // TreeViewItem / Grid / Bd:Border
			target = VisualTreeHelper.GetParent(target); // TreeViewItem / Grid

			// 연결선 요소 이동
			((Panel)parent).Children.Remove(sender as UIElement);
			((Panel)target).Children.Add(sender as UIElement);

			switch (((FrameworkElement)sender).Name)
			{
				case "connectingLine1":
					break;
				case "connectingLine2":
					Grid.SetRow(sender as UIElement, 1);

					// 트리뷰 아이템이 접힐 때 숨겨지는 하위 요소와 바인딩
					Binding binding = new()
					{
						Source = ((Panel)target).Children[2],
						Path = new("Visibility")
					};
					((FrameworkElement)sender).SetBinding(VisibilityProperty, binding);
					break;
				default:
					break;
			}
		}

		private void TreeView1_Expanded(object sender, RoutedEventArgs e)
		{

		}

		private void TreeView1_Collapsed(object sender, RoutedEventArgs e)
		{

		}

		private void TreeView1_Loaded(object sender, RoutedEventArgs e)
		{
#if DEBUG
			((FileExplorerWindowViewModel)DataContext).GetTreeView(@"D:\Temp");
#else
			string[] commnadArgs = Environment.GetCommandLineArgs();
			if (commnadArgs.Length < 2) Close();
			else ((FileExplorerWindowViewModel)DataContext).GetTreeView(commnadArgs[1]);
#endif
		}
	}
}
