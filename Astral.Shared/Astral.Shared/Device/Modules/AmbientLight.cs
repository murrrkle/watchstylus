using System;
using System.Collections.Generic;
using System.Text;

using Astral.Messaging;
using Astral.Net;
using Astral.Net.Serialization;

namespace Astral.Device
{
    #region Delegates
    public delegate void AstralAmbientLightEventHandler(object sender, AstralAmbientLightEventArgs e);
    #endregion

    #region Class 'AstralAmbientLightEventArgs'
    public class AstralAmbientLightEventArgs : EventArgs
    {
        #region Class Members
        private AmbientLightData m_lightData;
        #endregion

        #region Constructors
        internal AstralAmbientLightEventArgs(AmbientLightData lightData)
        {
            m_lightData = lightData;
        }
        #endregion

        #region Properties
        public AmbientLightData AmbientLightData
        {
            get { return m_lightData; }
        }
        #endregion
    }
    #endregion

    public class AmbientLight : IDeviceModule
    {
        #region Class Members
        private AmbientLightData m_prevData;
        #endregion

        #region Events
        private event AstralAmbientLightEventHandler InternalAmbientLightChanged;

        public event AstralAmbientLightEventHandler AmbientLightChanged
        {
            add
            {
                bool shouldStart = (InternalAmbientLightChanged == null
                    || (InternalAmbientLightChanged.GetInvocationList().Length == 0 && value != null));

                InternalAmbientLightChanged += value;

                if (shouldStart)
                {
                    Activate();
                }
            }
            remove
            {
                InternalAmbientLightChanged -= value;

                bool shouldStop = (InternalAmbientLightChanged == null
                    || InternalAmbientLightChanged.GetInvocationList().Length == 0);

                if (shouldStop)
                {
                    Deactive();
                }
            }
        }
        #endregion

        #region Constructors
        public AmbientLight()
            : base("AmbientLight")
        {

        }

        public AmbientLight(NetworkStreamInfo info)
            : base(info)
        {

        }
        #endregion

        #region Data Handling
        public void UpdateAmbientLightData(AmbientLightData ambientLightData)
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
                double deltaLight = Math.Abs(ambientLightData.AmbientLight - m_prevData.AmbientLight);
                shouldSend = (deltaLight >= Accuracy);
            }

            if (shouldSend)
            {
                // store the current value
                m_prevData = ambientLightData;

                Message ambientLightDataMessage = AmbientLightDataMessage.CreateInstance(ambientLightData);
                SendMessage(ambientLightDataMessage);
            }
        }
        #endregion

        #region Overrides (IDeviceModule)
        public override ModuleType Type => ModuleType.AmbientLight;

        internal override void GetStreamDataInternal(NetworkStreamInfo info)
        {

        }

        protected override void ProcessModuleMessage(Message msg)
        {
            AmbientLightData ambientLightData = AmbientLightDataMessage.ToAmbientLightData(msg);
            if (ambientLightData != null)
            {
                InternalAmbientLightChanged?.Invoke(this, new AstralAmbientLightEventArgs(ambientLightData));
            }
        }
        #endregion
    }
}
