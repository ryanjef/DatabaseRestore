using DBRestore.Config;
using DBRestore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DBRestore.ViewModel
{
    public class ConnectionSettingsViewModel : ObservableObject
    {
        #region Members
        private ConnectionSettings _connectionSettings = new ConnectionSettings();
        private bool _isSelected, _textboxEnabled;
        #endregion

        #region Properties
        public bool TextboxEnabled
        {
            get { return _textboxEnabled; }
            set
            {
                _textboxEnabled = value;
                RaisePropertyChangedEvent("TextboxEnabled");
            }
        }

        public bool IsWindowsAuthenticationSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                if (_isSelected)
                {
                    this.TextboxEnabled = false;
                }
                else
                {
                    this.TextboxEnabled = true;
                }
                RaisePropertyChangedEvent("IsSelected");
            }
        }
        
        public string ServerName
        {
            get { return _connectionSettings.ServerName; }
            set
            {
                if (_connectionSettings.ServerName == value) return;
                _connectionSettings.ServerName = value;
                RaisePropertyChangedEvent("ServerName");
           }
        }

        public string UserName
        {
            get { return _connectionSettings.UserName; }
            set
            {
                if (_connectionSettings.UserName == value) return;
                _connectionSettings.UserName = value;
                RaisePropertyChangedEvent("UserName");
            }
        }

        public string Password
        {
            get { return _connectionSettings.Password; }
            set
            {
                if (_connectionSettings.Password == value) return;
                _connectionSettings.Password = value;
                RaisePropertyChangedEvent("Password");
            }
        }
        #endregion
        
        #region Constructor
        /// <summary>
        /// Constructs a new instance of ConnectionSettingsViewModel
        /// </summary>
        public ConnectionSettingsViewModel()
        {
            var builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            builder.ConnectionString = ConfigUtil.GetConnectionString();

            if (builder.IntegratedSecurity == true)
            {
                ServerName = builder.DataSource;
                UserName = "";
                Password = "";
                IsWindowsAuthenticationSelected = true;
                this.TextboxEnabled = false;
            }
            else
            {
                ServerName = builder.DataSource;
                UserName = builder.UserID;
                Password = builder.Password;
                IsWindowsAuthenticationSelected = false;
                this.TextboxEnabled = true;
            }
        }
        #endregion


        //Save Command
        public ICommand SaveCommand
        {
            get { return new DelegateCommand(Save); }
        }

        private void Save()
        {
             if (this.CheckForBlankValues())
             {
                 return;
             }

             var builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
             builder.ConnectionString = ConfigUtil.GetConnectionString();

             if (IsWindowsAuthenticationSelected)
             {
                 builder.DataSource = ServerName;
                 builder.IntegratedSecurity = true;
             }
             else
             {
                 builder.DataSource = ServerName;
                 builder.UserID = UserName;
                 builder.Password = Password;
                 builder.IntegratedSecurity = false;
             }

             ConfigUtil.SaveConnectionString(builder.ConnectionString);
        }

        //Test Connection Command
        public ICommand TestConnectionCommand
        {
            get { return new DelegateCommand(TestConnection); }
        }

        private void TestConnection()
        {
            if (!CheckForBlankValues())
            {
                return;
            }

            var builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            builder.ConnectionString = ConfigUtil.GetConnectionString();

            if (IsWindowsAuthenticationSelected)
            {
                builder.DataSource = ServerName;
                builder.IntegratedSecurity = true;
            }
            else
            {
                builder.IntegratedSecurity = false;
                builder.DataSource = ServerName;
                builder.UserID = UserName;
                builder.Password = Password;
            }

            if (ConfigUtil.TestConnection(builder.ConnectionString))
            {
                MessageBox.Show("SQL Server Connection is good");
            }
        }

        private bool CheckForBlankValues()
        {
            if (ServerName.Length == 0 || ServerName == null)
            {
                MessageBox.Show("Server Name cannot be blank");
                return false;
            }
            else if (UserName == null || UserName.Length == 0 && !IsWindowsAuthenticationSelected)
            {
                MessageBox.Show("Username cannot be blank");
                return false;
            }
            else if (Password == null || Password.Length == 0 && !IsWindowsAuthenticationSelected)
            {
                MessageBox.Show("Password cannot be blank");
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}
