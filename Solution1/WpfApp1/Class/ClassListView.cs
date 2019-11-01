using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfApp1
{
	public class ListViewItem
	{
		public string Path { get; set; }
		public string Directory { get; set; }
		public string Filename { get; set; }
		public string Extension { get; set; }
	}

	public class ClassListView
	{
		public ClassListView() { }

		public void SetListView(ListView listView)
		{
			this.listView = listView;
		}

		public ObservableCollection<ListViewItem> items = new ObservableCollection<ListViewItem>();
		public ListView listView;

		private bool itemDrag = false;
		private int itemIndex1 = -1;
		private int itemIndex2 = -1;

		public void ColumnAutoWidth() // 칼럼 넓이 맞춤
		{
			foreach (GridViewColumn column in (listView.View as GridView).Columns)
			{
				column.Width = column.ActualWidth;
				column.Width = Double.NaN;
			}
		}

		public void Drop(DragEventArgs e) // 드롭 파일을 리스트 추가
		{
			string[] dropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
			Array.Sort(dropFiles);
			foreach(string str in dropFiles)
			{
				int index1 = str.LastIndexOf('\\') + 1;
				int index2 = str.LastIndexOf('.');
				if (index2 != 0)
				{
					items.Add(new ListViewItem()
					{
						Path = str,
						Directory = str.Substring(0, index1),
						Filename = str.Substring(index1, index2 - index1),
						Extension = str.Substring(index2)
					});
					ColumnAutoWidth();
				}
			}
		}

		public void ItemRemove() // 선택 삭제
		{
			int index = listView.SelectedIndex;
			if (index != -1)
			{
				while (index != -1)
				{
					items.RemoveAt(index);
					index = listView.SelectedIndex;
				}
			}
		}

		public void ItemClear() // 모두 삭제
		{
			items.Clear();
			ColumnAutoWidth();
		}

		public void DragStart() // 드래그 시작
		{
			itemDrag = true;
			itemIndex1 = GetItemIndex();
		}
		public void DragStop() // 드래그 중지
		{
			itemDrag = false;
		}
		public void DragMouseMove() // 아이템 드래그 이동
		{
			if (itemDrag && itemIndex1 != -1)
			{
				itemIndex2 = GetItemIndex();
				while (itemIndex1 != itemIndex2 && itemIndex2 != -1)
				{
					if (itemIndex1 < itemIndex2)
					{
						int tmp = itemIndex1 + 1;
						items.Move(itemIndex1, tmp);
						itemIndex1++;
					}
					else if (itemIndex1 > itemIndex2)
					{
						int tmp = itemIndex1 - 1;
						items.Move(itemIndex1, tmp);
						itemIndex1--;
					}
				}
			}
		}
		public int GetItemIndex() // 마우스 아래 아이템 인덱스 가져오기
		{
			try // null 예외 처리
			{
				DependencyObject item = VisualTreeHelper.HitTest(listView, Mouse.GetPosition(listView)).VisualHit;
				while (!(item is System.Windows.Controls.ListViewItem))
				{
					item = VisualTreeHelper.GetParent(item);
				}
				int itemIndex = listView.Items.IndexOf(((System.Windows.Controls.ListViewItem)item).DataContext);
				return itemIndex;
			}
			catch
			{
				return -1;
			}
		}
	}
}
