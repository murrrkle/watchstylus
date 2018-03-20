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
using Astral.Device;
using Android.Hardware;

namespace Astral.Droid.Sensors
{
    public class AndroidMagnetometer : Magnetometer, IAndroidSensorBase
    {
        #region Implementation (IAndroidSensorBase)
        public SensorType SensorType => SensorType.MagneticField;

        public void Update(SensorEvent e)
        {
            double x = e.Values[0];
            double y = e.Values[1];
            double z = e.Values[2];

            MagnetometerData magData = new MagnetometerData(x, y, z);

            UpdateMagnetometerData(magData);
        }
        #endregion
    }
}