using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Renamer_Project1
{
	public partial class MainWindow
	{
		private bool itemDrag = false;
		private bool itemDragged = false;
		private dynamic activeListView;
		private readonly List<int> indexesList = new List<int>();
		private int itemIndex1 = -1;
		private int itemIndex2 = -1;

		private void DragStart(dynamic classListView, MouseButtonEventArgs e) // 드래그 시작
		{
			activeListView = classListView;
			itemDragged = false;
			itemIndex1 = GetItemIndex();
			if (itemIndex1 != -1 && !(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.LeftCtrl)))
			{
				e.Handled = true;
				//itemDrag = true;
				indexesList.Clear(); // 인덱스 리스트 모든 요소 제거
				for (int i = 0; i < classListView.items.Count; i++) // 선택된 아이템 인덱스 리스트
				{
					if (classListView.items[i].Selected) indexesList.Add(i);
				}
				if (indexesList.Count > 1 && indexesList[0] <= itemIndex1 && indexesList[indexesList.Count - 1] >= itemIndex1)
				{
					itemDrag = true;
				}
				else
				{
					indexesList.Clear(); // 인덱스 리스트 모든 요소 제거
					indexesList.Add(itemIndex1);
					itemDrag = true;
				}
			}
		}
		private void DragStop() // 드래그 중지
		{
			itemDrag = false;
			if (itemDragged) // 드래그 후
			{
				foreach (int index in indexesList) // 이동한 아이템 선택
				{
					activeListView.items[index].Selected = true;
				}
				activeListView.listView.Items.Refresh(); // 새로고침
			}
			else if (!(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.LeftCtrl))) // 드래그 안함
			{
				activeListView.listView.SelectedItem = null; // 모두 선택 해제
				if (itemIndex1 != -1) activeListView.items[itemIndex1].Selected = true;
				activeListView.listView.Items.Refresh(); // 새로고침
			}
		}
		//private void DragMouseMove(dynamic classListView) // 1개 아이템 드래그 이동
		//{
		//	if (itemDrag && itemIndex1 != -1)
		//	{
		//		itemIndex2 = GetItemIndex(classListView);
		//		while (itemIndex1 != itemIndex2 && itemIndex2 != -1)
		//		{
		//			if (itemIndex1 < itemIndex2)
		//			{
		//				classListView.items.Move(itemIndex1, itemIndex1 + 1);
		//				itemIndex1++;
		//			}
		//			else if (itemIndex1 > itemIndex2)
		//			{
		//				classListView.items.Move(itemIndex1, itemIndex1 - 1);
		//				itemIndex1--;
		//			}
		//		}
		//	}
		//}
		private void DragMouseMove() // 1개 이상 아이템 드래그 이동
		{
			if (itemDrag && itemIndex1 != -1)
			{
				itemDragged = true;
				int itemsCount = activeListView.items.Count;
				itemIndex2 = GetItemIndex();
				bool moveLimit = false;
				while (itemIndex1 != itemIndex2 && itemIndex2 != -1) // 이동
				{
					if (itemIndex1 < itemIndex2) // 아래로 이동
					{
						for (int i = indexesList.Count - 1; i > -1; i--)
						{
							int index = indexesList[i];
							if (index == itemsCount - 1)
							{
								moveLimit = true;
								break;
							}
							indexesList[i]++;
							activeListView.items.Move(index, index + 1);
						}
						if (!moveLimit) itemIndex1++;
					}
					else if (itemIndex1 > itemIndex2) // 위로 이동
					{
						for (int i = 0; i < indexesList.Count; i++)
						{
							int index = indexesList[i];
							if (index == 0)
							{
								moveLimit = true;
								break;
							}
							indexesList[i]--;
							activeListView.items.Move(index, index - 1);
						}
						if (!moveLimit) itemIndex1--;
					}
					if (moveLimit) break;
				}
				activeListView.listView.SelectedItem = null; // 모두 선택 해제

				//draggedListView.listView.Items.Refresh(); mouseup 이벤트 생략 발생! // 새로고침
			}
		}
		private int GetItemIndex() // 마우스 아래 아이템 인덱스 가져오기
		{
			try // null 예외 처리
			{
				DependencyObject item = VisualTreeHelper.HitTest(activeListView.listView, Mouse.GetPosition(activeListView.listView)).VisualHit;
				while (!(item is ListViewItem))
				{
					item = VisualTreeHelper.GetParent(item);
				}
				int itemIndex = activeListView.listView.Items.IndexOf(((ListViewItem)item).DataContext);
				return itemIndex;
			}
			catch
			{
				return -1;
			}
		}

		private dynamic GetListView(object sender) // 리스트뷰 찾기
		{
			string tmp1;
			string tmp2;
			try
			{
				tmp1 = ((FrameworkElement)sender).Name;
				tmp2 = tmp1.Substring(tmp1.Length - 1);
			}
			catch // 컨텍스트 매뉴 예외
			{
				tmp1 = ((FrameworkElement)((ContextMenu)((FrameworkElement)sender).Parent).PlacementTarget).Name;
				tmp2 = tmp1.Substring(tmp1.Length - 1);
			}
			return listViews[tmp2];
		}

		private string GetTabItem() // 활성 탭 찾기
		{
			string tmp1 = ((TabItem)tabControl.SelectedItem).Name;
			string tmp2 = tmp1.Substring(tmp1.Length - 1);
			return tmp2;
		}

		private void Rename(ListViewType1 classListView1, ListViewType1 classListView2, string str) // Rename 1
		{
			if (classListView1.listView.Items.Count == classListView2.listView.Items.Count)
			{
				string error = "";
				for (int i = 0; i < classListView1.listView.Items.Count; i++)
				{
#pragma warning disable IDE0059 // 불필요한 값 할당 warning disable
					string path1 = classListView1.items[i].Path;
					string path2 = classListView2.items[i].Path;
					string dir1 = classListView1.items[i].Directory;
					string dir2 = classListView2.items[i].Directory;
					string name1 = classListView1.items[i].Filename;
					string name2 = classListView2.items[i].Filename;
					string ext1 = classListView1.items[i].Extension;
					string ext2 = classListView2.items[i].Extension;
#pragma warning restore IDE0059
					try
					{
						switch (str)
						{
							case "run":
								File.Move(path2, dir2 + name1 + ext2);
								break;
							case "restore":
								File.Move(dir2 + name1 + ext2, path2);
								break;
						}
					}
					catch (Exception exc)
					{
						error += exc.ToString() + "\n";
					}
				}
				if (error != "")
				{
					ErrorMessage(error);
				}
			}
		}
		private void Rename(ListViewType2 classListView, string str) // Rename 2
		{
			string error = "";
			string fileList = "";
			string workDir = "";
			for (int i = 0; i < classListView.listView.Items.Count; i++)
			{
				string path = classListView.items[i].Path;
				string dir = classListView.items[i].Directory;
				string name = classListView.items[i].Filename;
				string ext = classListView.items[i].Extension;
				string tempName = classListView.items[i].TempName;
				try
				{
					switch (str)
					{
						case "run":
							fileList += string.Format("{0}\t{1}\t{2}\t{3}\n", tempName, dir, name, ext);
							if (i == 0)
							{
								workDir = dir;
								textBox1.Text = workDir + "TempName.txt";
							}
							File.Move(path, dir + tempName + ext);
							break;
						case "restore":
							File.Move(dir + tempName + ext, path);
							break;
					}
				}
				catch (Exception exc)
				{
					error += exc.ToString() + "\n";
				}
			}
			if (fileList != "")
			{
				File.WriteAllText(workDir + "TempName.txt", fileList);
			}
			if (error != "")
			{
				ErrorMessage(error);
			}
		}
		private void ErrorMessage(string error) // Rename 에러 메시지
		{
			MessageBox.Show(error, "error message");
			MessageBox.Show(AppDomain.CurrentDomain.BaseDirectory + "error_message.txt", "error_message.txt");
			File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "error_message.txt", error);
		}

		private void SetTempName(ListViewType2 classListView) // TempName
		{
			foreach (ListViewType2Item item in classListView.items)
			{
				if (item.Selected)
				{
					item.TempName = TempName();
				}
			}
			classListView.listView.Items.Refresh(); // 새로고침
		}
		private string TempName()
		{
			string tmp1 = "";
			if (textBox4.Text != string.Empty) tmp1 += textBox4.Text;
			tmp1 += comboBox1.Text;
			if ((bool)checkBox1.IsChecked)
			{
				tmp1 += textBox2.Text.PadLeft(Convert.ToInt32(textBox3.Text), '0');
				textBox2.Text = (Convert.ToInt32(textBox2.Text) + 1).ToString();
			}
			return tmp1;
		}

		private void ColumnAutoWidth(ListView listView) // 칼럼 넓이 맞춤
		{
			foreach (GridViewColumn column in (listView.View as GridView).Columns)
			{
				column.Width = column.ActualWidth;
				column.Width = Double.NaN;
			}
		}

		private void DropFiles(DragEventArgs e, ListViewType1 classListView) // 드롭 파일 추가 (타입1)
		{
			string[] dropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
			Array.Sort(dropFiles);
			foreach (string dropFile in dropFiles)
			{
				int index1 = dropFile.LastIndexOf('\\') + 1;
				int index2 = dropFile.LastIndexOf('.');
				if (index2 > 0)
				{
					classListView.items.Add(new ListViewType1Item()
					{
						Path = dropFile,
						Directory = dropFile.Substring(0, index1),
						Filename = dropFile.Substring(index1, index2 - index1),
						Extension = dropFile.Substring(index2)
					});
					ColumnAutoWidth(classListView.listView);
				}
			}
		}
		private void DropFiles(DragEventArgs e, ListViewType2 classListView) // 드롭 파일 추가 (타입2)
		{
			string[] dropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
			Array.Sort(dropFiles);
			foreach (string dropFile in dropFiles)
			{
				int index1 = dropFile.LastIndexOf('\\') + 1;
				int index2 = dropFile.LastIndexOf('.');
				if (dropFile.Substring(index1) == "TempName.txt")
				{
					classListView.items.Clear();
					foreach (string line in File.ReadAllLines(dropFile))
					{
						string[] tmp = line.Split('\t');
						classListView.items.Add(new ListViewType2Item()
						{
							Path = tmp[1] + tmp[2] + tmp[3],
							Directory = tmp[1],
							Filename = tmp[2],
							Extension = tmp[3],
							TempName = tmp[0]
						});
					}
					ColumnAutoWidth(classListView.listView);
					break;
				}
				if (index2 > 0)
				{
					classListView.items.Add(new ListViewType2Item()
					{
						Path = dropFile,
						Directory = dropFile.Substring(0, index1),
						Filename = dropFile.Substring(index1, index2 - index1),
						Extension = dropFile.Substring(index2),
						TempName = TempName()
					});
					ColumnAutoWidth(classListView.listView);
				}
			}
		}

		private void ItemRemove(dynamic classListView) // 선택 삭제
		{
			int index = classListView.listView.SelectedIndex;
			if (index != -1)
			{
				while (index != -1)
				{
					classListView.items.RemoveAt(index);
					index = classListView.listView.SelectedIndex;
				}
			}
		}

		private void ItemClear(dynamic classListView) // 모두 삭제
		{
			classListView.items.Clear();
			ColumnAutoWidth(classListView.listView);
		}

		private void MouseWheelNumber(object sender, MouseWheelEventArgs e) // 마우스휠 숫자 증감
		{
			int number = Convert.ToInt32(((TextBox)sender).Text);
			if (e.Delta > 0)
			{
				((TextBox)sender).Text = (number + 1).ToString();
			}
			else if (number > 0 && e.Delta < 0)
			{
				((TextBox)sender).Text = (number - 1).ToString();
			}
		}

		private void ComboBox1SelectionChanged() // 선택 변경시 초기화
		{
			try
			{
				textBox2.Text = "1";
			}
			catch // 시작시 에러
			{

			}
		}
	}
}
