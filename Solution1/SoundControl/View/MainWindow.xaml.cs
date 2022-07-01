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

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			Debug.WriteLine("OnSourceInitialized");

			Config.LoadJson();
			if (SoundDevice.VolumeControl.CalculateVolumeLevel()) Config.SaveJson(); // 볼륨 계산

			// 핫키, 리스너 등록
			Win32Api.VolumeControl.WindowsHook.SetWindowsHookEx();
			Win32Api.VolumeControl.RegHotKey.RegisterHotKey(this);
			Win32Api.SwitchDefaultAudioDevice.WinMessage.AddHook(this);

			SoundDevice.VolumeControl.ShowVolumePopup();
		}
		protected override void OnClosed(EventArgs e)
		{
			Debug.WriteLine("OnClosed");

			// 핫키, 리스너 제거
			Win32Api.VolumeControl.WindowsHook.UnhookWindowsHookEx();
			Win32Api.VolumeControl.RegHotKey.UnregisterHotKey();
			Win32Api.SwitchDefaultAudioDevice.WinMessage.RemoveHook();

			Win32Api.VolumeControl.SystemVolumeOSD.Restore(); // 숨긴 볼륨 OSD 복원

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
		}
#endif
	}
}
