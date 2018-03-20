using System;

using Android.Hardware;

namespace Astral.Droid.Sensors
{
    #region Interface 'IAndroidSensorBase'
    public interface IAndroidSensorBase
    {
        #region Properties
        SensorType SensorType { get; }
        #endregion

        #region Updates
        void Update(SensorEvent e);
        #endregion
    }
    #endregion
}
