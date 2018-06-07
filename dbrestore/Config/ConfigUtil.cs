using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows;
using System.Data.SqlClient;

namespace DBRestore.Config
{
    static class ConfigUtil
    {
        /// <summary>
        /// Gets the connection string from the app config
        /// </summary>
        /// <returns></returns>
        public static string GetConnectionString()
        {
            var appConfig = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var connectionString = appConfig.ConnectionStrings.ConnectionStrings["DBConnection"].ConnectionString;
            return connectionString;
        }

        /// <summary>
        /// Gets the server name from the connection string
        /// </summary>s
        /// <returns></returns>
        public static string GetServerName()
        {
            var builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            builder.ConnectionString = GetConnectionString();
            return builder.DataSource;
        }

        /// <summary>
        /// Gets the username from the connection string
        /// </summary>
        /// <returns></returns>
        public static string GetUserName()
        {
            var builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            builder.ConnectionString = GetConnectionString();
            return builder.UserID;
        }

        /// <summary>
        /// Gets the password from the connectionstring
        /// </summary>
        /// <returns></returns>
        public static string GetPassword()
        {
            var builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            builder.ConnectionString = GetConnectionString();
            return builder.Password;
        }

        /// <summary>
        /// Sets the connection string in the app config
        /// </summary>
        /// <param name="conString"></param>
        public static void SaveConnectionString(string connectionString)
        {
            var appConfig = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetExecutingAssembly().Location);
            appConfig.ConnectionStrings.ConnectionStrings["DBConnection"].ConnectionString = connectionString;
            appConfig.Save();
        }

        /// <summary>
        /// Tests Sql Server Connection
        /// </summary>
        /// <param name="showMessage">True displays the SqlException message in a messagebox</param>
        /// <returns></returns>
        public static bool TestConnection(bool showMessage)
        {
            
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch (SqlException ex)
                {
                    if (showMessage)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    return false;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Tests Sql Server Connection with the given connection string
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static bool TestConnection(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                    return false;
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}
