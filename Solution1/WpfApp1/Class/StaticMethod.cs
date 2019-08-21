using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
	public static class ClassMethodGroup
	{
		public static void Rename(ClassListView classListView1, ClassListView classListView2, string str)
		{
			if (classListView1.listView.Items.Count == classListView2.listView.Items.Count)
			{
				String error = "";
				for (int i = 0; i < classListView1.listView.Items.Count; i++)
				{
					//string dir1 = classListView1.items[i].Directory;
					string dir2 = classListView2.items[i].Directory;
					string name1 = classListView1.items[i].Filename;
					string name2 = classListView2.items[i].Filename;
					//string ext1 = classListView1.items[i].Extension;
					string ext2 = classListView2.items[i].Extension;
					//MessageBox.Show(dir2 + name2 + ext2 + '\n' + dir2 + name1 + ext2);
					try
					{
						switch (str)
						{
							case "run":
								File.Move(dir2 + name2 + ext2, dir2 + name1 + ext2);
								break;
							case "restore":
								File.Move(dir2 + name1 + ext2, dir2 + name2 + ext2);
								break;
						}
					}
					catch(Exception exc)
					{
						error += exc.ToString();
					}
				}
				if(error != "")
				{
					MessageBox.Show(error, "error message");
				}
			}
		}
	}
}
