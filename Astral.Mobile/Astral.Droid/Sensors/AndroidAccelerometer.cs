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
    #region Class 'AndroidAccelerometer'
    public class AndroidAccelerometer : Accelerometer, IAndroidSensorBase
    {
        #region Implementation (IAndroidSensorBase)
        public SensorType SensorType => SensorType.Accelerometer;

        public void Update(SensorEvent e)
        {
            double x = e.Values[0];
            double y = e.Values[1];
            double z = e.Values[2];

            AccelerationData accData = new AccelerationData(x, y, z);
            UpdateAccelerationData(accData);
        }
        #endregion
    }
    #endregion
}