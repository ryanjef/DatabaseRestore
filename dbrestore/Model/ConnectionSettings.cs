using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBRestore.Model
{
    public class ConnectionSettings
    {
        #region Members
        private string _serverName;
        private string _userName;
        private string _password;
        #endregion

        #region Properties
        public string ServerName
        {
            get { return _serverName; }
            set { _serverName = value; }
        }

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
        #endregion
    }
}
