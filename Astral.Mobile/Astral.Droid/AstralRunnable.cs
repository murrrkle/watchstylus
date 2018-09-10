using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Astral.Device;
using Astral.Droid.Sensors;
using Astral.Droid.Media;
using Java.Lang;
using Android.Hardware;
using System.Net;
using Astral.Net;

namespace Astral.Droid
{
     class AstralRunnable : Java.Lang.Object, IRunnable, ISensorEventListener
    {
        Context mContext;
        private AstralDevice m_device;
        private SensorManager m_sensorManager;

        private string debugTag;

        private Dictionary<Android.Hardware.SensorType, IAndroidSensorBase> m_sensors;

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

        private int displayWidth, displayHeight;

        public AstralRunnable(Context c, int w, int h)
        {
            mContext = c;
            displayWidth = w;
            displayHeight = h;

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
            //Window.AddFlags(WindowManagerFlags.KeepScreenOn);

            // get the sensor manager
            m_sensorManager = (SensorManager)mContext.GetSystemService(Context.SensorService);
        }

        public void Run()
        {
            InitializeDevice();
            InitializeAstral();
            m_device.Start();
        }

        private void InitializeDevice()
        {
            string deviceClass = Build.Model;
            string deviceName = "Astral Device";

            m_device = new AstralDevice(deviceClass, deviceName);

            // display
            Astral.Device.Display display = new Astral.Device.Display(new System.Drawing.Size(
                displayWidth,
                displayHeight),
                DeviceOrientation.Portrait, TouchCapabilities.Multi,
                ConnectivityType.RequestResponse);
            m_device.AddModule(display);

            //microhpone
            AndroidMicrophone microphone = new AndroidMicrophone();
            m_device.AddModule(microphone);

            // add sensors
            foreach (IDeviceModule module in SensorModules)
            {
                m_device.AddModule(module);
            }
        }

        private void InitializeAstral()
        {
            //ScreenshotView screenshotView = FindViewById<ScreenshotView>(Resource.Id.ScreenshotView);

            // add the corresponding handlers to the views

            //screenshotView.Screen = m_device[ModuleType.Display] as Astral.Device.Display;

            
            string ipAddress = "192.168.0.27";
            int port = 10001;

            m_device.Connect(IPAddress.Parse(ipAddress), port);
            m_device.MessageReceived += AstralMessageReceived;


        }

        private void AstralMessageReceived(object sender, Net.Message msg)
        {
            
        }

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

        #region ISensorListener Overrides
        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
            // nothing
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
}