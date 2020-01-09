using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace WpfApp1
{
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window
	{

		private readonly Dictionary<string, ClassListView> listViews = new Dictionary<string, ClassListView>();

		public MainWindow()
		{
			InitializeComponent();

			listViews.Add("1", new ClassListView(listView1));
			listViews.Add("2", new ClassListView(listView2));
			listViews.Add("3", new ClassListView(listView3));
			listViews.Add("4", new ClassListView(listView4));

			listView1.ItemsSource = listViews["1"].items; // 바인딩
			listView2.ItemsSource = listViews["2"].items;
			listView3.ItemsSource = listViews["3"].items;
			listView4.ItemsSource = listViews["4"].items;
		}
		
		private void ListView_DragEnter(object sender, DragEventArgs e) // 파일 드롭시 마우스 커서 변경
		{
			e.Effects = DragDropEffects.Copy;
		}
		
		private void ListView_Drop(object sender, DragEventArgs e) // 드롭 이벤트
		{
			switch (GetTabItem())
			{
				case "1":
					GetListView(sender).Drop(e);
					break;
				case "2":
					SetTempName(e);
					break;
			}
		}
		
		private void ListViewMenuItemRemove_Click(object sender, RoutedEventArgs e) // 삭제 클릭
		{
			GetListView(sender).ItemRemove();
		}

		private void ListViewMenuItemClear_Click(object sender, RoutedEventArgs e) // 클리어 클릭
		{
			GetListView(sender).ItemClear();
		}
		private void MainMenuItemClear_Click(object sender, RoutedEventArgs e)
		{
			switch (GetTabItem())
			{
				case "1":
					listViews["1"].ItemClear();
					listViews["2"].ItemClear();
					break;
				case "2":
					listViews["3"].ItemClear();
					listViews["4"].ItemClear();
					break;
			}
		}

		private void MainMenuItemRun_Click(object sender, RoutedEventArgs e) // Rename 실행
		{
			switch (GetTabItem())
			{
				case "1":
					Rename(listViews["1"], listViews["2"], "copyname");
					break;
				case "2":
					Rename(listViews["3"], listViews["4"], "tempname");
					break;
			}
		}

		private void MainMenuItemRestore_Click(object sender, RoutedEventArgs e) // Rename 복구
		{
			switch (GetTabItem())
			{
				case "1":
					Rename(listViews["1"], listViews["2"], "restore");
					break;
				case "2":
					Rename(listViews["3"], listViews["4"], "restore");
					break;
			}
		}

		private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) // 드래그 시작
		{
			GetListView(sender).DragStart();
		}

		private void ListView_PreviewMouseMove(object sender, MouseEventArgs e) // 드래그 동작
		{
			GetListView(sender).DragMouseMove();
		}

		private void ListView_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) // 드래그 중지
		{
			GetListView(sender).DragStop();
		}
	}
}
