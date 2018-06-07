using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace DBRestore.ViewModel
{
    class RestoreCommand : ICommand
    {
        private MainWindowViewModel _viewModel;
        private readonly Action _action;
        
        public RestoreCommand(Action action, MainWindowViewModel viewModel)
        {
            _action = action;
            _viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return (_viewModel.CurrentDatabaseSelected != null && _viewModel.DatabaseBackupFile != null && _viewModel.IsRestoring == false);
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
