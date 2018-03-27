using System;
using System.Collections.Generic;
using System.Text;

using Astral.Messaging;
using Astral.Net;
using Astral.Net.Serialization;


namespace Astral.Device
{
    #region Delegates
    public delegate void AstralMagnetometerEventHandler(object sender, AstralMagnetometerEventArgs e);
    #endregion

    #region Class 'AstralMagnetometerEventArgs'
    public class AstralMagnetometerEventArgs : EventArgs
    {
        #region Class Members
        private MagnetometerData m_magData;
        #endregion

        #region Constructors
        internal AstralMagnetometerEventArgs(MagnetometerData magData)
        {
            m_magData = magData;
        }
        #endregion

        #region Properties
        public MagnetometerData MagnetometerData
        {
            get { return m_magData; }
        }
        #endregion
    }
    #endregion

    public class Magnetometer : IDeviceModule
    {
        #region Class Members
        private MagnetometerData m_prevData;
        #endregion

        #region Events
        private event AstralMagnetometerEventHandler InternalMagnetometerChanged;

        public event AstralMagnetometerEventHandler MagnetometerChanged
        {
            add
            {
                bool shouldStart = (InternalMagnetometerChanged == null
                    || (InternalMagnetometerChanged.GetInvocationList().Length == 0 && value != null));

                InternalMagnetometerChanged += value;

                if (shouldStart)
                {
                    Activate();
                }
            }
            remove
            {
                InternalMagnetometerChanged -= value;

                bool shouldStop = (InternalMagnetometerChanged == null
                    || InternalMagnetometerChanged.GetInvocationList().Length == 0);

                if (shouldStop)
                {
                    Deactive();
                }
            }
        }
        #endregion

        #region Constructors
        public Magnetometer()
            : base("Magnetometer")
        {

        }

        public Magnetometer(NetworkStreamInfo info)
            : base(info)
        {

        }
        #endregion

        #region Data Handling
        public void UpdateMagnetometerData(MagnetometerData magnetometerData)
        {
            // first check whether we should even send it
            bool shouldSend = true;
            if (m_prevData == null
                || Accuracy <= 0.0)
            {
                shouldSend = true;
            }
            else
            {
                double deltaX = Math.Abs(magnetometerData.X - m_prevData.X);
                double deltaY = Math.Abs(magnetometerData.Y - m_prevData.Y);
                double deltaZ = Math.Abs(magnetometerData.Z - m_prevData.Z);

                shouldSend = (deltaX >= Accuracy
                    || deltaY >= Accuracy
                    || deltaZ >= Accuracy);
            }

            if (shouldSend)
            {
                // store the current value
                m_prevData = magnetometerData;

                Message magnetometerDataMessage = MagnetometerDataMessage.CreateInstance(magnetometerData);
                SendMessage(magnetometerDataMessage);
            }
        }
        #endregion

        #region Overrides (IDeviceModule)
        public override ModuleType Type => ModuleType.Magnetometer;

        internal override void GetStreamDataInternal(NetworkStreamInfo info)
        {

        }

        protected override void ProcessModuleMessage(Message msg)
        {
            MagnetometerData magData = MagnetometerDataMessage.ToMagnetometerData(msg);
            if (magData != null)
            {
                InternalMagnetometerChanged?.Invoke(this, new AstralMagnetometerEventArgs(magData));
            }
        }
        #endregion
    }
}
