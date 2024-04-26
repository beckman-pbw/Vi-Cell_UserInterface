using System;
using System.Diagnostics;
using System.Windows.Input;
using ScoutUtilities.Common;

namespace ScoutUtilities
{
    public class RelayCommand<T> : ICommand
    {
        #region Constructors

        public RelayCommand(Action<T> execute) : this(execute, null)
        {
        }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        #endregion

        #region Properties & Fields

        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;
        public event EventHandler CanExecuteChanged;

        #endregion

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke((T)parameter) ?? true;
        }
        
        public void Execute(object parameter)
        {
            if (parameter == null)
            {
                _execute(default(T));
            }
            else
            {
                _execute((T)parameter);
            }
        }

        #endregion

        public void RaiseCanExecuteChanged()
        {
            DispatcherHelper.ApplicationExecute(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
        }
    }

    public class RelayCommand : ICommand
    {
        protected readonly Action<object> _executeWithParameter;
        protected readonly Action _execute;
        protected readonly Func<bool> _canExecute;
        protected readonly bool _doExecuteWithParameter;
        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action execute) : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute) : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Func<bool> canExecute)
        {
            if(execute == null) throw new ArgumentNullException(nameof(execute));
            _executeWithParameter = execute;
            _canExecute = canExecute;
            _doExecuteWithParameter = true;
        }

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            _execute = execute;
            _canExecute = canExecute;
            _doExecuteWithParameter = false;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute();
        }

        public void Execute(object parameter)
        {
            if (_doExecuteWithParameter)
            {
                _executeWithParameter(parameter);
            }
            else
            {
                _execute();
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
