using DBRestore.Config;
using DBRestore.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DBRestore.View
{
    /// <summary>
    /// Interaction logic for ConnectionSettingsWindow.xaml
    /// </summary>
    public partial class ConnectionSettingsWindow : Window
    {
        private ConnectionSettingsViewModel _connectionSettingsViewModel = new ConnectionSettingsViewModel();

        public ConnectionSettingsWindow()
        {
            InitializeComponent();
            base.DataContext = _connectionSettingsViewModel;
            this.ResizeMode = ResizeMode.NoResize;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ServerNameTextbox.Text == "")
            {
                return;
            }

            if (UserNameTextbox.Text.Length == 0 || PasswordTextbox.Text.Length == 0)
            {
                if (WindowsAuthCheckbox.IsChecked == false)
                {
                    return;
                }
            }
                        
            var builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            builder.ConnectionString = ConfigUtil.GetConnectionString();

            if (WindowsAuthCheckbox.IsChecked == true)
            {
                builder.DataSource = ServerNameTextbox.Text;
                builder.IntegratedSecurity = true;
            }
            else
            {
                builder.DataSource = ServerNameTextbox.Text;
                builder.UserID = UserNameTextbox.Text;
                builder.Password = PasswordTextbox.Text;
                builder.IntegratedSecurity = false;
            }

            ConfigUtil.SaveConnectionString(builder.ConnectionString);

            this.Close();
        }
    }
}
