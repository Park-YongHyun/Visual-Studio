using SoundControl.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SoundControl.ViewModel
{
	class MainWindowViewModel : ViewModelBase
	{
		/*	볼륨 조절 (로그 단위)
		 *		글로벌 단축키
		 *	
		 *	system sounds unmute
		 *	
		 *	switch default audio device
		 */

		private Visibility _winVisibility = // Binding Mode=TwoWay
#if DEBUG
			Visibility.Visible;
#else
			Visibility.Hidden;
#endif
		private WindowState _winState;

		private ICommand _taskbarIconClickCommand;
#if DEBUG
		private ICommand _testCommand;
#endif
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
			WinVisibility = WinVisibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
		}

#if DEBUG
		public ICommand TestCommand
		{
			get
			{
				_testCommand ??= new RelayCommand<object>(param => TestCommandExec());
				return _testCommand;
			}
		}
		private void TestCommandExec()
		{
			SoundDevice.GetInstance.Test();
		}
#endif
	}
}
