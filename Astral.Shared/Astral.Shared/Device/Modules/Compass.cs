using System;
using System.Collections.Generic;
using System.Text;

using Astral.Messaging;
using Astral.Net;
using Astral.Net.Serialization;

namespace Astral.Device
{
    #region Delegates
    public delegate void AstralCompassEventHandler(object sender, AstralCompassEventArgs e);
    #endregion

    #region Class 'AstralCompassEventArgs'
    public class AstralCompassEventArgs : EventArgs
    {
        #region Class Members
        private CompassData m_compassData;
        #endregion
        
        #region Constructors
        internal AstralCompassEventArgs(CompassData compassData)
        {
            m_compassData = compassData;
        }
        #endregion

        #region Properties
        public CompassData CompassData
        {
            get { return m_compassData; }
        }
        #endregion
    }
    #endregion

    #region Class 'Compass'
    public class Compass : IDeviceModule
    {
        #region Class Members
        private CompassData m_prevData;
        #endregion

        #region Events
        private event AstralCompassEventHandler InternalHeadingChanged;

        public event AstralCompassEventHandler HeadingChanged
        {
            add
            {
                bool shouldStart = (InternalHeadingChanged == null
                    || (InternalHeadingChanged.GetInvocationList().Length == 0 && value != null));

                InternalHeadingChanged += value;

                if (shouldStart)
                {
                    Activate();
                }
            }
            remove
            {
                InternalHeadingChanged -= value;

                bool shouldStop = (InternalHeadingChanged == null
                    || InternalHeadingChanged.GetInvocationList().Length == 0);

                if (shouldStop)
                {
                    Deactive();
                }
            }
        }
        #endregion

        #region Constructors
        public Compass()
            : base("Compass")
        { }

        public Compass(NetworkStreamInfo info)
            : base(info)
        { }
        #endregion

        #region Data Handling
        public void UpdateCompassData(CompassData compassData)
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
                double deltaHdg = Math.Abs(compassData.Heading - m_prevData.Heading);
                shouldSend = (deltaHdg >= Accuracy);
            }

            if (shouldSend)
            {
                // store the current value
                m_prevData = compassData;

                Message compassDataMsg = CompassDataMessage.CreateInstance(compassData);
                SendMessage(compassDataMsg);
            }
        }
        #endregion

        #region Overrides (IDeviceModule)
        public override ModuleType Type => ModuleType.Compass;

        internal override void GetStreamDataInternal(NetworkStreamInfo info)
        {

        }

        protected override void ProcessModuleMessage(Message msg)
        {
            CompassData compassData = CompassDataMessage.ToCompassData(msg);
            if (compassData != null)
            {
                InternalHeadingChanged?.Invoke(this, new AstralCompassEventArgs(compassData));
            }
        }
        #endregion
    }
    #endregion
}
