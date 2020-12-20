using System.Windows;
using System.Windows.Input;

namespace Renamer_Project1
{
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			listViewTypeA1 = new ClsListViewTypeA(listView1);
			listViewTypeA2 = new ClsListViewTypeA(listView2);
			listViewTypeB = new ClsListViewTypeB(listView3);

			listView1.ItemsSource = listViewTypeA1.items; // 바인딩
			listView2.ItemsSource = listViewTypeA2.items;
			listView3.ItemsSource = listViewTypeB.items;
		}
		private readonly ClsListViewTypeA listViewTypeA1;
		private readonly ClsListViewTypeA listViewTypeA2;
		private readonly ClsListViewTypeB listViewTypeB;

		private void ListView_DragEnter(object sender, DragEventArgs e) // 파일 드롭시 마우스 커서 변경
		{
			e.Effects = DragDropEffects.Copy;
		}

		private void ListView_Drop(object sender, DragEventArgs e) // 드롭 이벤트
		{
			DropFiles(e, GetListView(sender));
		}

		private void ListViewMenuItemRemove_Click(object sender, RoutedEventArgs e) // 삭제 클릭
		{
			ItemRemove(GetListView(sender));
		}

		private void ListViewMenuItemClear_Click(object sender, RoutedEventArgs e) // 클리어 클릭
		{
			ItemClear(GetListView(sender));
		}
		private void MainMenuItemClear_Click(object sender, RoutedEventArgs e) // 클리어 클릭
		{
			switch (tabControl.SelectedIndex)
			{
				case 0:
					ItemClear(listViewTypeA1);
					ItemClear(listViewTypeA2);
					break;
				case 1:
					ItemClear(listViewTypeB);
					break;
			}
		}

		private void MainMenuItemRun_Click(object sender, RoutedEventArgs e) // Rename 실행
		{
			switch (tabControl.SelectedIndex)
			{
				case 0:
					Rename(listViewTypeA1, listViewTypeA2, "run");
					break;
				case 1:
					Rename(listViewTypeB, "run");
					break;
			}
		}

		private void MainMenuItemRestore_Click(object sender, RoutedEventArgs e) // Rename 복구
		{
			switch (tabControl.SelectedIndex)
			{
				case 0:
					Rename(listViewTypeA1, listViewTypeA2, "restore");
					break;
				case 1:
					Rename(listViewTypeB, "restore");
					break;
			}
		}

		private void ListViewMenuItemTempName_Click(object sender, RoutedEventArgs e) // TempName 실행
		{
			SetTempName(listViewTypeB);
		}

		private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) // 마우스 좌측 버튼
		{
			DragStart(GetListView(sender), e); // 드래그 시작
		}

		private void ListView_PreviewMouseMove(object sender, MouseEventArgs e) // 마우스 이동
		{
			DragMouseMove(); // 드래그 동작
		}

		private void ListView_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) // 마우스 좌측 버튼
		{
			DragStop(); // 드래그 중지
		}

		private void TextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e) // 마우스휠
		{
			MouseWheelNumber(sender, e);
		}

		private void ComboBox1_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) // 선택 변경
		{
			ComboBox1SelectionChanged();
		}
	}
}
