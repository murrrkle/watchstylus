using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Astral.Device;
using Astral.Net;

namespace Astral
{
    #region Singleton Class 'AstralService'
    public sealed class AstralService
    {
        #region Static Class Members
        internal static volatile AstralService Instance;

        private static object SyncObj = new object();
        #endregion

        #region Class Members
        private Server m_server;

        private Dictionary<Connection, AstralSession> m_sessions;

        private bool m_running;
        #endregion

        #region Events
        public event AstralSessionEventHandler SessionEstablished;

        public event AstralSessionEventHandler SessionTerminated;
        #endregion

        #region Constructors
        private AstralService()
        {
            m_running = false;
            
            InitializeDeviceModules();

            // do this last
            InitializeServer();
        }
        #endregion

        #region Instantiation
        public static AstralService GetInstance()
        {
            if (Instance == null)
            {
                lock (SyncObj)
                {
                    if (Instance == null)
                    {
                        Instance = new AstralService();
                    }
                }
            }

            return Instance;
        }
        #endregion

        #region Initialization
        private void InitializeDeviceModules()
        {
            // TODO: here, we need to load any other DLL that contains special device configurations
        }

        private void InitializeServer()
        {
            // create the device list/dictionary
            m_sessions = new Dictionary<Connection, AstralSession>();

            // setup the server
            m_server = new Server(Configuration.Network.AstralServerName);
            m_server.ConnectionAccepted += ServerConnectionAccepted;
            m_server.ConnectionRemoved += ServerConnectionRemoved;
        }
        #endregion

        #region Properties
        public bool IsRunning
        {
            get { return m_running; }
            private set { m_running = value; }
        }
        #endregion

        #region Start/Stop
        public void Start()
        {
            if (!(IsRunning))
            {
                IsRunning = true;
                m_server.Start();
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;

                m_server.Stop();
                m_server = null;
            }
        }
        #endregion

        #region Message Handling
        private void HandleInitializationMessage(Message msg)
        {

        }
        #endregion

        #region Event Handler
        #region Network Event Handler
        private void ServerConnectionAccepted(object sender, ConnectionEventArgs e)
        {
            if (e.Connection != null
                && !(m_sessions.ContainsKey(e.Connection)))
            {
                // we got a new connection, add it
                AstralDevice device = new AstralDevice(e.Connection);
                AstralSession session = new AstralSession(device);

                session.SessionInitialized += OnAstralSessionInitialized;

                m_sessions.Add(e.Connection, session);
            }
        }

        private void ServerConnectionRemoved(object sender, ConnectionEventArgs e)
        {
            if (e.Connection != null
                && m_sessions.ContainsKey(e.Connection))
            {
                // ok, this connection should be removed
                AstralSession sessionToRemove = m_sessions[e.Connection];
                m_sessions.Remove(e.Connection);

                sessionToRemove.Stop();

                // notify clients
                SessionTerminated?.Invoke(this, sessionToRemove);
            }
        }

        private void OnAstralSessionInitialized(object sender, AstralSession session)
        {
            // remove the initialized event handler again (it's not needed)
            session.SessionInitialized -= OnAstralSessionInitialized;

            // now, we can actually inform the hosting application
            SessionEstablished?.Invoke(this, session);
        }
        #endregion
        #endregion
    }
    #endregion
}
