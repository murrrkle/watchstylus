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
    public class AndroidLightSensor : AmbientLight, IAndroidSensorBase
    {
        #region Implementation (IAndroidSensorBase)
        public SensorType SensorType => SensorType.Light;

        public void Update(SensorEvent e)
        {
            double light = e.Values[0];

            AmbientLightData lightData = new AmbientLightData(light);
            UpdateAmbientLightData(lightData);
        }
        #endregion
    }
}