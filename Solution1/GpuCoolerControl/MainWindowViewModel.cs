using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace GpuCoolerControl
{
	internal class MainWindowViewModel : ViewModelBase
	{
		private Visibility _winVisibility = Visibility.Hidden; // Binding Mode=TwoWay
		private WindowState _winState;
		private string _activateButtonContent = "deactivated";
		private CoolerControl.GpuInfo _gpuInfo;

		private ICommand _taskbarIconClickCommand;
		private ICommand _controlActivateCommand;
#if DEBUG
		private ICommand _testCommand;
#endif

		private DispatcherTimer _controlTimer;
		private DispatcherTimer _gpuInfoTimer;


		public Visibility WinVisibility
		{
			get => _winVisibility;
			set => SetProperty(ref _winVisibility, value);
		}

		public WindowState WinState
		{
			get => _winState;
			set => SetProperty(ref _winState, value);
		}

		public string ActivateButtonContent
		{
			get => _activateButtonContent;
			set => SetProperty(ref _activateButtonContent, value);
		}

		public CoolerControl.GpuInfo GpuInfo
		{
			get => _gpuInfo;
			set => SetProperty(ref _gpuInfo, value);
		}

		public ICommand TackbarIconClickCommand
		{
			get
			{
				_taskbarIconClickCommand ??= new RelayCommand<object>(param => TaskbarIconClickCommandExec());
				return _taskbarIconClickCommand;
			}
		}
		private void TaskbarIconClickCommandExec()
		{
			ShowWindowSwitch();
		}

		public ICommand ControlActivateCommand
		{
			get
			{
				_controlActivateCommand ??= new RelayCommand<object>(param => ControlActivateCommandExec());
				return _controlActivateCommand;
			}
		}
		private void ControlActivateCommandExec()
		{
			ControlTimerSwitch();
		}

#if DEBUG
		public ICommand TestCommand1
		{
			get
			{
				if (_testCommand == null)
					_testCommand = new RelayCommand<object>(param => TestCommand1Exec());
				return _testCommand;
			}
		}
		private void TestCommand1Exec()
		{
			Debug.WriteLine("test command");
			//CoolerControl.GetInstance.SetFanSpeedLevel();

			UpdateGpuInfo();
		}
#endif

		private DispatcherTimer ControlTimer
		{
			get
			{
				if (_controlTimer == null)
				{
					_controlTimer = new()
					{
						Interval = TimeSpan.FromMilliseconds(Config.GetRoot.Control.TimerIntervalMilliseconds)
					};
					_controlTimer.Tick += (sender, args) =>
					{
						Debug.WriteLine($"{nameof(_controlTimer)} elapsed");
						CoolerControl.GetInstance.SetFanSpeedLevel();
					};
				}
				return _controlTimer;
			}
		}

		private DispatcherTimer GpuInfoTimer
		{
			get
			{
				if (_gpuInfoTimer == null)
				{
					_gpuInfoTimer = new()
					{
						Interval = TimeSpan.FromSeconds(2)
					};
					_gpuInfoTimer.Tick += (sender, args) =>
					{
						Debug.WriteLine($"{nameof(_gpuInfoTimer)} elapsed");
						UpdateGpuInfo();
					};
				}
				return _gpuInfoTimer;
			}
		}

		public void ShowWindowSwitch()
		{
			//WinVisibility = WinVisibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;

			if (WinVisibility == Visibility.Hidden)
			{
				UpdateGpuInfo();
				GpuInfoTimer.Start();
				WinVisibility = Visibility.Visible;
			}
			else
			{
				GpuInfoTimer.Stop();
				WinVisibility = Visibility.Hidden;
			}
		}

		public void ControlTimerSwitch()
		{
			if (ControlTimer.IsEnabled)
			{
				ControlTimer.Stop();
				ActivateButtonContent = "deactivated";
			}
			else
			{
				ControlTimer.Start();
				ActivateButtonContent = "activated";
			}
		}

		public void UpdateGpuInfo()
		{
			GpuInfo = CoolerControl.GetInstance.GetGpuInfo();
		}
	}
}
