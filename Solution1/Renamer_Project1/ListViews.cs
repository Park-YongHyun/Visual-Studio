using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Renamer_Project1
{
	public class ListViewItemType1
	{
		public string Path { get; set; }
		public string Directory { get; set; }
		public string Filename { get; set; }
		public string Extension { get; set; }
	}
	public class ListViewType1
	{
		public ListViewType1(ListView listView)
		{
			this.listView = listView;
		}

		public ObservableCollection<ListViewItemType1> items = new ObservableCollection<ListViewItemType1>();
		public ListView listView;
	}

	public class ListViewItemType2
	{
		public string Path { get; set; }
		public string Directory { get; set; }
		public string Filename { get; set; }
		public string Extension { get; set; }
		public string TempName { get; set; }
		public bool Selected { get; set; }
	}
	public class ListViewType2
	{
		public ListViewType2(ListView listView)
		{
			this.listView = listView;
		}

		public ObservableCollection<ListViewItemType2> items = new ObservableCollection<ListViewItemType2>();
		public ListView listView;
	}
}
