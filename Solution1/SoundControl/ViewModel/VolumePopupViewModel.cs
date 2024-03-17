using SoundControl.Configuration;
using SoundControl.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SoundControl.ViewModel
{
	class VolumePopupViewModel : ViewModelBase
	{
		public VolumePopupViewModel()
		{
			SoundDevice.GetInstance.volumeControl.VolumeChanged += OnVolumeChanged;
		}

		private Visibility _winVisibility = Visibility.Hidden; // Binding Mode=TwoWay
		private double _winOpacity = Config.GetData.Popup.WindowOpacity;

		private string _deviceName;
		private decimal _volumeLevel;

		private DispatcherTimer _showTimeoutTimer;

		public Visibility WinVisibility
		{
			get => _winVisibility;
			set => SetProperty(ref _winVisibility, value);
		}

		public double WinOpacity
		{
			get => _winOpacity;
			set => SetProperty(ref _winOpacity, value);
		}

		public string DeviceName
		{
			get => _deviceName;
			set => SetProperty(ref _deviceName, value);
		}
		public decimal VolumeLevel
		{
			get => _volumeLevel;
			set => SetProperty(ref _volumeLevel, value);
		}

		public DispatcherTimer ShowTimeoutTimer
		{
			get
			{
				if (_showTimeoutTimer == null)
				{
					_showTimeoutTimer = new()
					{
						Interval = TimeSpan.FromMilliseconds(Config.GetData.Popup.TimeoutMillisec)
					};
					_showTimeoutTimer.Tick += (sender, eventArgs) =>
					{
						Debug.WriteLine($"{nameof(_showTimeoutTimer)} elapsed");
						(sender as DispatcherTimer)?.Stop();

						if (WinVisibility != Visibility.Hidden) WinVisibility = Visibility.Hidden;
					};
				}
				return _showTimeoutTimer;
			}
		}

		public void OnVolumeChanged(object sender, SoundDevice.VolumeControl.VolumeChangedEventArgs e)
		{
			Debug.WriteLine(nameof(OnVolumeChanged));
			DeviceName = e.DeviceName;
			VolumeLevel = decimal.Parse(e.VolumeLevel.ToString($"E{Config.GetData.Popup.SignificantDigits - 1}"), System.Globalization.NumberStyles.Float); // 정밀도 유효숫자 E + 1, Ex) E1 = 유효숫자 2

			if (WinVisibility != Visibility.Visible) WinVisibility = Visibility.Visible;

			if (ShowTimeoutTimer.IsEnabled)
			{
				ShowTimeoutTimer.Stop();
			}
			ShowTimeoutTimer.Start();
		}
	}
}
