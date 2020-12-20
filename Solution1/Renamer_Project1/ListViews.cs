using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Renamer_Project1
{
	public abstract class ClsListViewItem
	{
		public string Path { get; set; }
		public string Directory { get; set; }
		public string FileName { get; set; }
		public string Extension { get; set; }
		public bool Selected { get; set; }
	}
	public class ClsListViewItemTypeA : ClsListViewItem
	{ }
	public class ClsListViewItemTypeB : ClsListViewItem
	{
		public string TempName { get; set; }
	}

	public abstract class ClsListView
	{
		public ClsListView(ListView listView)
		{
			this.listView = listView;
		}
		public ListView listView;
	}
	public class ClsListViewTypeA : ClsListView
	{
		public ClsListViewTypeA(ListView listView) : base(listView)
		{ }
		public ObservableCollection<ClsListViewItemTypeA> items = new ObservableCollection<ClsListViewItemTypeA>();
	}
	public class ClsListViewTypeB : ClsListView
	{
		public ClsListViewTypeB(ListView listView) : base(listView)
		{ }
		public ObservableCollection<ClsListViewItemTypeB> items = new ObservableCollection<ClsListViewItemTypeB>();
	}
}
