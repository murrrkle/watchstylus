using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Astral.Net
{
    #region Class 'Server'
    public class Server
    {
        #region Class Members
        #region Network Class Members
        private TcpListener m_servSock;

        private List<Connection> m_connections;

        private IPAddress m_ipAddress;

        private int m_port;

        private string m_name;
        #endregion

        #region Control Class Members
        private bool m_running = false;
        #endregion

        #region Thread Class Members
        private Thread m_acceptThread;
        #endregion
        #endregion

        #region Events
        public event ConnectionEventHandler ConnectionAccepted;

        public event ConnectionEventHandler ConnectionRemoved;
        #endregion

        #region Constructors
        public Server(string name)
            : this(name, -1)
        { }

        public Server(string name, int port)
        {
            if (name == null
                || name.Equals(""))
            {
                throw new ConfigurationException("The 'Server' must have a name.");
            }

            m_name = name;

            IPAddress[] ipAddrs = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            IPAddress correctAddress = null;
            for (int i = 0; i < ipAddrs.Length; i++)
            {
                if (!(ipAddrs[i].IsIPv6LinkLocal)
                    && (ipAddrs[i].GetAddressBytes().Length == 4))
                {
                    correctAddress = ipAddrs[i];
                }
            }

            m_port = InitializeServer(port);
            m_ipAddress = correctAddress;
        }

        public Server(string name, IPAddress ipAddress)
            : this(name, ipAddress, -1)
        { }

        public Server(string name, IPAddress ipAddress, int port)
        {
            if (name == null
                || name.Equals(""))
            {
                throw new ConfigurationException("The 'Server' must have a name.");
            }

            m_name = name;

            m_port = InitializeServer(port);
            m_ipAddress = ipAddress;
        }
        #endregion

        #region Initialization
        private int InitializeServer()
        {
            return (InitializeServer(-1));
        }

        private int InitializeServer(int port)
        {
            if (port < 1)
            {
                port = Configuration.Network.StartPort;
            }

            bool portValid = false;
            while (!(portValid))
            {
                try
                {
                    m_servSock = new TcpListener(IPAddress.Any, port);
                    m_servSock.Start();
                    portValid = true;

                    return port;
                }
                catch (Exception)
                {
                    portValid = false;
                    port += Configuration.Network.PortStepping;
                }
            }

            return -1;
        }
        #endregion

        #region Properties
        public IPAddress IPAddress
        {
            get { return m_ipAddress; }
        }

        public int Port
        {
            get { return m_port; }
        }

        public bool IsRunning
        {
            get { return m_running; }
        }

        internal List<Connection> Connections
        {
            get { return m_connections; }
        }
        #endregion

        #region Static Properties
        public static IPAddress[] AllAddresses
        {
            get { return Dns.GetHostEntry(Dns.GetHostName()).AddressList; }
        }
        #endregion

        #region Start/Stop Methods
        public void Start()
        {
            m_connections = new List<Connection>();

            m_acceptThread = new Thread(new ThreadStart(Accept))
            {
                Name = "TcpServer#" + IPAddress + ":" + Port,
                IsBackground = true
            };
            m_acceptThread.Start();
        }

        public void Stop()
        {
            if (m_running)
            {
                m_running = false;

                for (int i = 0; i < m_connections.Count; i++)
                {
                    Connection connection = m_connections[i];
                    connection.Stop();
                    connection = null;
                }

                if (m_servSock != null)
                {
                    m_servSock.Stop();
                }

                if (m_acceptThread != null)
                {
                    m_acceptThread.Join(100);
                    if (m_acceptThread != null
                        && m_acceptThread.IsAlive)
                    {
                        try
                        {
                            m_acceptThread.Abort();
                        }
                        catch (Exception) { }
                        finally
                        {
                            m_acceptThread = null;
                        }
                    }
                }

                Debug.WriteLine("Server stopped!");
            }
        }
        #endregion

        #region Connection Methods
        #region Broadcast Methods
        public void BroadcastMessage(Message message, Connection excludedReceiver)
        {
            List<Connection> excludedReceivers = null;
            if (excludedReceiver != null)
            {
                excludedReceivers = new List<Connection> { excludedReceiver };
            }

            BroadcastMessage(message, excludedReceivers);
        }

        public void BroadcastMessage(Message message, List<Connection> excludedReceivers = null)
        {
            lock (this)
            {
                foreach (Connection connection in m_connections)
                {
                    if (excludedReceivers == null
                        || !(excludedReceivers.Contains(connection)))
                    {
                        connection.SendMessage(message);
                    }
                }
            }
        }
        #endregion

        #region Accept Methods
        private void Accept()
        {
            m_running = true;

#if DEBUG
            Debug.WriteLine("'Server' started on '"
                            + IPAddress + "' [port: " + Port + "]");
#endif

            while (m_running)
            {
                try
                {
                    TcpClient client = m_servSock.AcceptTcpClient();
                    Connection conn = new Connection(client);

#if DEBUG
                    Debug.WriteLine("Client connected... ["
                                    + ((IPEndPoint)client.Client.RemoteEndPoint).Address
                                    + ":" + Port + "]");
#endif

                    m_connections.Add(conn);

                    conn.ConnectionFailed += ConnectionTerminated;
                    conn.ConnectionTerminated += ConnectionTerminated;

                    ConnectionAccepted?.Invoke(this, new ConnectionEventArgs(conn));

                    conn.Start();
                }
                catch (Exception)
                {
                    m_running = false;
                }
            }
        }
        #endregion

        #region Delete Methods
        internal void RemoveConnection(Connection conn)
        {
            lock (m_connections)
            {
                if (m_connections.Contains(conn))
                {
#if DEBUG
                    Debug.WriteLine("Client disconnected... ["
                                    + conn.RemoteEndPoint.Address
                                    + ":" + Port + "]");
#endif


                    conn.ConnectionFailed -= ConnectionTerminated;
                    conn.ConnectionTerminated -= ConnectionTerminated;

                    conn.Stop();
                    m_connections.Remove(conn);

                    ConnectionRemoved?.Invoke(this, new ConnectionEventArgs(conn));
                }
            }
        }
        #endregion
        #endregion

        #region Event Handler
        private void ConnectionTerminated(object sender, ConnectionEventArgs e)
        {
            if (e != null
                && e.Connection != null)
            {
                RemoveConnection(e.Connection);
            }
        }
        #endregion
    }
    #endregion
}
