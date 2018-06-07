using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBRestore.Model
{
    class Database
    {
        #region Members
        string _databaseName;
        #endregion

        #region Properties
        public string DatabaseName
        {
            get { return _databaseName; }
            set { _databaseName = value; }
        }
        #endregion

    }
}
