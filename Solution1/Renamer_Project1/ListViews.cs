using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Renamer_Project1
{
	public class ListViewType1Item
	{
		public string Path { get; set; }
		public string Directory { get; set; }
		public string Filename { get; set; }
		public string Extension { get; set; }
		public bool Selected { get; set; }
	}
	public class ListViewType1
	{
		public ListViewType1(ListView listView)
		{
			this.listView = listView;
		}

		public ObservableCollection<ListViewType1Item> items = new ObservableCollection<ListViewType1Item>();
		public ListView listView;
	}

	public class ListViewType2Item
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

		public ObservableCollection<ListViewType2Item> items = new ObservableCollection<ListViewType2Item>();
		public ListView listView;
	}
}
