using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Astral.Net.Serialization;

namespace Astral.Device
{
    #region Class 'MicrophoneData'
    public class MicrophoneData : ITransferable
    {
        #region Constant Class Members
        private const string MicrophoneDataField = "mic";
        #endregion

        #region Class Members
        private short[] m_micData;
        #endregion

        #region Constructors
        public MicrophoneData(short[] buffer)
        {
            m_micData = buffer;
        }

        public MicrophoneData(NetworkStreamInfo info)
        {
            m_micData = (short[])info.GetValue(MicrophoneDataField, typeof(short[]));
        }
        #endregion

        #region Properties
        public short[] Data
        {
            get { return m_micData; }
        }

        public double Amplitude
        {
            get { return Math.Abs(Array.ConvertAll(m_micData, s => (int)s).Sum() / m_micData.Length); }
        }


        #endregion

        #region Overrides (ITransferable)
        public void GetStreamData(NetworkStreamInfo info)
        {
            info.AddValue(MicrophoneDataField, m_micData);
        }
        #endregion
    }
    #endregion
}
