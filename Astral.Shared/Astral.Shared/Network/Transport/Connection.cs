using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Astral.Net
{
    // TODO: finish documentation

    #region Class 'ConnectionState'
    internal class ConnectionState
    {
        #region Properties
        public TcpClient Client { get; set; }

        public bool Success { get; set; }
        #endregion
    }
    #endregion

    #region Class 'Connection'
    public class Connection
    {
        #region Static Class Members
        private static object SyncObj = new object();
        #endregion

        #region Class Members
        #region Network Class Members
        private TcpClient m_sock;

        private IPAddress m_ipAddress;

        private int m_port;

        private IPEndPoint m_endPoint;
        #endregion

        #region Stream Class Members
        private NetworkStream m_netStrm;
        #endregion

        #region Control Class Members
        private bool m_running;
        #endregion

        #region Thread Class Members
        private Thread m_receivingThread;
        #endregion
        #endregion

        #region Events
        public event ConnectionEventHandler ConnectionEstablished;

        public event ConnectionEventHandler ConnectionFailed;

        public event ConnectionEventHandler ConnectionTerminated;

        public event MessageEventHandler MessageReceived;

        internal event MessageEventHandler InternalMessageReceived;
        #endregion

        #region Constructors
        public Connection(string ipAddress, int port)
            : this(IPAddress.Parse(ipAddress), port)
        { }

        public Connection(IPAddress ipAddress, int port)
        {
            m_running = false;

            m_ipAddress = ipAddress;
            m_port = port;
        }

        internal Connection(TcpClient socket)
        {
            m_running = false;
            m_sock = socket;

            m_endPoint = (IPEndPoint)socket.Client.RemoteEndPoint;
            m_ipAddress = m_endPoint.Address;
            m_port = m_endPoint.Port;
        }
        #endregion

        #region Properties
        internal IPEndPoint RemoteEndPoint
        {
            get { return m_endPoint; }
        }

        public bool IsRunning
        {
            get { return m_running; }
        }
        #endregion

        #region Starting/Stopping
        public void Start()
        {
            if (!(m_running))
            {
                try
                {
                    if (m_sock == null)
                    {
                        m_sock = new TcpClient();

                        ConnectionState state = new ConnectionState { Client = m_sock, Success = true };
                        IAsyncResult result = m_sock.BeginConnect(m_ipAddress, m_port, AsyncConnectionComplete, state);
                        state.Success = result.AsyncWaitHandle.WaitOne(Configuration.Network.Timeout, false);

                        if (!(state.Success) || !(m_sock.Connected))
                        {
                            throw new Exception("Could not connect to '" + m_ipAddress.ToString() + "' at port '" + m_port + "'");
                        }

                        m_endPoint = (IPEndPoint)m_sock.Client.RemoteEndPoint;
                    }

                    m_netStrm = m_sock.GetStream();
                    m_netStrm.WriteTimeout = 2000;

                    m_running = true;

                    m_receivingThread = new Thread(new ThreadStart(Receive))
                    {
                        IsBackground = true,
                        Name = "TcpConnection#receivingThread_0"
                    };
                    m_receivingThread.Start();

                    ConnectionEstablished?.Invoke(this, new ConnectionEventArgs(this));
                }
                catch (Exception e)
                {
#if DEBUG
                    Debug.WriteLine(e.Message + "\n" + e.StackTrace);
#endif

                    m_running = false;

                    ConnectionFailed?.Invoke(this, new ConnectionEventArgs(this));
                }
            }
        }

        public void Stop()
        {
            if (m_running)
            {
                m_running = false;
                if (m_netStrm != null)
                {
                    try
                    {
                        m_netStrm.Close();
                        m_netStrm.Dispose();
                    }
                    finally
                    {
                        m_netStrm = null;
                    }
                }

                if (m_sock != null)
                {
                    try
                    {
                        m_sock.Close();
                    }
                    finally
                    {
                        m_sock = null;
                    }
                }

                if (m_receivingThread != null)
                {
                    try
                    {
                        m_receivingThread.Join(100);
                        if (m_receivingThread != null
                            && m_receivingThread.IsAlive)
                        {
                            m_receivingThread.Abort();
                        }
                    }
                    finally
                    {
                        m_receivingThread = null;
                    }
                }

                ConnectionTerminated?.Invoke(this, new ConnectionEventArgs(this));
            }
        }
        #endregion

        #region Sending / Receiving
        #region Sending
        public void SendMessage(Message msg)
        {
            lock (this)
            {
                try
                {
                    byte[] sendBytes = msg.ToByteArray();
                    m_netStrm.Write(sendBytes, 0, sendBytes.Length);
                }
                catch (Exception)
                {
                    m_running = false;
                }
            }
        }
        #endregion

        #region Receiving Methods
        #region Reading Messages
        private byte[] ReadBytesFromStream(int length, Stream strm)
        {
            if (length < 0)
            {
                return null;
            }

            byte[] bytes = new byte[length];

            int read = 0;
            while (read < length)
            {
                int n = strm.Read(bytes, read, length - read);
                if (n < 0)
                {
                    throw new IOException("Can't read the message data");
                }
                read += n;
            }
            return bytes;
        }

        private MessageHeader ReadMessageHeader(Stream strm)
        {
            int totalLength = strm.ReadByte() << 24;
            totalLength |= (strm.ReadByte() << 16);
            totalLength |= (strm.ReadByte() << 8);
            totalLength |= strm.ReadByte();

            int controlFlag = strm.ReadByte();

            int nameLength = strm.ReadByte() << 8;
            nameLength |= strm.ReadByte();

            byte[] nameBytes = ReadBytesFromStream(nameLength, strm);
            string name = Encoding.UTF8.GetString(nameBytes);

            return (new MessageHeader(totalLength, (controlFlag == 1), name));
        }
        #endregion

        #region Threading
        private void OnMessageReceived(Message message)
        {
            if (!(message.IsInternal)
                && MessageReceived != null)
            {
                MessageReceived(this, message);
            }
            else if (message.IsInternal
                && InternalMessageReceived != null)
            {
                InternalMessageReceived(this, message);
            }
        }

        private void UpdateMessages(object obj)
        {
            if (obj is Message)
            {
                OnMessageReceived((Message)obj);
            }

            obj = null;
            GC.Collect();
        }

        private void Receive()
        {
            while (m_running)
            {
                try
                {
                    MessageHeader header = ReadMessageHeader(m_netStrm);
                    if (header.IsValid)
                    {
                        byte[] content = ReadBytesFromStream(header.ContentLength, m_netStrm);

                        Message message = Message.FromStream(header, content);
                        if (message != null)
                        {
                            if (Configuration.Network.IsMultiThreaded)
                            {
                                Thread updater = new Thread(new ParameterizedThreadStart(UpdateMessages))
                                {
                                    IsBackground = true
                                };
                                updater.Start(message);
                            }
                            else
                            {
                                OnMessageReceived(message);

                                message = null;
                                GC.Collect();
                            }
                        }
                    }
                    else
                    {
                        throw new IOException("Message header corrupt!");
                    }
                }
                catch(Exception ex)
                {
                    m_running = false;
                    Debug.WriteLine(ex.Message);
                }
            }
            
            ConnectionTerminated?.Invoke(this, new ConnectionEventArgs(this));

#if DEBUG
            Debug.WriteLine("Connection closed...");
#endif
        }
        #endregion
        #endregion
        #endregion

        #region Event Hander
        private void AsyncConnectionComplete(IAsyncResult ar)
        {
            ConnectionState state = ar.AsyncState as ConnectionState;
            TcpClient client = state.Client;

            try
            {
                client.EndConnect(ar);
            }
            catch { }

            if (client.Connected && state.Success)
            {
                return;
            }

            client.Close();
        }
        #endregion

        #region Overrides (Object)
        public override string ToString()
        {
            return m_endPoint.Address.ToString() + ":"
                + m_endPoint.Port.ToString();
        }
        #endregion
    }
    #endregion
}
