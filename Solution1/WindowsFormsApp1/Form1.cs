using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void ListView1_DragDrop(object sender, DragEventArgs e)
		{
			string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
			Array.Sort(fileNames);
			string tmp = string.Join("\n", fileNames);
			MessageBox.Show(tmp);
		}

		private void ListView1_DragEnter(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.Copy;
		}
	}
}
