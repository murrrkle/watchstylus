using System;
using System.Collections.Generic;
using System.Text;

using Astral.Net.Serialization;

namespace Astral.Device
{
    #region Class 'GyroscopeData'
    public class GyroscopeData : ITransferable
    {
        #region Constant Class Members
        private const string GyroscopeDataField = "gyro";
        #endregion

        #region Class Members
        private double[] m_gyroData;
        #endregion

        #region Constructors
        public GyroscopeData(double x, double y, double z)
        {
            m_gyroData = new double[] { x, y, z };
        }

        public GyroscopeData(NetworkStreamInfo info)
        {
            m_gyroData = (double[])info.GetValue(GyroscopeDataField, typeof(double[]));
        }
        #endregion

        #region Properties
        public double X
        {
            get { return m_gyroData[0]; }
            set { m_gyroData[0] = value; }
        }

        public double Y
        {
            get { return m_gyroData[1]; }
            set { m_gyroData[1] = value; }
        }

        public double Z
        {
            get { return m_gyroData[2]; }
            set { m_gyroData[2] = value; }
        }

        public double[] Data
        {
            get { return m_gyroData; }
        }
        #endregion

        #region Overrides (ITransferable)
        public void GetStreamData(NetworkStreamInfo info)
        {
            info.AddValue(GyroscopeDataField, m_gyroData);
        }
        #endregion
    }
    #endregion
}
