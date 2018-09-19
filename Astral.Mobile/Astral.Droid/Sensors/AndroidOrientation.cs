using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Hardware;

using Astral.Device;

namespace Astral.Droid.Sensors
{
    public class AndroidOrientation : Device.Orientation, IAndroidSensorBase
    {
        #region Implementation (IAndroidSensorBase)
        public SensorType SensorType => SensorType.RotationVector;

        public void Update(SensorEvent e)
        {
            double x = e.Values[0];
            double y = e.Values[1];
            double z = e.Values[2];
            double w = e.Values[3];

            float[] rotationMatrix = new float[9];
            float[] eulerAngles = new float[3];
            
            float[] vector = new float[] { e.Values[0], e.Values[1], e.Values[2], e.Values[3] };
            SensorManager.GetRotationMatrixFromVector(rotationMatrix, vector);
            SensorManager.GetOrientation(rotationMatrix, eulerAngles);
            
            // we convert everything to doubles to be consistent with iOS and C# usage of doubles instead of floats by default
            double[] quaternion = new double[4] { e.Values[0], e.Values[1], e.Values[2], e.Values[3] };

            double[] rotationMatrixDouble = new double[9] { rotationMatrix[0], rotationMatrix[1], rotationMatrix[2],
                                                            rotationMatrix[3], rotationMatrix[4], rotationMatrix[5],
                                                            rotationMatrix[6], rotationMatrix[7], rotationMatrix[8] };

            double[] eulerAnglesDouble = new double[3] { eulerAngles[0], eulerAngles[1], eulerAngles[2] };

            OrientationData orientationData = new OrientationData(quaternion, rotationMatrixDouble, eulerAnglesDouble);
            UpdateOrientationData(orientationData);
        }
        #endregion

    }
}