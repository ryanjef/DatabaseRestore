using DBRestore.Config;
using DBRestore.Model;
using DBRestore.View;
using DBRestore.Database;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DBRestore.ViewModel
{
    public class MainWindowViewModel : ObservableObject
    {
        #region Members
        ObservableCollection<string> _databases = new ObservableCollection<string>();
        string _currentDatabaseSelected, _databaseBackupFile;
        int  _percentComplete;
        private BackgroundWorker _backgroundWorker;
        private ConnectionSettingsWindow _connectionSettingsWindow;
        private bool _isRestoring;
        #endregion

        #region Constructor
        public MainWindowViewModel()
        {
            if (ConfigUtil.TestConnection(false))
            {
                this.PopulateDatabaseCombobox();
            }

            _backgroundWorker = new BackgroundWorker() { WorkerSupportsCancellation = true };
            _backgroundWorker.DoWork += new DoWorkEventHandler(bw_DoWork);
            _backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            IsRestoring = false;
        }
        #endregion


        public bool IsRestoring
        {
            get { return _isRestoring; }
            private set { _isRestoring = value; }
        }

        /// <summary>
        /// Gets the collection of databases
        /// </summary>
        public IEnumerable<string> Databases
        {
            get { return _databases; }
        }

        /// <summary>
        /// Gets of sets the Currently Selected Database
        /// </summary>
        public string CurrentDatabaseSelected
        {
            get { return _currentDatabaseSelected; }
            set
            {
                if (_currentDatabaseSelected == value) return;
                _currentDatabaseSelected = value;
                RaisePropertyChangedEvent("CurrentDatabaseSelected");
            }
        }

        /// <summary>
        /// Gets or sets the value of the progessbar percent complete
        /// </summary>
        public int PercentComplete
        {
            get { return _percentComplete; }
            set
            {
                _percentComplete = value;
                RaisePropertyChangedEvent("PercentComplete");
            }
        }

        /// <summary>
        /// Gets or sets the Database Backup File path
        /// </summary>
        public string DatabaseBackupFile
        {
            get { return _databaseBackupFile; }
            set
            {
                if (_databaseBackupFile == value) return;
                _databaseBackupFile = value;
                RaisePropertyChangedEvent("DatabaseBackupFile");
            }
        }

        //Show file chooser
        public ICommand ShowFileChooserDialogCommand
        {
            get { return new DelegateCommand(ShowFileChooser); }
        }

        private void ShowFileChooser()
        {
            var fileChooser = new OpenFileDialog();

            fileChooser.Filter = "bak files (*.bak)|*.bak|All files (*.*)|*.*" ;

            if (fileChooser.ShowDialog() == true)
            {
                DatabaseBackupFile = fileChooser.FileName;
            }
        }

        /// <summary>
        /// Show connection settings window command
        /// </summary>
        public ICommand ConnectionSettingsCommand
        {
            get { return new DelegateCommand(ShowConnectionSettings); }
        }

        /// <summary>
        /// Shows connection settings window
        /// </summary>
        private void ShowConnectionSettings()
        {
            _connectionSettingsWindow = new ConnectionSettingsWindow();
            _connectionSettingsWindow.Show();
            _connectionSettingsWindow.Closed += connectionSettingsWindow_Closed;
        }

        //refresh the database combobox when connection settings window is closed
        private void connectionSettingsWindow_Closed(object sender, EventArgs e)
        {
            if (ConfigUtil.TestConnection(false))
            {
                PopulateDatabaseCombobox();
            }
            else
            {
                _databases.Clear();
            }
        }

        /// <summary>
        /// Restore Command
        /// </summary>
        public ICommand RestoreCommand
        {
            get { return new RestoreCommand(Restore, this); }
        }


        /// <summary>
        /// Restores the database
        /// </summary>
        private void Restore()
        {
            PercentComplete = 2;

            if (_backgroundWorker.IsBusy != true)
            {
                IsRestoring = true;
                _backgroundWorker.RunWorkerAsync();
            }

        }

        /// <summary>
        /// Cancel Command
        /// </summary>
        public ICommand CancelCommand
        {
            get { return new CancelCommand(Cancel, this); }
        }

        /// <summary>
        /// Cancel Restore
        /// </summary>
        private void Cancel()
        {
            if (IsRestoring)
            {
                var dialogResult = MessageBox.Show("Cancelling a Restore in Progress can leave the database in a weird state.\n\nAre You Sure?", "Are you sure?", MessageBoxButton.YesNo);

                if (dialogResult == MessageBoxResult.Yes)
                {
                    if (_backgroundWorker.WorkerSupportsCancellation == true)
                    {
                        _backgroundWorker.CancelAsync();
                        _backgroundWorker.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// About Command
        /// </summary>
        public ICommand AboutCommand
        {
            get { return new DelegateCommand(About); }
        }

        /// <summary>
        /// Shows the about messagebox
        /// </summary>
        private void About()
        {
            MessageBox.Show("DBRestore Version 0.1beta\n\nBeta is Latin for “still doesn’t work.\"", "About");
        }

        /// <summary>
        /// Populates the combobox with all databases on the server
        /// </summary>
        private void PopulateDatabaseCombobox()
        {
            var connectionString = ConfigUtil.GetConnectionString();
            var selectDatabases = "SELECT NAME FROM SYSDATABASES ORDER BY NAME";

            //clear the database collection if we are doing a refresh
            if (_databases.Count != 0)
            {
                _databases.Clear();
            }

            if (ConfigUtil.TestConnection(false))
            {
                try
                {
                    using (var sqlConnection = new SqlConnection(connectionString))
                    {

                        var cmd = new SqlCommand(selectDatabases, sqlConnection);
                        cmd.Connection.Open();
                        var reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            var newDb = reader.GetString(0);
                            if (newDb.Equals("master", StringComparison.InvariantCultureIgnoreCase) == false)
                            {
                                _databases.Add(newDb);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// Background worker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            
            var res = new RestoreDB(this);
            
            var worker = sender as BackgroundWorker;

            bool restoreSucceeded = res.RestoreDatabase(CurrentDatabaseSelected, DatabaseBackupFile);

            //ToDo: Actually, probably dont ever need to cancel a restore in progress
            //but the only way I can figure out to cancel is to Restore the database Async
            //which causes other issues
            /*
            while (IsRestoring)
            {
                if (worker.CancellationPending == true)
                {       
                    e.Cancel = true;
                    break;
                }
            }
             */

            if (!restoreSucceeded)
            {
                e.Result = false;
            }
            else
            {
                e.Result = true;
            }
        }

        /// <summary>
        /// Worker Completed Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                PercentComplete = 0;
                IsRestoring = false;
            }
            else if (!(e.Error == null))
            {
                MessageBox.Show(e.Error.Message);
                PercentComplete = 0;
                IsRestoring = false;
            }
            else
            {
                IsRestoring = false;
                PercentComplete = 0;
                if ((bool)e.Result)
                {
                    MessageBox.Show("Restore Completed");
                }
            }
        }


    }
}
