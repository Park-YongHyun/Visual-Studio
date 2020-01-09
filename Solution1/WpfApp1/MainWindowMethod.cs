using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
	public partial class MainWindow
	{
		private ClassListView GetListView(object sender) // 리스트뷰 찾기
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

		private void Rename(ClassListView classListView1, ClassListView classListView2, string str) // Rename
		{
			if (classListView1.listView.Items.Count == classListView2.listView.Items.Count)
			{
				string error = "";
				string fileList = "";
				string dir = "";
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
					//MessageBox.Show(dir2 + name2 + ext2 + '\n' + dir2 + name1 + ext2);
					try
					{
						switch (str)
						{
							case "copyname":
								File.Move(path2, dir2 + name1 + ext2);
								break;
							case "restore":
								File.Move(dir2 + name1 + ext2, path2);
								break;
							case "tempname":
								fileList += string.Format("{0}\t{1}\t{2}\t{3}\n", name1, dir2, name2, ext2);
								if(i == 0)
								{
									dir = dir2;
									textBox1.Text = dir;
								}
								File.Move(path2, dir2 + name1 + ext2);
								break;
						}
					}
					catch(Exception exc)
					{
						error += exc.ToString();
					}
				}
				if(str == "tempname")
				{
					File.WriteAllText(dir + "tempname.txt", fileList);
				}
				if(error != "")
				{
					MessageBox.Show(error, "error message");
				}
			}
		}

		private void SetTempName(DragEventArgs e)
		{
			string[] dropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
			Array.Sort(dropFiles);
			foreach (string dropFile in dropFiles)
			{
				int index = dropFile.LastIndexOf('.');
				if (index > 0)
				{
					listViews["3"].items.Add(new ClassListViewItem()
					{
						Path = "",
						Directory = "",
						Filename = string.Format("{0}{1:00}", comboBox1.Text, Convert.ToInt32(textBox2.Text)),
						Extension = ""
					});
					textBox2.Text = (Convert.ToInt32(textBox2.Text) + 1).ToString();
				}
			}
			listViews["4"].Drop(e);
		}
	}
}
