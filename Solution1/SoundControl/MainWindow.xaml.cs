using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace SoundControl
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

#if DEBUG
			SimpleEvent.Event1 += Test;
#endif
		}

		private readonly VolumePopup volumePopup = new();

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			Debug.WriteLine("OnSourceInitialized");


			//volumePopup.Show();


			Config.LoadJson();

			Win32Api.WindowsHook.SetWindowsHookEx();

			Win32Api.RegHotKey.RegisterHotKey(this);
		}
		protected override void OnClosed(EventArgs e)
		{
			Debug.WriteLine("OnClosed");


			Win32Api.WindowsHook.UnhookWindowsHookEx();

			Win32Api.RegHotKey.UnregisterHotKey();


			base.OnClosed(e);

			Application.Current.Shutdown();
		}

		private void MenuItemExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

#if DEBUG
		private void Test(object sender, EventArgs e)
		{
			//Win32Api.Test(new WindowInteropHelper(this).Handle);

			//System.Threading.Thread.Sleep(2000);
			//Win32Api.SetWinPos.SetWindowPos(volumePopup);
		}
#endif
	}
}
