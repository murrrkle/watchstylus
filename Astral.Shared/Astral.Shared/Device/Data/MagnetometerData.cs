using System;
using System.Collections.Generic;
using System.Text;

using Astral.Net.Serialization;

namespace Astral.Device
{
    public class MagnetometerData : ITransferable
    {
        #region Constant Class Members
        private const string MagnetometerDataField = "mag";
        #endregion

        #region Class Members
        private double[] m_magData;
        #endregion

        #region Constructors
        public MagnetometerData(double x, double y, double z)
        {
            m_magData = new double[] { x, y, z };
        }

        public MagnetometerData(NetworkStreamInfo info)
        {
            m_magData = (double[])info.GetValue(MagnetometerDataField, typeof(double[]));
        }
        #endregion

        #region Properties
        public double X
        {
            get { return m_magData[0]; }
            set { m_magData[0] = value; }
        }

        public double Y
        {
            get { return m_magData[1]; }
            set { m_magData[1] = value; }
        }

        public double Z
        {
            get { return m_magData[2]; }
            set { m_magData[2] = value; }
        }
        #endregion

        #region Overrides (ITransferable)
        public void GetStreamData(NetworkStreamInfo info)
        {
            info.AddValue(MagnetometerDataField, m_magData);
        }
        #endregion
    }
}
