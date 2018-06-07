using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using DBRestore.ViewModel;
using System.Windows.Threading;
using System.Data.SqlClient;
using DBRestore.Config;

namespace DBRestore.Database
{
    public class RestoreDB
    {
        private Server srv;
        private Restore res;
        private MainWindowViewModel viewModel;

        public RestoreDB(MainWindowViewModel databaseViewModel)
        {
            this.viewModel = databaseViewModel;
        }

        public bool RestoreDatabase(string database, string filePath)
        {
            using (var sqlConnection = new SqlConnection(ConfigUtil.GetConnectionString()))
            {
                
                ServerConnection conn = new ServerConnection(ConfigUtil.GetServerName(), ConfigUtil.GetUserName(), ConfigUtil.GetPassword());
                srv = new Server(conn);

                try
                {
                
                    res = new Restore()
                    {
                        Database = database,
                        NoRecovery = false,
                        ReplaceDatabase = true
                    };


                    res.PercentComplete += new PercentCompleteEventHandler(Restore_PercentComplete);
                    res.Devices.AddDevice(filePath, DeviceType.File);

                    var DataFile = new RelocateFile();
                    var MDF = res.ReadFileList(srv).Rows[0][1].ToString();
                    DataFile.LogicalFileName = res.ReadFileList(srv).Rows[0][0].ToString();
                    DataFile.PhysicalFileName = srv.Databases[database].FileGroups[0].Files[0].FileName;

                    var LogFile = new RelocateFile();
                    var LDF = res.ReadFileList(srv).Rows[1][1].ToString();
                    LogFile.LogicalFileName = res.ReadFileList(srv).Rows[1][0].ToString();
                    LogFile.PhysicalFileName = srv.Databases[database].LogFiles[0].FileName;

                    res.RelocateFiles.Add(DataFile);
                    res.RelocateFiles.Add(LogFile);

                    this.KillProcesses(database);

                    //this.viewModel.RestoreStarted = true;

                    res.SqlRestore(srv);

                    return true;

                }
                catch (SmoException ex)
                {
                    var innerExceptionMessage = "";
                    if (ex.InnerException != null)
                    {
                        innerExceptionMessage = ex.InnerException.Message;
                    }
                    MessageBox.Show(string.Format("{0}\n\n{1}", ex.Message, innerExceptionMessage));
                    return false;
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.Message);
                    return false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return false;
                }

            }
        }

        /// <summary>
        /// Kill all active processes on a database
        /// </summary>
        /// <param name="databaseName"></param>
        private void KillProcesses(string databaseName)
        {
            using (var sqlConnection = new SqlConnection(ConfigUtil.GetConnectionString()))
            {
                var conn = new ServerConnection(sqlConnection);
                var server = new Server(conn);

                if (server.GetActiveDBConnectionCount(databaseName) != 0)
                {
                    server.KillAllProcesses(databaseName);
                }
            }
        }


        /// <summary>
        /// Percentage complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Restore_PercentComplete(object sender, PercentCompleteEventArgs e)
        {
           Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
           new Action(() =>
           {
               this.viewModel.PercentComplete = e.Percent;
               /*
               if (e.Percent == 100)
               {
                   this.viewModel.RestoreStarted = false;
               }
               */
           })); 
            
        }

    }
}
