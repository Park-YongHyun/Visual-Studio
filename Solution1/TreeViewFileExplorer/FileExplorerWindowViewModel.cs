using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TreeViewFileExplorer;

namespace TreeViewFileExplorer
{
	internal class FileExplorerWindowViewModel : ViewModelBase
	{
		public FileExplorerWindowViewModel()
		{
			//FileSystemTree.Add(new FileSystemEntryNode("t1"));
			//FileSystemTree.Add(new FileSystemEntryNode("t2"));
			//FileSystemTree[0].Children.Add(new FileSystemEntryNode("t3"));

			//GetTreeView();
		}

		private ICommand _command1;

		public ObservableCollection<FileSystemEntryNode> FileSystemTree { get; set; } = new();

		public ICommand Command1
		{
			get
			{
				_command1 ??= new RelayCommand<object>(param => Command1Exec());
				return _command1;
			}
		}
		private void Command1Exec()
		{
			//GetTreeView();
		}

		/*	파일 시스템 트리
		 *	
		 *	함수(노드 , 디렉토리)
		 *	{
		 *		foreach 하위 디렉토리
		 *		{
		 *			트리에 노드 추가
		 *			(재귀) 함수(하위 노드 , 하위 디렉토리)
		 *		}
		 *		foreach 하위 파일
		 *		{
		 *			트리에 노드 추가
		 *		}
		 *	}
		 */
		public void GetTreeView(string directoryPath)
		{
			ObservableCollection<FileSystemEntryNode> fileSystemTree = FileSystemTree;
			fileSystemTree.Clear();

			DirectoryInfo sourceDir = new(directoryPath);

			FileSystemEntryNode rootNode = new(sourceDir.Name, Win32Api.FileSystemIcon.GetIcon(sourceDir.FullName), true);
			//FileSystemEntryNode rootNode = new(sourceDir.Name, null, true);
			fileSystemTree.Add(rootNode);

			recursivefunc1(sourceDir, rootNode);

			void recursivefunc1(DirectoryInfo parentDir, FileSystemEntryNode parentNode)
			{
				Debug.WriteLine(parentDir.FullName);

				foreach (DirectoryInfo subDir in parentDir.EnumerateDirectories())
				{
					FileSystemEntryNode node = new(subDir.Name, Win32Api.FileSystemIcon.GetIcon(subDir.FullName), true);
					//FileSystemEntryNode node = new(subDir.Name, null, true);
					parentNode.Children.Add(node);

					recursivefunc1(subDir, node); // 재귀
				}
				foreach (FileInfo subFile in parentDir.EnumerateFiles())
				{
					FileSystemEntryNode node = new(subFile.Name, Win32Api.FileSystemIcon.GetIcon(subFile.FullName));
					//FileSystemEntryNode node = new(subFile.Name, null);
					parentNode.Children.Add(node);
				}
			}

			Debug.WriteLine("connecting line");
			FileSystemEntryNode.RefreshAllConnectingLines(fileSystemTree);
		}
	}
}
