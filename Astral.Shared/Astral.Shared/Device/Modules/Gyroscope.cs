using System;
using System.Collections.Generic;
using System.Text;

using Astral.Messaging;
using Astral.Net;
using Astral.Net.Serialization;

namespace Astral.Device
{
    #region Delegates
    public delegate void AstralGyroscopeEventHandler(object sender, AstralGyroscopeEventArgs e);
    #endregion

    #region Class 'AstralGyroscopeEventArgs'
    public class AstralGyroscopeEventArgs : EventArgs
    {
        #region Class Members
        private GyroscopeData m_gyroData;
        #endregion

        #region Constructors
        internal AstralGyroscopeEventArgs(GyroscopeData gyroData)
        {
            m_gyroData = gyroData;
        }
        #endregion

        #region Properties
        public GyroscopeData GyroscopeData
        {
            get { return m_gyroData; }
        }
        #endregion
    }
    #endregion

    #region Class 'Gyroscope'
    public class Gyroscope : IDeviceModule
    {
        #region Class Members

        #endregion

        #region Events
        private event AstralGyroscopeEventHandler InternalRotationChanged;

        public event AstralGyroscopeEventHandler RotationChanged
        {
            add
            {
                bool shouldStart = (InternalRotationChanged == null
                    || (InternalRotationChanged.GetInvocationList().Length == 0 && value != null));

                InternalRotationChanged += value;

                if (shouldStart)
                {
                    Activate();
                }
            }
            remove
            {
                InternalRotationChanged -= value;

                bool shouldStop = (InternalRotationChanged == null
                    || InternalRotationChanged.GetInvocationList().Length == 0);

                if (shouldStop)
                {
                    Deactive();
                }
            }
        }
        #endregion

        #region Constructors
        public Gyroscope()
            : base("Gyroscope")
        {

        }

        public Gyroscope(NetworkStreamInfo info)
            : base(info)
        {

        }
        #endregion

        #region Data Handling
        public void UpdateGyroscopeData(GyroscopeData gyroscopeData)
        {
            Message gyroscopeDataMsg = GyroscopeDataMessage.CreateInstance(gyroscopeData);
            SendMessage(gyroscopeDataMsg);
        }
        #endregion

        #region Overrides (IDeviceModule)
        public override ModuleType Type => ModuleType.Gyroscope;

        internal override void GetStreamDataInternal(NetworkStreamInfo info)
        {

        }

        protected override void ProcessModuleMessage(Message msg)
        {
            GyroscopeData gyroData = GyroscopeDataMessage.ToRotationData(msg);
            if (gyroData != null)
            {
                InternalRotationChanged?.Invoke(this, new AstralGyroscopeEventArgs(gyroData));
            }
        }
        #endregion
    }
    #endregion
}
