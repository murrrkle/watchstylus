using System;
using System.Collections.Generic;
using System.Text;

using Astral.Messaging;
using Astral.Net;
using Astral.Net.Serialization;

namespace Astral.Device
{
    #region Delegates
    public delegate void AstralAccelerometerEventHandler(object sender, AstralAccelerometerEventArgs e);
    #endregion

    #region Class 'AstralAccelerometerEventArgs'
    public class AstralAccelerometerEventArgs : EventArgs
    {
        #region Class Members
        private AccelerationData m_accData;
        #endregion

        #region Constructors
        internal AstralAccelerometerEventArgs(AccelerationData accData)
        {
            m_accData = accData;
        }
        #endregion

        #region Properties
        public AccelerationData AccelerationData
        {
            get { return m_accData; }
        }
        #endregion
    }
    #endregion

    #region Class 'Accelerometer'
    public class Accelerometer : IDeviceModule
    {
        #region Class Members

        #endregion

        #region Events
        private event AstralAccelerometerEventHandler InternalAccelerationChanged;

        public event AstralAccelerometerEventHandler AccelerationChanged
        {
            add
            {
                bool shouldStart = (InternalAccelerationChanged == null
                    || (InternalAccelerationChanged.GetInvocationList().Length == 0 && value != null));

                InternalAccelerationChanged += value;

                if (shouldStart)
                {
                    Activate();
                }
            }
            remove
            {
                InternalAccelerationChanged -= value;

                bool shouldStop = (InternalAccelerationChanged == null
                    || InternalAccelerationChanged.GetInvocationList().Length == 0);

                if (shouldStop)
                {
                    Deactive();
                }
            }
        }
        #endregion

        #region Constructors
        public Accelerometer()
            : base("Accelerometer")
        {

        }

        public Accelerometer(NetworkStreamInfo info)
            : base(info)
        {

        }
        #endregion

        #region Data Handling
        public void UpdateAccelerationData(AccelerationData accelerationData)
        {
            Message accelerationDataMsg = AccelerationDataMessage.CreateInstance(accelerationData);
            SendMessage(accelerationDataMsg);
        }
        #endregion

        #region Overrides (IDeviceModule)
        public override ModuleType Type => ModuleType.Accelerometer;

        internal override void GetStreamDataInternal(NetworkStreamInfo info)
        {

        }

        protected override void ProcessModuleMessage(Message msg)
        {
            AccelerationData accData = AccelerationDataMessage.ToAccelerationData(msg);
            if (accData != null)
            {
                InternalAccelerationChanged?.Invoke(this, new AstralAccelerometerEventArgs(accData));
            }
        }
        #endregion
    }
    #endregion
}
