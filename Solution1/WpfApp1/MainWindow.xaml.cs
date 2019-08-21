using System.Windows;
using System.Windows.Input;

namespace WpfApp1
{
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window
	{
		//private ObservableCollection<ClassListViewItem> listView1Items = new ObservableCollection<ClassListViewItem>();
		//private ObservableCollection<ClassListViewItem> listView2Items = new ObservableCollection<ClassListViewItem>();
		private ClassListView classListView1 = new ClassListView();
		private ClassListView classListView2 = new ClassListView();
		
		public MainWindow()
		{
			InitializeComponent();
			listView1.ItemsSource = classListView1.items; // 바인딩
			listView2.ItemsSource = classListView2.items;
			classListView1.SetListView(listView1);
			classListView2.SetListView(listView2);
		}
		
		private void ListView_DragEnter(object sender, DragEventArgs e) // 파일 드롭시 마우스 커서 변경
		{
			e.Effects = DragDropEffects.Copy;
		}
		
		private void ListView1_Drop(object sender, DragEventArgs e) // 드롭 이벤트
		{
			classListView1.Drop(e);
		}
		private void ListView2_Drop(object sender, DragEventArgs e)
		{
			classListView2.Drop(e);
		}
		
		private void ListView1MenuItemRemove_Click(object sender, RoutedEventArgs e) // 삭제 클릭
		{
			classListView1.ItemRemove();
		}
		private void ListView2MenuItemRemove_Click(object sender, RoutedEventArgs e)
		{
			classListView2.ItemRemove();
		}

		private void ListView1MenuItemClear_Click(object sender, RoutedEventArgs e) // 클리어 클릭
		{
			classListView1.ItemClear();
		}
		private void ListView2MenuItemClear_Click(object sender, RoutedEventArgs e)
		{
			classListView2.ItemClear();
		}
		private void MainMenuItemClear_Click(object sender, RoutedEventArgs e)
		{
			classListView1.ItemClear();
			classListView2.ItemClear();
		}

		private void MainMenuItemRun_Click(object sender, RoutedEventArgs e) // 리네임 실행
		{
			ClassMethodGroup.Rename(classListView1, classListView2, "run");
		}

		private void MainMenuItemRestore_Click(object sender, RoutedEventArgs e) // 리네임 복구
		{
			ClassMethodGroup.Rename(classListView1, classListView2, "restore");
		}

		private void ListView1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) // 드래그 시작
		{
			classListView1.DragStart();
		}
		private void ListView2_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			classListView2.DragStart();
		}

		private void ListView1_PreviewMouseMove(object sender, MouseEventArgs e) // 드래그 동작
		{
			classListView1.DragMouseMove();
		}
		private void ListView2_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			classListView2.DragMouseMove();
		}

		private void ListView1_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) // 드래그 중지
		{
			classListView1.DragStop();
		}
		private void ListView2_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			classListView2.DragStop();
		}
	}
}
