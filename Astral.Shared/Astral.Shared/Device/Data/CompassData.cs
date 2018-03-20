using System;
using System.Collections.Generic;
using System.Text;

using Astral.Net.Serialization;

namespace Astral.Device
{
    #region Class 'CompassData'
    public class CompassData : ITransferable
    {
        #region Constant Class Members
        private const string HeadingField = "hdg";
        #endregion

        #region Class Members
        private double m_hdg;
        #endregion

        #region Constructors
        public CompassData(double heading)
        {
            m_hdg = heading;
        }

        public CompassData(NetworkStreamInfo info)
        {
            m_hdg = info.GetDouble(HeadingField);
        }
        #endregion

        #region Properties
        public double Heading
        {
            get { return m_hdg; }
            set { m_hdg = value; }
        }
        #endregion

        #region Overrides (ITransferable)
        public void GetStreamData(NetworkStreamInfo info)
        {
            info.AddValue(HeadingField, m_hdg);
        }
        #endregion
    }
    #endregion
}
