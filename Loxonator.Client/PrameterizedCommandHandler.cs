using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Loxonator.Client
{
    public class ParameterizedCommandHandler : CommandHandler
    {
        private Action<object> action;
        private Func<object, bool> canExecute;

        public ParameterizedCommandHandler(Action<object> action, Func<object, bool> canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        public ParameterizedCommandHandler(Action<object> action)
            : this(action, (parameter) => true)
        {
        }

        public override bool CanExecute(object parameter)
        {
            return this.canExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            this.action(parameter);
        }
    }
}
