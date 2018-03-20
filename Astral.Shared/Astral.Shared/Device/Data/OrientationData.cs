using System;
using System.Collections.Generic;
using System.Text;

using Astral.Net.Serialization;

namespace Astral.Device
{
    public class OrientationData : ITransferable
    {
        #region Constant Class Members
        private const string QuaternionWField = "qW";
        private const string QuaternionXField = "qX";
        private const string QuaternionYField = "qY";
        private const string QuaternionZField = "qZ";

        private const string PitchField = "pitch";
        private const string YawField = "yaw";
        private const string RollField = "roll";

        private const string RotationMatrixField = "rotationMatrix";
        #endregion

        #region Class Members
        private double m_qw;
        private double m_qx;
        private double m_qy;
        private double m_qz;

        private double m_yaw;
        private double m_pitch;
        private double m_roll;

        private double[] m_rotationMatrix;
        #endregion

        #region Constructors
        public OrientationData(double[] quaternion, double[] rotationMatrix, double[] eulerAngles)
        {
            // order for quaternion on Android is x y z w
            m_qx = quaternion[0];
            m_qy = quaternion[1];
            m_qz = quaternion[2];
            m_qw = quaternion[3];

            // another note: we don't know if the order is actually yaw, pitch, roll
            m_yaw = eulerAngles[0];
            m_pitch = eulerAngles[1];
            m_roll = eulerAngles[2];

            // originally a float array, maybe convert
            m_rotationMatrix = rotationMatrix;
        }

        public OrientationData(NetworkStreamInfo info)
        {
            m_qw = info.GetDouble(QuaternionWField);
            m_qx = info.GetDouble(QuaternionXField);
            m_qy = info.GetDouble(QuaternionYField);
            m_qz = info.GetDouble(QuaternionZField);
            
            m_yaw = info.GetDouble(YawField);
            m_pitch = info.GetDouble(PitchField);
            m_roll = info.GetDouble(RollField);

            // SEBASTIAN PLEASE FIX THIS :)
            // we want to be able to retrieve the rotation matrix (as double array? Android default is float array though, iOS uses double array)
            // or maybe even convert this into C# Matrix object at some point somewhere...
            // m_rotationMatrix = info.GetValue(RotationMatrixField, m_rotationMatrix.GetType() as object);

            m_rotationMatrix = (double[])info.GetValue(RotationMatrixField, typeof(double[]));
        }
        #endregion

        #region Properties

        public double QuaternionW { get => m_qw; set => m_qw = value; }
        public double QuaternionX { get => m_qx; set => m_qx = value; }
        public double QuaternionY { get => m_qy; set => m_qy = value; }
        public double QuaternionZ { get => m_qz; set => m_qz = value; }
        public double Yaw { get => m_yaw; set => m_yaw = value; }
        public double Pitch { get => m_pitch; set => m_pitch = value; }
        public double Roll { get => m_roll; set => m_roll = value; }
        public double[] RotationMatrix { get => m_rotationMatrix; set => m_rotationMatrix = value; }
        public double YawDegrees { get => (Yaw * 180 / Math.PI); }
        public double PitchDegrees { get => (Pitch * 180 / Math.PI); }
        public double RollDegrees { get => (Roll * 180 / Math.PI); }

        #endregion

        #region Overrides (ITransferable)
        public void GetStreamData(NetworkStreamInfo info)
        {
            info.AddValue(QuaternionWField, m_qw);
            info.AddValue(QuaternionXField, m_qx);
            info.AddValue(QuaternionYField, m_qy);
            info.AddValue(QuaternionZField, m_qz);

            info.AddValue(YawField, m_yaw);
            info.AddValue(PitchField, m_pitch);
            info.AddValue(RollField, m_roll);

            info.AddValue(RotationMatrixField, m_rotationMatrix);
        }
        #endregion
    }
}
