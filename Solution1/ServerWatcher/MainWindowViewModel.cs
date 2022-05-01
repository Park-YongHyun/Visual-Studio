using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ServerWatcher
{
	/*	ssh 서버
	 *		ssh => docker containerName start|stop|restart
	 *	
	 *	서버
	 *		5분 간격으로 상태 검사 (로컬, 서버)
	 *		검사 start/stop
	 */
	public class MainWindowViewModel : ViewModelBase
	{
		private Visibility _winVisibility = Visibility.Visible; // Binding Mode=TwoWay
		private WindowState _winState;
		private string _serverButton1Content = "disabled";
		private string lastErrorMessage;

		private ICommand _sshServerCommand;
		private ICommand _serverCommand1;
		private ICommand _showErrorMessageCommand;
		private ICommand _taskbarIconClickCommand;
#if DEBUG
		private ICommand _testCommand;
#endif

		public event EventHandler Warned;

		private static readonly HttpClient httpClient = new();
		private static readonly SshClient sshClient = new(
			Config.GetRoot.SshServer.Host,
			Config.GetRoot.SshServer.Port,
			Config.GetRoot.SshServer.UserName,
			Config.GetRoot.SshServer.Password);

		// System.InvalidOperationException, 다른 스레드가 이 개체를 소유하고 있어 호출 스레드가 해당 개체에 액세스할 수 없습니다.
		// => System.Timers.Timer 대신 System.Windows.Threading.DispatcherTimer 사용
		//private Timer timer1 = new(Config.GetRoot.Server.Interval);
		private DispatcherTimer _watchTimer;

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

		public string ServerButton1Content
		{
			get => _serverButton1Content;
			set => SetProperty(ref _serverButton1Content, value);
		}

		public ICommand SshServerCommand
		{
			get
			{
				if (_sshServerCommand == null)
					_sshServerCommand = new RelayCommand<string>(SshServerCommandExec);
				return _sshServerCommand;
			}
		}
		private void SshServerCommandExec(string param) // param = "start" | "stop" | "restart"
		{
			try
			{
				sshClient.Connect();
				Debug.WriteLine("connected");
				SshCommand result = sshClient.RunCommand($"docker {param} {Config.GetRoot.SshServer.DockerContainerName}");
				Debug.WriteLine(result.Result);
				sshClient.Disconnect();
				Debug.WriteLine("disconnected");
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message, null, MessageBoxButton.OK, MessageBoxImage.Error);
			}

			//switch (param)
			//{
			//	case "start":
			//		break;
			//	case "stop":
			//		break;
			//	case "restart":
			//		break;
			//}
		}

		public ICommand ServerCommand1
		{
			get
			{
				if (_serverCommand1 == null)
					_serverCommand1 = new RelayCommand<object>(param => ServerCommand1Exec());
				return _serverCommand1;
			}
		}
		private void ServerCommand1Exec()
		{
			if (WatchTimer.IsEnabled)
			{
				ServerButton1Content = "disabled";
				WatchTimer.Stop();
			}
			else
			{
				ServerButton1Content = "enabled";
				if (WinVisibility == Visibility.Visible)
					WinVisibility = Visibility.Hidden;
				WatchTimer.Start();
				ServerWatch();
			}
		}

		public ICommand ShowErrorMessageCommand
		{
			get
			{
				if (_showErrorMessageCommand == null)
					_showErrorMessageCommand = new RelayCommand<object>(param => ShowErrorMessageCommandExec(), param => ShowErrorMessageCommandCanExec());
				return _showErrorMessageCommand;
			}
		}
		private void ShowErrorMessageCommandExec()
		{
			MessageBox.Show(lastErrorMessage, null);
		}
		private bool ShowErrorMessageCommandCanExec()
		{
			return lastErrorMessage != null;
		}

		public ICommand TackbarIconClickCommand
		{
			get
			{
				if (_taskbarIconClickCommand == null)
					_taskbarIconClickCommand = new RelayCommand<object>(param => TaskbarIconClickCommandExec());
				return _taskbarIconClickCommand;
			}
		}
		private void TaskbarIconClickCommandExec()
		{
			WinVisibility = WinVisibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
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
			ServerWatch();
		}
#endif

		public void OnWarned()
		{
			Warned.Invoke(null, EventArgs.Empty);
		}

		private DispatcherTimer WatchTimer
		{
			get
			{
				if (_watchTimer == null)
				{
					_watchTimer = new()
					{
						Interval = new TimeSpan(Config.GetRoot.Server.Interval.Hours, Config.GetRoot.Server.Interval.Minutes, Config.GetRoot.Server.Interval.Seconds)
					};
					_watchTimer.Tick += (sender, eventArgs) =>
					{
						Debug.WriteLine($"{nameof(_watchTimer)} elapsed");
						ServerWatch();
					};
				}
				return _watchTimer;
			}
		}

		private async void ServerWatch()
		{
			bool resultIsSuccess = false;
			string resultStatus = "";

			for (int i = 0; i < 3; i++)
			{
				try
				{
					Debug.WriteLine("request");

					HttpResponseMessage result = await httpClient.GetAsync(Config.GetRoot.Server.Url);
					resultIsSuccess = result.IsSuccessStatusCode;
					resultStatus = $"status: {(int)result.StatusCode} {result.StatusCode}";
					if (resultIsSuccess)
					{
						lastErrorMessage = null;
						break;
					}
					else
					{
						lastErrorMessage = resultStatus;
						System.Threading.Thread.Sleep(5000);
					}
				}
				catch (Exception e)
				{
					lastErrorMessage = e.Message;
					System.Threading.Thread.Sleep(5000);
				}
			}

			if (resultIsSuccess)
			{
				Debug.WriteLine(resultStatus);
			}
			else
			{
				Debug.WriteLine(lastErrorMessage);

				if (WinState == WindowState.Normal)
					WinState = WindowState.Minimized;
				if (WinVisibility == Visibility.Hidden)
					WinVisibility = Visibility.Visible;

				OnWarned();
			}
		}
	}
}
