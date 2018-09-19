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
    #region Class 'LiveSensorActivity'
    public class LiveSensorActivity : Activity, ISensorEventListener
    {
        #region Class Members
        #region Core Class Members
        private SensorManager m_sensorManager;
        #endregion

        #region Sensor Class Members
        private Dictionary<SensorType, IAndroidSensorBase> m_sensors;
        #endregion
        #endregion

        #region Android Startup
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // setup sensors
            m_sensors = new Dictionary<SensorType, IAndroidSensorBase>();

            AndroidAccelerometer accelerometer = new AndroidAccelerometer();
            AddHandlers(accelerometer);
            m_sensors.Add(accelerometer.SensorType, accelerometer);

            AndroidGyroscope gyroscope = new AndroidGyroscope();
            AddHandlers(gyroscope);
            m_sensors.Add(gyroscope.SensorType, gyroscope);

            AndroidCompass compass = new AndroidCompass();
            AddHandlers(compass);
            m_sensors.Add(compass.SensorType, compass);

            AndroidOrientation orientation = new AndroidOrientation();
            AddHandlers(orientation);
            m_sensors.Add(orientation.SensorType, orientation);

            AndroidLightSensor light = new AndroidLightSensor();
            AddHandlers(light);
            m_sensors.Add(light.SensorType, light);

            AndroidMagnetometer magnetometer = new AndroidMagnetometer();
            AddHandlers(magnetometer);
            m_sensors.Add(magnetometer.SensorType, magnetometer);

            // Keep screen on all the time
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);

            // get the sensor manager
            m_sensorManager = (SensorManager)GetSystemService(SensorService);
        }
        #endregion

        #region Event Handlers
        private void AddHandlers(IDeviceModule module)
        {
            module.Activated += OnDeviceModuleActivated;
            module.Deactivated += OnDeviceModuleDeactivated;
        }

        private void OnDeviceModuleActivated(object sender, EventArgs e)
        {
            m_sensorManager.RegisterListener(this, m_sensorManager.GetDefaultSensor((sender as IAndroidSensorBase).SensorType), SensorDelay.Normal);
        }

        private void OnDeviceModuleDeactivated(object sender, EventArgs e)
        {
            m_sensorManager.UnregisterListener(this, m_sensorManager.GetDefaultSensor((sender as IAndroidSensorBase).SensorType));
        }
        #endregion

        protected override void OnPause()
        { 
            base.OnPause();
            Console.WriteLine("in onPause");
        }

        protected override void OnStart()
        {
            base.OnStart();
            Console.WriteLine("in onStart");
        }

        protected override void OnResume()
        {
            base.OnResume();
            Console.WriteLine("in onResume");
        }

        protected override void OnStop()
        {
            base.OnStop();
            Console.WriteLine("in onStop");
        }
        
        #region Properties
        public List<IDeviceModule> SensorModules
        {
            get
            {
                IEnumerator<SensorType> enumerator = m_sensors.Keys.GetEnumerator();
                List<IDeviceModule> modules = new List<IDeviceModule>();

                while (enumerator.MoveNext())
                {
                    modules.Add(m_sensors[enumerator.Current] as IDeviceModule);
                }

                return modules;
            }
        }
        #endregion

        #region Overrides (ISensorEventListener)
        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
            // let's not do anything here
        }

        public void OnSensorChanged(SensorEvent e)
        {
            SensorType type = e.Sensor.Type;
            
            if (m_sensors.ContainsKey(type))
            {
                m_sensors[type].Update(e);
            }
        }
        #endregion
    }
    #endregion
}