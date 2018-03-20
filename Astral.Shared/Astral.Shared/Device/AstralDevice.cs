using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

using Astral.Net;

namespace Astral.Device
{
    #region Sealed Class 'AstralDevice'
    public sealed class AstralDevice
    {
        #region Constant Class Members
        private const string InitializationMessageName = "AstralInit";

        private const string DeviceClassField = "devC";

        private const string DeviceNameField = "devN";

        private const string NumberOfModulesField = "numMod";

        private const string ModuleTypeFieldPrefix = "modT_";

        private const string ModuleObjectFieldPrefix = "modObj_";
        #endregion

        #region Class Members
        #region Network Class Members
        private Connection m_connection;

        private bool m_running;

        private bool m_initialized;
        #endregion

        #region Device Class Members
        private string m_deviceClass;

        private string m_deviceName;

        private Dictionary<ModuleType, IDeviceModule> m_modules;
        #endregion
        #endregion

        #region Events
        public event AstralDeviceEventHandler DeviceInitialized;

        public event EventHandler ServiceEstablished;

        public event EventHandler ServiceFailed;

        public event EventHandler ServiceTerminated;

        public event MessageEventHandler MessageReceived;
        #endregion

        #region Constructors/Destructors
        public AstralDevice(Connection connection)
        {
            m_modules = new Dictionary<ModuleType, IDeviceModule>();

            // note: this connection is already running
            m_connection = connection;
            m_running = true;
            m_initialized = false;

            // add event handlers
            m_connection.MessageReceived += OnMessageReceived;
        }

        public AstralDevice(string deviceClass, string deviceName)
        {
            m_modules = new Dictionary<ModuleType, IDeviceModule>();

            m_deviceClass = deviceClass;
            m_deviceName = deviceName;

            m_running = false;
            m_initialized = true;
        }

        ~AstralDevice()
        {
            if (m_connection != null)
            {
                // remove event handlers
                m_connection.MessageReceived -= OnMessageReceived;
            }
        }
        #endregion

        #region Properties
        public bool IsRunning
        {
            get { return m_running; }
            private set { m_running = value; }
        }

        public string Name
        {
            get { return m_deviceName; }
        }

        public string Class
        {
            get { return m_deviceClass; }
        }

        public IDeviceModule this[ModuleType type]
        {
            get
            {
                if (m_modules.ContainsKey(type))
                {
                    return m_modules[type];
                }
                return null;
            }
        }

        public bool HasDisplay
        {
            get { return HasModule(ModuleType.Display); }
        }

        public bool HasAccelerometer
        {
            get { return HasModule(ModuleType.Accelerometer); }
        }

        public bool HasGyroscope
        {
            get { return HasModule(ModuleType.Gyroscope); }
        }

        public bool HasCompass
        {
            get { return HasModule(ModuleType.Compass); }
        }

        public bool HasMagnetometer
        {
            get { return HasModule(ModuleType.Magnetometer); }
        }

        public bool HasOrientation
        {
            get { return HasModule(ModuleType.Orientation); }
        }

        public bool HasAmbientLight
        {
            get { return HasModule(ModuleType.AmbientLight); }
        }

        public bool HasMicrophone
        {
            get { return HasModule(ModuleType.Microphone); }
        }
        #endregion

        #region Connection
        public void Connect(IPAddress ipAddress, int port)
        {
            m_connection = new Connection(ipAddress, port);
            m_connection.ConnectionEstablished += AstralConnectionEstablished;
            m_connection.ConnectionFailed += AstralConnectionFailed;
            m_connection.ConnectionTerminated += AstralConnectionTerminated;
        }
        #endregion

        #region Start/Stop
        public void Start()
        {
            if (!(IsRunning))
            {
                IsRunning = true;
                m_connection.Start();
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                m_connection.Stop();
            }
        }
        #endregion

        #region Modules
        public void AddModule(IDeviceModule module)
        {
            if (!(m_modules.ContainsKey(module.Type)))
            {
                module.Host = this;
                m_modules.Add(module.Type, module);
            }
        }

        public void RemoveModule(IDeviceModule module)
        {
            if (m_modules.ContainsKey(module.Type))
            {
                m_modules.Remove(module.Type);
            }
        }

        public bool HasModule(ModuleType type)
        {
            return (m_modules.ContainsKey(type)
                && m_modules[type] != null);
        }
        #endregion

        #region Message Handling
        public void SendMessage(Message msg)
        {
            if (m_connection != null)
            {
                m_connection.SendMessage(msg);
            }
        }

        private void HandleInitializationMessage(Message msg)
        {
            m_deviceClass = msg.GetStringField(DeviceClassField);
            m_deviceName = msg.GetStringField(DeviceNameField);

            int numModules = msg.GetIntField(NumberOfModulesField);
            for (int i = 0; i < numModules; i++)
            {
                string typeStr = msg.GetStringField(ModuleTypeFieldPrefix + i);
                Type type = Type.GetType(typeStr, true, false);

                IDeviceModule module = (IDeviceModule)msg.GetField(
                    ModuleObjectFieldPrefix + i, type);
                AddModule(module);
            }

            DeviceInitialized?.Invoke(this, this);
        }

        private Message CreateInitializationMessage()
        {
            Message msg = new Message(InitializationMessageName);

            // add class and name
            msg.AddField(DeviceClassField, m_deviceClass);
            msg.AddField(DeviceNameField, m_deviceName);

            // add modules
            int moduleCounter = 0;

            IEnumerator<ModuleType> enumerator = m_modules.Keys.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ModuleType key = enumerator.Current;
                IDeviceModule module = m_modules[key];

                if (module != null)
                {
                    // we need to get the correct type
                    Type moduleType = module.GetType();
                    while(moduleType.BaseType != null && moduleType.BaseType != typeof(IDeviceModule))
                    {
                        moduleType = moduleType.BaseType;
                    }

                    msg.AddField(ModuleTypeFieldPrefix + moduleCounter, moduleType.ToString());
                    msg.AddField(ModuleObjectFieldPrefix + moduleCounter, module);

                    moduleCounter++;
                }
            }

            msg.AddField(NumberOfModulesField, moduleCounter);

            return msg;
        }
        #endregion

        #region Event Handler
        #region Network Event Handler
        private void AstralConnectionEstablished(object sender, ConnectionEventArgs e)
        {
            if (e.Connection != null
                && e.Connection.Equals(m_connection))
            {
                // send initialization
                Message msg = CreateInitializationMessage();
                m_connection.SendMessage(msg);

                // notify client
                ServiceEstablished?.Invoke(this, EventArgs.Empty);

                m_connection.MessageReceived += OnMessageReceived;
            }
        }

        private void AstralConnectionTerminated(object sender, ConnectionEventArgs e)
        {
            if (e.Connection != null
                && e.Connection.Equals(m_connection))
            {
                m_connection.MessageReceived -= OnMessageReceived;

                // notify client
                ServiceTerminated?.Invoke(this, EventArgs.Empty);
            }
        }

        private void AstralConnectionFailed(object sender, ConnectionEventArgs e)
        {
            if (e.Connection != null
                && e.Connection.Equals(m_connection))
            {
                m_connection.MessageReceived -= OnMessageReceived;

                // notify client
                ServiceFailed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnMessageReceived(object sender, Message msg)
        {
            if (msg != null)
            {
                bool messageHandled = false;

                // is this an initialization message?
                if (!(m_initialized)
                    && msg.Name.Equals(InitializationMessageName))
                {
                    HandleInitializationMessage(msg);
                    messageHandled = true;
                }

                if (messageHandled)
                {
                    return;
                }

                // get the handler (if there is one)
                if (msg.ContainsField(IDeviceModule.ModuleHandlerField))
                {
                    ModuleType type = (ModuleType)Enum.ToObject(typeof(ModuleType),
                        msg.GetIntField(IDeviceModule.ModuleHandlerField));

                  //  Debug.WriteLine("MODULE: " + type);

                    if (m_modules.ContainsKey(type))
                    {
                        m_modules[type].HandleModuleMessage(msg);
                        messageHandled = true;
                    }
                }

                if (messageHandled)
                {
                    return;
                }

                MessageReceived?.Invoke(this, msg);
            }
        }
        #endregion
        #endregion
    }
    #endregion
}
