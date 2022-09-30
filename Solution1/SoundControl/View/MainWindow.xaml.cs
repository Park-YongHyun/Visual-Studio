using SoundControl.Configuration;
using SoundControl.Model;
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

namespace SoundControl.View
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

		public VolumePopup volumePopup = new();

		protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);

			Debug.WriteLine($"{nameof(MainWindow)} OnSourceInitialized");

			SoundDevice soundDevice = SoundDevice.GetInstance;

			Config.LoadJson();
			if (soundDevice.volumeControl.CalculateVolumeLevel()) Config.SaveJson(); // 볼륨 계산

			// 핫키, 리스너 등록
			soundDevice.hotKeyAndMessage.windowsHook.SetWindowsHookEx();
			soundDevice.hotKeyAndMessage.regHotKey.RegisterHotKey(this);
			soundDevice.hotKeyAndMessage.winMessage.AddHook(this);

			soundDevice.popupControl.ShowPopup();
		}
		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			Debug.WriteLine($"{nameof(MainWindow)} OnClosed");

			SoundDevice soundDevice = SoundDevice.GetInstance;

			// 핫키, 리스너 제거
			soundDevice.hotKeyAndMessage.windowsHook.UnhookWindowsHookEx();
			soundDevice.hotKeyAndMessage.regHotKey.UnregisterHotKey();
			soundDevice.hotKeyAndMessage.winMessage.RemoveHook();

			Application.Current.Shutdown();
		}

		private void MenuItemExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

#if DEBUG
		private void Test(object sender, EventArgs e)
		{
		}
#endif
	}
}
