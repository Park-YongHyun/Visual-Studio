using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GpuCoolerControl
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void MenuItemExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			bool start = Config.GetRoot.Control.AutoStart;
#if DEBUG
			if (false && start)
#else
			if (start)
#endif
			{
				((MainWindowViewModel)DataContext).ControlTimerSwitch();
			}
			else
			{
				((MainWindowViewModel)DataContext).ShowWindowSwitch();
			}
		}
	}
}
