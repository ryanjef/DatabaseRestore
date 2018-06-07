using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace DBRestore.ViewModel
{
    class CancelCommand : ICommand
    {
        private MainWindowViewModel _viewModel;
        private readonly Action _action;

        public CancelCommand(Action action, MainWindowViewModel viewModel)
        {
            _action = action;
            _viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return (_viewModel.IsRestoring == true);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _action();
        }
    }
}