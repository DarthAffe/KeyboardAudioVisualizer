using System;
using System.Windows.Input;

namespace KeyboardAudioVisualizer.Helper
{
    public class ActionCommand : ICommand
    {
        #region Properties & Fields

        private readonly Func<bool> _canExecute;
        private readonly Action _command;

        #endregion

        #region Events

        public event EventHandler CanExecuteChanged;

        #endregion

        #region Constructors

        public ActionCommand(Action command, Func<bool> canExecute = null)
        {
            this._command = command;
            this._canExecute = canExecute;
        }

        #endregion

        #region Methods

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            _command?.Invoke();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }

        #endregion
    }
}
