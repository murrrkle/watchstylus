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
using Android.Views;
using Android.Util;
using Android.Graphics;

namespace Astral.Droid
{
    // NOTE: had to add potrait orientation: if that's not there, an orientation change recreates the activity!
    [Activity(Label = "Astral.Droid", MainLauncher = true, Icon = "@mipmap/icon", Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : LiveSensorActivity
    {


        #region Class Members
        public enum BrushTypes
        {
            BRUSH = 0,
            ERASER = 1,
            STAMP = 2,
            AIRBRUSH = 3
        }

        private AstralDevice m_device;
        Vibrator vibrator;
        private Android.Views.GestureDetector gestureDetector;
        private BrushTypes currentTool;

        private string debugTag;
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
            vibrator = (Vibrator)this.ApplicationContext.GetSystemService(Context.VibratorService);
            currentTool = BrushTypes.BRUSH;
            debugTag = "VS DEBUG";
        }
        #endregion

        #region Initialization
        private void InitializeDevice()
        {
            string deviceClass = Build.Model;
            string deviceName = "Astral Device";

            m_device = new AstralDevice(deviceClass, deviceName);

            // display
            Astral.Device.Display display = new Astral.Device.Display(new System.Drawing.Size(
                Resources.DisplayMetrics.WidthPixels,
                Resources.DisplayMetrics.HeightPixels),
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
            ScreenshotView screenshotView = FindViewById<ScreenshotView>(Resource.Id.ScreenshotView);

            // add the corresponding handlers to the views
            screenshotView.Screen = m_device[ModuleType.Display] as Astral.Device.Display;

            // TODO: THIS IS HARDCODED
            //string ipAddress = "10.101.34.110";
            //string ipAddress = "192.168.0.10";

            // David's IP
            string ipAddress = "192.168.0.27";

            //string ipAddress = "192.168.0.27";
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
            Log.Info(debugTag, "RECEIVED ASTRAL MESSAGE" );
            if (msg != null)
            {
                // get the message name
                string msgName = msg.Name;
                switch (msgName)
                {

                    case "ChangeTool":
                        switch (msg.GetIntField("Type"))
                        {
                            case (int) BrushTypes.BRUSH:
                                vibrator.Vibrate(50);
                                currentTool = BrushTypes.BRUSH;
                                //SetContentView(Resource.Layout.BrushUI);
                                // load brush UI 
                                Intent intent = new Intent(this, typeof(BrushActivity));
                                this.StartActivity(intent);
                                Log.Info(debugTag, "Loading BRUSH tool");
                                break;

                            case (int)BrushTypes.ERASER:
                                vibrator.Vibrate(50);
                                currentTool = BrushTypes.ERASER;
                                // load brush UI 
                                Log.Info(debugTag, "Loading ERASER tool");
                                break;

                            case (int)BrushTypes.AIRBRUSH:
                                vibrator.Vibrate(50);
                                currentTool = BrushTypes.AIRBRUSH;
                                // load brush UI 
                                Log.Info(debugTag, "Loading AIRBRUSH tool");
                                break;

                            case (int)BrushTypes.STAMP:
                                vibrator.Vibrate(50);
                                //SetContentView(Resource.Layout.Main);
                                currentTool = BrushTypes.STAMP;
                                // load brush UI 
                                Log.Info(debugTag, "Loading STAMP tool");
                                break;
                        }

                        break;

                    case "BrushMic":
                        ImageView biv = FindViewById<ImageView>(Resource.Id.BrushImageView);
                        if (biv != null)
                        {
                            double hue = msg.GetDoubleField("Hue");
                            double size = msg.GetDoubleField("Size");
                            Bitmap bm = Bitmap.CreateBitmap(biv.Width, biv.Height, Bitmap.Config.Rgb565);
                            Canvas c = new Canvas(bm);
                            Paint p = new Paint { Color = Android.Graphics.Color.HSVToColor(new float[]{(float)hue, 0.6f, 0.6f }),
                                                StrokeWidth = (float) size};
                            c.DrawOval(new RectF(), new Paint());
                            Log.Info("BRUSHMIC", "BITMAP FOUND, MAY PROCEED *********************");
                        }
                        break;
                    default:
                        break;
                }

            }
        }
        #endregion
    }
}

