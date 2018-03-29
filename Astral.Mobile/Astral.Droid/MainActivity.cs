using System;
using System.Net;
using System.Drawing;

using Android.App;
using Android.Widget;
using Android.OS;

using Astral.Device;
using Android.Content;
using Android.Bluetooth;

using Astral.Droid.Media;
using Astral.Droid.Sensors;
using Astral.Droid.UI;


namespace Astral.Droid
{
    // NOTE: had to add potrait orientation: if that's not there, an orientation change recreates the activity!
    [Activity(Label = "Astral.Droid", MainLauncher = true, Icon = "@mipmap/icon", Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : LiveSensorActivity
    {
        #region Class Members
        private AstralDevice m_device;
        #endregion

        #region Android Starup
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // initialize Device
            InitializeDevice();
        }

        protected override void OnStart()
        {
            base.OnStart();

            InitializeAstral();
            m_device.Start();
        }
        #endregion

        #region Initialization
        private void InitializeDevice()
        {
            string deviceClass = Build.Model;
            string deviceName = "Astral Device";

            m_device = new AstralDevice(deviceClass, deviceName);

            // display
            Display display = new Display(new Size(
                Resources.DisplayMetrics.WidthPixels,
                Resources.DisplayMetrics.HeightPixels),
                DeviceOrientation.Portrait, TouchCapabilities.Multi);
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
            ScreenshotView screenshotView = FindViewById<ScreenshotView>(Resource.Id.ScreenshotView);

            // add the corresponding handlers to the views
            screenshotView.Screen = m_device[ModuleType.Display] as Display;

            // TODO: THIS IS HARDCODED
            string ipAddress = "10.101.34.110";
            //string ipAddress = "192.168.0.10";

            // David's IP
            // string ipAddress = "192.168.0.15";

            //string ipAddress = "192.168.0.23";
            // iLab one
            //string ipAddress = "192.168.1.102";
            //string ipAddress = "192.168.137.1";
            //string ipAddress = "10.11.106.246";
            // emulator link to host = 10.0.2.2; see <https://developer.android.com/studio/run/emulator-networking.html>
            //string ipAddress = "10.0.2.2";
            int port = 10001;

            m_device.Connect(IPAddress.Parse(ipAddress), port);
            m_device.MessageReceived += AstralMessageReceived;
        }
        #endregion

        #region Event Handler
        private void AstralMessageReceived(object sender, Net.Message msg)
        {
            if (msg != null)
            {
                // get the message name
                string msgName = msg.Name;
                switch (msgName)
                {
                    default:
                        break;
                }
            }
        }
        #endregion
    }
}

