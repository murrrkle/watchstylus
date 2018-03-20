using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Astral.Device;

namespace Astral.Droid.Sensors
{
    #region Class 'AndroidCompass'
    public class AndroidCompass : Compass, IAndroidSensorBase
    {
        #region Class Members
        private float[] m_orientation = new float[3];

        private float[] m_rotationMatrix = new float[9];
        #endregion

        #region Conversion
        private double ToDegrees(double radians)
        {
            return (radians / Math.PI * 180.0);
        }
        #endregion

        #region Implementation (IAndroidSensorBase)
        public SensorType SensorType => SensorType.GeomagneticRotationVector;

        public void Update(SensorEvent e)
        {
            SensorManager.GetRotationMatrixFromVector(m_rotationMatrix, e.Values.ToArray());
            float[] orientation = SensorManager.GetOrientation(m_rotationMatrix, m_orientation);

            double newHeading = (ToDegrees(orientation[0]) + 360.0) % 360.0;

            CompassData compassData = new CompassData(newHeading);
            UpdateCompassData(compassData);
        }
        #endregion
    }
    #endregion
}