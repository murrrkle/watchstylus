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
using Android.Hardware;
using Astral.Content;

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

        AstralDevice m_device;

        BrushTypes currentTool;
        Android.Graphics.Color CurrentColor; 
        Vibrator vibrator;
        LinearLayout activityContent;
        BrushImageView biv;
        AirbrushImageView aiv;
        ScreenshotView siv;
        

        string debugTag;
        #endregion

        #region Android Startup
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
            debugTag = "VS DEBUG";
            vibrator = (Vibrator)this.ApplicationContext.GetSystemService(Context.VibratorService);
            activityContent = FindViewById<LinearLayout>(Resource.Id.ActivityContent);
            biv = new BrushImageView(this);
            aiv = new AirbrushImageView(this);
            siv = new ScreenshotView(this, null);
            InitializeAstral();
            m_device.Start();

            aiv.AirflowChanged += Aiv_AirflowChanged;

            currentTool = BrushTypes.BRUSH;
            activityContent.AddView(biv);
            
        }

        private void Aiv_AirflowChanged(object sender, float airflow)
        {
            Net.Message msg = new Net.Message("AirflowChanged");
            msg.AddField("Amount", airflow);
            m_device.SendMessage(msg);

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

            //ScreenshotView screenshotView = FindViewById<ScreenshotView>(Resource.Id.ScreenshotView);

            // add the corresponding handlers to the views
            //screenshotView.Screen = m_device[ModuleType.Display] as Astral.Device.Display;
            string ipAddress = "192.168.0.32";
            int port = 10001;

            m_device.Connect(IPAddress.Parse(ipAddress), port);
            m_device.MessageReceived += AstralMessageReceived;

            
        }
        #endregion

        #region Event Handler
        private void AstralMessageReceived(object sender, Net.Message msg)
        {
            //Log.Info(debugTag, "RECEIVED ASTRAL MESSAGE" );
            
            if (msg != null)
            {
                // get the message name
                string msgName = msg.Name;
                switch (msgName)
                {
                    case "ChangeTool":
                        
                        RunOnUiThread(() =>
                        {
                            activityContent.RemoveAllViews();
                        });
                        
                        switch (msg.GetIntField("Type"))
                        {
                            case (int) BrushTypes.BRUSH:
                                //Log.Info(debugTag, "Loading BRUSH tool");
                                StartBrushTool();
                                break;

                            case (int)BrushTypes.ERASER:
                                StartEraserTool();
                                // load brush UI 
                                //Log.Info(debugTag, "Loading ERASER tool");
                                break;

                            case (int)BrushTypes.AIRBRUSH:
                                StartAirbrushTool();
                                // load brush UI 
                                //Log.Info(debugTag, "Loading AIRBRUSH tool");
                                break;

                            case (int)BrushTypes.STAMP:
                                StartStampTool();
                                
                                //SetContentView(Resource.Layout.Main);

                                // load brush UI 
                                //Log.Info(debugTag, "Loading STAMP tool");
                                break;
                        }

                        break;

                    case "BrushMic":
                            float hue = (float) msg.GetDoubleField("Hue");
                            CurrentColor = Android.Graphics.Color.HSVToColor(new float[] { hue, 0.8f, 0.8f});
                            float size = (float) msg.GetDoubleField("Size");
                            biv.SetBrush(CurrentColor, size);
                            biv.PostInvalidate();
                            break;

                    case "Vibrate":
                        double velocity = msg.GetDoubleField("Velocity");
                        Int64[] pattern = new long[] { 0, (Int64)velocity, 0, (Int64)velocity };
                        vibrator.Vibrate(pattern, 1);
                        break;

                    case "Stamp":

                        Log.Info("SCREENSHOT RECIEVED", "SCREENSHOT RECEIVED");
                        
                        siv.UpdateContent((byte[]) msg.GetField("buffer", typeof(byte[])));
                        break;
                            
                    default:
                        break;
                }

            }
        }

        private void StartStampTool()
        {
            RunOnUiThread(() =>
            {
                activityContent.AddView(siv);
            });
            vibrator.Vibrate(50);
            currentTool = BrushTypes.STAMP;
        }

        private void StartAirbrushTool()
        {
            RunOnUiThread(() =>
            {
                activityContent.AddView(aiv);
            });
            vibrator.Vibrate(50);
            currentTool = BrushTypes.AIRBRUSH;

        }

        private void StartEraserTool()
        {
            vibrator.Vibrate(50);
            currentTool = BrushTypes.ERASER;
        }

        private void StartBrushTool()
        {
            
            RunOnUiThread(() =>
            {
                activityContent.AddView(biv);
            });
            
            vibrator.Vibrate(50);
            currentTool = BrushTypes.BRUSH;
            
        }
        #endregion
    }
}

