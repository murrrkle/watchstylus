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
    #region Class 'AndroidGyroscope'
    public class AndroidGyroscope : Gyroscope, IAndroidSensorBase
    {
        #region Implementation (IAndroidSensorBase)
        public SensorType SensorType => SensorType.Gyroscope;

        public void Update(SensorEvent e)
        {
            double x = e.Values[0];
            double y = e.Values[1];
            double z = e.Values[2];

            GyroscopeData gyroData = new GyroscopeData(x, y, z);
            UpdateGyroscopeData(gyroData);
        }
        #endregion
    }
    #endregion
}