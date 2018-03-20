using System;
using System.Collections.Generic;
using System.Text;

using Astral.Net.Serialization;

namespace Astral.Device
{
    #region Class 'AccelerationData'
    public class AccelerationData : ITransferable
    {
        #region Constant Class Members
        private const string AccelerationDataField = "acc";
        #endregion

        #region Class Members
        private double[] m_accData;
        #endregion

        #region Constructors
        public AccelerationData(double x, double y, double z)
        {
            m_accData = new double[] { x, y, z };
        }

        public AccelerationData(NetworkStreamInfo info)
        {
            m_accData = (double[])info.GetValue(AccelerationDataField, typeof(double[]));
        }
        #endregion

        #region Properties
        public double X
        {
            get { return m_accData[0]; }
            set { m_accData[0] = value; }
        }

        public double Y
        {
            get { return m_accData[1]; }
            set { m_accData[1] = value; }
        }

        public double Z
        {
            get { return m_accData[2]; }
            set { m_accData[2] = value; }
        }

        public double[] Data
        {
            get { return m_accData; }
        }
        #endregion

        #region Overrides (ITransferable)
        public void GetStreamData(NetworkStreamInfo info)
        {
            info.AddValue(AccelerationDataField, m_accData);
        }
        #endregion
    }
    #endregion
}
