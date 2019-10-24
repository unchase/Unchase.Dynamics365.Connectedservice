using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Unchase.Dynamics365.ConnectedService.Common.Commands
{
    internal class StackPanelChangeVisibilityCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            (parameter as StackPanel)?.ChangeStackPanelVisibility();
        }
    }
}
