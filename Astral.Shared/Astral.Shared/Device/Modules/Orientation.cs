using System;
using System.Collections.Generic;
using System.Text;

using Astral.Messaging;
using Astral.Net;
using Astral.Net.Serialization;

namespace Astral.Device
{
    #region Delegates
    public delegate void AstralOrientationEventHandler(object sender, AstralOrientationEventArgs e);
    #endregion

    #region Class 'AstralOrientationEventArgs'
    public class AstralOrientationEventArgs : EventArgs
    {
        #region Class Members
        private OrientationData m_oriData;
        #endregion

        #region Constructors
        internal AstralOrientationEventArgs(OrientationData oriData)
        {
            m_oriData = oriData;
        }
        #endregion

        #region Properties
        public OrientationData OrientationData
        {
            get { return m_oriData; }
        }
        #endregion
    }
    #endregion

    public class Orientation : IDeviceModule
    {
        #region Class Members
        private OrientationData m_prevData;
        #endregion

        #region Events
        private event AstralOrientationEventHandler InternalOrientationChanged;

        public event AstralOrientationEventHandler OrientationChanged
        {
            add
            {
                bool shouldStart = (InternalOrientationChanged == null
                    || (InternalOrientationChanged.GetInvocationList().Length == 0 && value != null));

                InternalOrientationChanged += value;

                if (shouldStart)
                {
                    Activate();
                }
            }
            remove
            {
                InternalOrientationChanged -= value;

                bool shouldStop = (InternalOrientationChanged == null
                    || InternalOrientationChanged.GetInvocationList().Length == 0);

                if (shouldStop)
                {
                    Deactive();
                }
            }
        }
        #endregion

        #region Constructors
        public Orientation()
            : base("Orientation")
        {

        }

        public Orientation(NetworkStreamInfo info)
            : base(info)
        {

        }
        #endregion

        #region Data Handling
        public void UpdateOrientationData(OrientationData orientationData)
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
                double deltaRoll = Math.Abs(orientationData.RollDegrees - m_prevData.RollDegrees);
                double deltaPitch = Math.Abs(orientationData.PitchDegrees - m_prevData.PitchDegrees);
                double deltaYaw = Math.Abs(orientationData.YawDegrees - m_prevData.YawDegrees);

                shouldSend = (deltaRoll >= Accuracy
                    || deltaPitch >= Accuracy
                    || deltaYaw >= Accuracy);
            }

            if (shouldSend)
            {
                // store the current value
                m_prevData = orientationData;

                Message orientationDataMessage = OrientationDataMessage.CreateInstance(orientationData);
                SendMessage(orientationDataMessage);
            }
        }
        #endregion

        #region Overrides (IDeviceModule)
        public override ModuleType Type => ModuleType.Orientation;

        internal override void GetStreamDataInternal(NetworkStreamInfo info)
        {

        }

        protected override void ProcessModuleMessage(Message msg)
        {
            OrientationData oriData = OrientationDataMessage.ToOrientationData(msg);
            if (oriData != null)
            {
                InternalOrientationChanged?.Invoke(this, new AstralOrientationEventArgs(oriData));
            }
        }
        #endregion
    }
}
