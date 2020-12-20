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
		// clsListView.listView.SelectedItem = null; 선택 해제, 화면 내에 표시된 것만 작동, 주의!
		// clsListView.listView.UnselectAll(); 위와 동일

		private bool itemDrag = false;
		private bool itemDragged = false;
		private dynamic activeListView;
		private readonly List<int> dragIndexList = new List<int>();
		private int dragItemIndex1 = -1;
		private int dragItemIndex2 = -1;

		private void DragStart(dynamic clsListView, MouseButtonEventArgs e) // 드래그 시작
		{
			activeListView = clsListView;
			dragItemIndex1 = GetItemIndex();
			if (dragItemIndex1 != -1 && !(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.LeftCtrl)))
			{
				e.Handled = true; // 기존 이벤트 효과 제거
				dragIndexList.Clear(); // 인덱스 리스트 비우기
				for (int i = 0; i < activeListView.items.Count; i++) // 선택된 아이템 인덱스 리스팅
				{
					if (activeListView.items[i].Selected) dragIndexList.Add(i);
				}
				if (!dragIndexList.Contains(dragItemIndex1))
				{
					dragIndexList.Clear();
					dragIndexList.Add(dragItemIndex1);
				}
				itemDrag = true;
			}
		}
		private void DragStop() // 드래그 중지
		{
			if (itemDrag) itemDrag = false;
			if (itemDragged) itemDragged = false;
			else if (!(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.LeftCtrl))) // 드래그 안함
			{
				DeselectAll(activeListView);
				if (dragItemIndex1 != -1) activeListView.items[dragItemIndex1].Selected = true;
				activeListView.listView.Items.Refresh();
			}
		}
		private void DragMouseMove() // 1개 이상 아이템 드래그 이동
		{
			if (itemDrag)
			{
				if (Mouse.LeftButton != MouseButtonState.Pressed) DragStop();
				else if (dragItemIndex1 != -1)
				{
					int itemsCount = activeListView.items.Count;
					dragItemIndex2 = GetItemIndex();
					bool moveLimit = false;
					bool oneTimeTrue = true;
					bool itemMoved = false;
					while (dragItemIndex1 != dragItemIndex2 && dragItemIndex2 != -1) // 이동
					{
						if (oneTimeTrue)
						{
							oneTimeTrue = false;
							itemDragged = true;
							itemMoved = true;
							DeselectAll(activeListView);
						}
						if (dragItemIndex1 < dragItemIndex2) // 아래로 이동
						{
							for (int i = dragIndexList.Count - 1; i > -1; i--)
							{
								int index = dragIndexList[i];
								if (index == itemsCount - 1)
								{
									moveLimit = true;
									break;
								}
								dragIndexList[i]++;
								activeListView.items.Move(index, index + 1);
							}
							if (!moveLimit) dragItemIndex1++;
						}
						else if (dragItemIndex1 > dragItemIndex2) // 위로 이동
						{
							for (int i = 0; i < dragIndexList.Count; i++)
							{
								int index = dragIndexList[i];
								if (index == 0)
								{
									moveLimit = true;
									break;
								}
								dragIndexList[i]--;
								activeListView.items.Move(index, index - 1);
							}
							if (!moveLimit) dragItemIndex1--;
						}
						if (moveLimit) break;
					}
					if (itemMoved) // 이동 후
					{
						foreach (int index in dragIndexList) activeListView.items[index].Selected = true; // 이동한 아이템 선택
						activeListView.listView.Items.Refresh();
					}

					//draggedListView.clsListViewTypeB.Items.Refresh(); mouseup 이벤트 생략 발생! // 새로고침
				}
			}
		}
		private int GetItemIndex() // 마우스 아래 아이템 인덱스 가져오기
		{
			object item = VisualTreeHelper.HitTest(activeListView.listView, Mouse.GetPosition(activeListView.listView));
			if (item == null) return -1;
			item = ((HitTestResult)item).VisualHit;
			while (!(item is ListViewItem)) // 탐색
			{
				item = VisualTreeHelper.GetParent((DependencyObject)item);
				if (item == null) return -1;
			}
			int itemIndex = activeListView.listView.Items.IndexOf(((ListViewItem)item).DataContext);
			return itemIndex;
		}

		private dynamic GetListView(object sender) // 리스트뷰 찾기
		{
			if (sender == listView1) return listViewTypeA1;
			if (sender == listView2) return listViewTypeA2;
			if (sender == listView3) return listViewTypeB;
			object tmp1 = (FrameworkElement)((ContextMenu)((FrameworkElement)sender).Parent).PlacementTarget;
			if (tmp1 == listView1) return listViewTypeA1;
			if (tmp1 == listView2) return listViewTypeA2;
			if (tmp1 == listView3) return listViewTypeB;
			MessageBox.Show("error: GetListView");
			throw new Exception();
		}

		private void Rename(ClsListViewTypeA clsListViewTypeA1, ClsListViewTypeA clsListViewTypeA2, string job) // Rename (타입A)
		{
			if (clsListViewTypeA1.items.Count == clsListViewTypeA2.items.Count)
			{
				string error = "";
				for (int i = 0; i < clsListViewTypeA1.items.Count; i++)
				{
#pragma warning disable IDE0059 // 불필요한 값 할당 warning disable {
					string path1 = clsListViewTypeA1.items[i].Path;
					string path2 = clsListViewTypeA2.items[i].Path;
					string dir1 = clsListViewTypeA1.items[i].Directory;
					string dir2 = clsListViewTypeA2.items[i].Directory;
					string name1 = clsListViewTypeA1.items[i].FileName;
					string name2 = clsListViewTypeA2.items[i].FileName;
					string ext1 = clsListViewTypeA1.items[i].Extension;
					string ext2 = clsListViewTypeA2.items[i].Extension;
#pragma warning restore IDE0059 // }
					try
					{
						switch (job)
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
					ErrorMessageLog(error);
				}
			}
		}
		private void Rename(ClsListViewTypeB clsListViewTypeB, string job) // Rename (타입B)
		{
			string error = "";
			string fileList = "";
			string workDir = "";
			bool oneTimeTrue = true;
			for (int i = 0; i < clsListViewTypeB.items.Count; i++)
			{
				string path = clsListViewTypeB.items[i].Path;
				string dir = clsListViewTypeB.items[i].Directory;
				string name = clsListViewTypeB.items[i].FileName;
				string ext = clsListViewTypeB.items[i].Extension;
				string tempName = clsListViewTypeB.items[i].TempName;
				if (tempName is null) continue;
				try
				{
					switch (job)
					{
						case "run":
							fileList += string.Format("{0}\t{1}\t{2}\t{3}\n", tempName, dir, name, ext);
							if (oneTimeTrue)
							{
								oneTimeTrue = false;
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
				ErrorMessageLog(error);
			}
		}
		private void ErrorMessageLog(string error) // Rename 에러 메시지, 로그
		{
			MessageBox.Show(error, "error message");
			string logFileName = string.Format("error_message_{0}.txt", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
			MessageBox.Show(AppDomain.CurrentDomain.BaseDirectory + logFileName, logFileName);
			File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + logFileName, error);
		}

		private void SetTempName(ClsListViewTypeB clsListViewTypeB) // TempName
		{
			if ((bool)checkBox2.IsChecked)
			{
				int countSelectedItem = 0;
				foreach (ClsListViewItemTypeB item in clsListViewTypeB.items) if (item.Selected) countSelectedItem++;
				textBox3.Text = ((int)Math.Log10(countSelectedItem) + 1).ToString();
			}
			foreach (ClsListViewItemTypeB item in clsListViewTypeB.items)
			{
				if (item.Selected)
				{
					item.TempName = TempName();
				}
			}
			clsListViewTypeB.listView.Items.Refresh(); // 새로고침
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

		private void AddItems(string[] dropFiles, dynamic clsListView) // 드롭 파일 아이템 추가
		{
			DeselectAll(clsListView);
			Array.Sort(dropFiles);
			foreach (string dropFile in dropFiles)
			{
				int index1 = dropFile.LastIndexOf('\\') + 1;
				int index2 = dropFile.LastIndexOf('.');
				if (index2 > 0)
				{
					if (clsListView is ClsListViewTypeA) clsListView.items.Add(new ClsListViewItemTypeA()
					{
						Path = dropFile,
						Directory = dropFile.Substring(0, index1),
						FileName = dropFile.Substring(index1, index2 - index1),
						Extension = dropFile.Substring(index2),
						Selected = true
					});
					else if (clsListView is ClsListViewTypeB) clsListView.items.Add(new ClsListViewItemTypeB()
					{
						Path = dropFile,
						Directory = dropFile.Substring(0, index1),
						FileName = dropFile.Substring(index1, index2 - index1),
						Extension = dropFile.Substring(index2),
						Selected = true
					});
				}
			}
			ColumnAutoWidth(clsListView.listView);
		}
		private void DropFiles(DragEventArgs e, ClsListViewTypeA clsListViewTypeA) // 드롭 파일 추가 (타입A)
		{
			string[] dropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
			AddItems(dropFiles, clsListViewTypeA);
		}
		private void DropFiles(DragEventArgs e, ClsListViewTypeB clsListViewTypeB) // 드롭 파일 추가 (타입B)
		{
			string[] dropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
			if (dropFiles[0].Contains("TempName.txt"))
			{
				clsListViewTypeB.items.Clear();
				foreach (string line in File.ReadAllLines(dropFiles[0]))
				{
					string[] tmp1 = line.Split('\t');
					clsListViewTypeB.items.Add(new ClsListViewItemTypeB()
					{
						Path = tmp1[1] + tmp1[2] + tmp1[3],
						Directory = tmp1[1],
						FileName = tmp1[2],
						Extension = tmp1[3],
						TempName = tmp1[0],
						Selected = true
					});
				}
				ColumnAutoWidth(clsListViewTypeB.listView);
			}
			else
			{
				AddItems(dropFiles, clsListViewTypeB);
				SetTempName(clsListViewTypeB);
			}
		}

		private void ItemRemove(dynamic clsListView) // 선택 삭제
		{
			for (int i = clsListView.items.Count - 1; i >= 0; i--) // 안전하게 역순으로 삭제
			{
				if (clsListView.items[i].Selected) clsListView.items.RemoveAt(i);
			}
		}

		private void ItemClear(dynamic clsListView) // 모두 삭제
		{
			clsListView.items.Clear();
		}

		private void MouseWheelNumber(object sender, MouseWheelEventArgs e) // 마우스휠로 숫자 증감
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
			if (textBox2 != null) // 시작시 예외 발생(NullReferenceException)
			{
				textBox2.Text = "1";
			}
		}

		private void DeselectAll(dynamic clsListView)
		{
			clsListView.listView.UnselectAll();
			foreach (dynamic item in clsListView.items)
			{
				if (item.Selected) item.Selected = false;
			}
		}
	}
}
