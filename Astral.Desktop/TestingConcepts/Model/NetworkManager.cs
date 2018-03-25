using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Astral;
using Astral.Device;

namespace TestingConcepts
{
    public class DeviceConnectedEventArgs : EventArgs
    {
        public AstralDevice Device { get; set; }
        public AstralSession Session { get; set; }

        public DeviceConnectedEventArgs(AstralDevice device, AstralSession session)
        {
            this.Device = device;
            this.Session = session;
        }
    }

    public class NetworkManager
    {
        private static NetworkManager instance = null;

        public static NetworkManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NetworkManager();
                }
                return instance;
            }
        }


        private AstralService m_service;
        private AstralSession m_session;
        private AstralDevice m_device;

        public event EventHandler<DeviceConnectedEventArgs> DeviceAdded;

        private NetworkManager()
        {

        }

        public void Start()
        {
            InitializeService();
        }

        public void Stop()
        {
            this.m_service.Stop();
        }

        private void RaiseDeviceAdded(DeviceConnectedEventArgs e)
        {
            DeviceAdded?.Invoke(this, e);
        }

        private void InitializeService()
        {
            m_session = null;

            m_service = AstralService.GetInstance();
            m_service.SessionEstablished += AstralSessionEstablished;
            m_service.SessionTerminated += AstralSessionTerminated;

            m_service.Start();
        }

        private void AstralSessionTerminated(object sender, AstralSession session)
        {

        }

        private void AstralSessionEstablished(object sender, AstralSession session)
        {
            if (session != null
                && m_session == null)
            {
                m_session = session;

                // this.m_device = m_session.Device;
                RaiseDeviceAdded(new DeviceConnectedEventArgs(session.Device, session));
            }
        }
    }
}
