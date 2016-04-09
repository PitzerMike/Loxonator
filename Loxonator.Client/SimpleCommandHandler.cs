using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Loxonator.Client
{
    public class SimpleCommandHandler : CommandHandler
    {
        private Action action;
        private Func<bool> canExecute;

        public SimpleCommandHandler(Action action, Func<bool> canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        public SimpleCommandHandler(Action action)
            : this(action, () => true)
        {
        }

        public override bool CanExecute(object parameter)
        {
            return this.canExecute();
        }

        public override void Execute(object parameter)
        {
            this.action();
        }
    }
}
