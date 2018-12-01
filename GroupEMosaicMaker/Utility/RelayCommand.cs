using System;
using System.Windows.Input;

namespace GroupEMosaicMaker.Utility
{
    public class RelayCommand : ICommand
    {
        #region Data members

        private readonly Action<object> execute;
        private readonly Predicate<object> canExecute;

        #endregion

        #region Constructors

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        #endregion

        #region Methods

        public bool CanExecute(object parameter)
        {
            var result = this.canExecute?.Invoke(parameter) ?? true;
            return result;
        }

        public void Execute(object parameter)
        {
            if (this.CanExecute(parameter))
            {
                this.execute(parameter);
            }
        }

        public event EventHandler CanExecuteChanged;

        /// <summary>
        ///     Typically, protected but made public, so can trigger a manual refresh on the result of CanExecute.
        /// </summary>
        public virtual void OnCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}