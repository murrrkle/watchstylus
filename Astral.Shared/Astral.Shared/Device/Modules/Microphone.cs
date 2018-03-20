using System;
using System.Collections.Generic;
using System.Text;

using Astral.Messaging;
using Astral.Net;
using Astral.Net.Serialization;

namespace Astral.Device
{
    #region Delegates
    public delegate void AstralMicrophoneEventHandler(object sender, AstralMicrophoneEventArgs e);
    #endregion

    #region Class 'AstralMicrophoneEventArgs'
    public class AstralMicrophoneEventArgs : EventArgs
    {
        #region Class Members
        private MicrophoneData m_micData;
        #endregion

        #region Constructors
        internal AstralMicrophoneEventArgs(MicrophoneData micData)
        {
            m_micData = micData;
        }
        #endregion

        #region Properties
        public MicrophoneData MicrophoneData
        {
            get { return m_micData; }
        }
        #endregion
    }
    #endregion

    #region Enumerations
    [Flags]
    public enum SamplingRate
    {
        Minimum = 8000,
        Low = 11025,
        Medium = 16000,
        High = 22050,
        Maximum = 44100
    }
    #endregion

    #region Class 'Microphone'
    public class Microphone : IDeviceModule
    {
        #region Constant Class Members
        private const string SamplingRateField = "samR";
        #endregion

        #region Class Members
        private SamplingRate m_samplingRate;
        #endregion

        #region Events
        private event AstralMicrophoneEventHandler InternalMicrophoneUpdated;

        public event AstralMicrophoneEventHandler MicrophoneUpdated
        {
            add
            {
                bool shouldStart = (InternalMicrophoneUpdated == null
                    || (InternalMicrophoneUpdated.GetInvocationList().Length == 0 && value != null));

                InternalMicrophoneUpdated += value;

                if (shouldStart)
                {
                    Activate();
                }
            }
            remove
            {
                InternalMicrophoneUpdated -= value;

                bool shouldStop = (InternalMicrophoneUpdated == null
                    || InternalMicrophoneUpdated.GetInvocationList().Length == 0);

                if (shouldStop)
                {
                    Deactive();
                }
            }
        }
        #endregion

        #region Constructors
        public Microphone(SamplingRate samplingRate = SamplingRate.High)
            : base("Microphone")
        {
            m_samplingRate = samplingRate;
        }

        public Microphone(NetworkStreamInfo info)
            : base(info)
        {
            m_samplingRate = (SamplingRate)Enum.ToObject(
                typeof(SamplingRate), info.GetInt(SamplingRateField));
        }
        #endregion

        #region Properties
        public SamplingRate SamplingRate
        {
            get { return m_samplingRate; }
        }
        #endregion

        #region Data Handling
        public void UpdateMicrophoneData(MicrophoneData microphoneData)
        {
            Message microphoneDataMsg = MicrophoneDataMessage.CreateInstance(microphoneData);
            SendMessage(microphoneDataMsg);
        }
        #endregion

        #region Overrides (IDeviceModule)
        public override ModuleType Type => ModuleType.Microphone;

        internal override void GetStreamDataInternal(NetworkStreamInfo info)
        {
            info.AddValue(SamplingRateField, (int)m_samplingRate);
        }

        protected override void ProcessModuleMessage(Message msg)
        {
            MicrophoneData micData = MicrophoneDataMessage.ToMicrophoneData(msg);
            if (micData != null)
            {
                InternalMicrophoneUpdated?.Invoke(this, new AstralMicrophoneEventArgs(micData));
            }
        }
        #endregion
    }
    #endregion
}
