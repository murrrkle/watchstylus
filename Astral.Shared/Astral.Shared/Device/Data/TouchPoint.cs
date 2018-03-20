using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using Astral.Net.Serialization;

namespace Astral.Device
{
    #region Class 'TouchPoint'
    public class TouchPoint : ITransferable
    {
        #region Constant Class Members
        private const string TouchIdField = "toId";

        private const string TouchStateField = "toS";

        private const string TouchPositionField = "toP";
        #endregion

        #region Class Members
        private int m_id;

        private TouchState m_state;

        private double m_xPos;

        private double m_yPos;
        #endregion

        #region Constructors
        public TouchPoint(int id, TouchState state, double x, double y)
        {
            m_id = id;
            m_state = state;

            m_xPos = x;
            m_yPos = y;
        }

        public TouchPoint(NetworkStreamInfo info)
        {
            m_id = info.GetInt(TouchIdField);
            m_state = (TouchState)Enum.ToObject(
                typeof(TouchState), info.GetInt(TouchStateField));

            double[] position = (double[])info.GetValue(TouchPositionField, typeof(double[]));
            m_xPos = position[0];
            m_yPos = position[1];
        }
        #endregion

        #region Properties
        public int Id
        {
            get { return m_id; }
        }

        public TouchState State
        {
            get { return m_state; }
            set { m_state = value; }
        }

        public double X
        {
            get { return m_xPos; }
            set { m_xPos = value; }
        }

        public double Y
        {
            get { return m_yPos; }
            set { m_yPos = value; }
        }
        #endregion

        #region Overrides (ITransferable)
        public void GetStreamData(NetworkStreamInfo info)
        {
            info.AddValue(TouchIdField, m_id);
            info.AddValue(TouchStateField, (int)m_state);
            info.AddValue(TouchPositionField, new double[] { m_xPos, m_yPos });
        }
        #endregion
    }
    #endregion
}
