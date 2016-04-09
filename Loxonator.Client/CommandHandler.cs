using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Loxonator.Client
{
    public abstract class CommandHandler : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged()
        {
            if (this.CanExecuteChanged != null)
                this.CanExecuteChanged(this, EventArgs.Empty);
        }

        public abstract bool CanExecute(object parameter);
        public abstract void Execute(object parameter);
    }
}
