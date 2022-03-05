using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace 
{
	class RelayCommand<T> : ICommand
	{
		private readonly Action<T> _execute;
		private readonly Predicate<T> _canExecute;
		public RelayCommand(Action<T> execute)
			: this(execute, null) { }

		public RelayCommand(Action<T> execute, Predicate<T> canExecute)
		{
			_execute = execute;
			_canExecute = canExecute;
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute(object parameter)
		{
			_execute((T)parameter);
		}

		public bool CanExecute(object parameter)
		{
			return _canExecute == null || _canExecute((T)parameter);
		}

		/* 뷰모델에서 사용 예시
		private ICommand _command1;
		public ICommand Command1
		{
			get
			{
				_command1 ??= new RelayCommand<object>(param => Command1Exec(), param => Command1CanExec());
				return _command1;
			}
		}
		private void Command1Exec()
		{
		}
		private bool Command1CanExec()
		{
			return true;
		}
		*/
	}
}
