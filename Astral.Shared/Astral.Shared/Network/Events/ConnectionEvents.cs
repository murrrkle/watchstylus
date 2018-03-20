using System;
using System.Collections.Generic;
using System.Text;

namespace Astral.Net
{
    #region Delegates
    public delegate void ConnectionEventHandler(object sender, ConnectionEventArgs e);
    #endregion

    #region Event Arguments Classes
    public class ConnectionEventArgs : EventArgs
    {
        #region Class Members
        private Connection m_conn;
        #endregion

        #region Constructors
        public ConnectionEventArgs(Connection conn)
        {
            m_conn = conn;
        }
        #endregion

        #region Properties
        public Connection Connection
        {
            get { return m_conn; }
        }
        #endregion
    }
    #endregion

}
