using System;
using System.Collections.Generic;
using System.Text;
using Astral.Net.Serialization;


namespace Astral.Device
{
    public class AmbientLightData : ITransferable
    {
        #region Constant Class Members
        private const string AmbientLightField = "aLight";
        #endregion

        #region Class Members
        private double m_ambientLight;
        #endregion

        #region Constructors
        public AmbientLightData(double value)
        {
            m_ambientLight = value;
        }

        public AmbientLightData(NetworkStreamInfo info)
        {
            m_ambientLight = info.GetDouble(AmbientLightField);
        }
        #endregion

        #region Properties
        public double AmbientLight
        {
            get { return m_ambientLight; }
            set { m_ambientLight = value; }
        }
        #endregion

        #region Overrides (ITransferable)
        public void GetStreamData(NetworkStreamInfo info)
        {
            info.AddValue(AmbientLightField, m_ambientLight);
        }
        #endregion
    }
}
