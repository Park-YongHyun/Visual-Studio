using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SoundControl
{
	class VolumePopupViewModel : ViewModelBase
	{
		public VolumePopupViewModel()
		{
			Win32Api.VolumeChanged += OnVolumeChanged;
		}

		private Visibility _winVisibility = Visibility.Hidden; // Binding Mode=TwoWay
		private double _winOpacity = Config.GetRoot.Popup.WindowOpacity;

		private int _volumeLevel;

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

		public int VolumeLevel
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
					_showTimeoutTimer = new DispatcherTimer
					{
						Interval = TimeSpan.FromMilliseconds(Config.GetRoot.Popup.TimeoutMilliseconds)
					};
					_showTimeoutTimer.Tick += (sender, eventArgs) =>
					{
						Debug.WriteLine($"{nameof(_showTimeoutTimer)} elapsed");
						(sender as DispatcherTimer).Stop();

						if (WinVisibility == Visibility.Visible) WinVisibility = Visibility.Hidden;
					};
				}
				return _showTimeoutTimer;
			}
		}

		public void OnVolumeChanged(object sender, Win32Api.VolumeChangedEventArgs e)
		{
			Debug.WriteLine(nameof(OnVolumeChanged));
			VolumeLevel = e.VolumeLevel;

			if (WinVisibility != Visibility.Visible) WinVisibility = Visibility.Visible;

			if (ShowTimeoutTimer.IsEnabled)
			{
				ShowTimeoutTimer.Stop();
			}
			ShowTimeoutTimer.Start();
		}
	}
}
