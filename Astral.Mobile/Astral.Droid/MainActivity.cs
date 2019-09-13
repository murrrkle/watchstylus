using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Widget;
using Astral.Device;
using Astral.Droid.Fragments;
using Astral.Droid.Media;
using Astral.Droid.Sensors;
using Astral.Droid.UI;
using System;
using System.Net;

namespace Astral.Droid
{
    // NOTE: had to add potrait orientation: if that's not there, an orientation change recreates the activity!
    [Activity(Label = "Astral.Droid", MainLauncher = true, Icon = "@mipmap/icon", 
        Theme = "@android:style/Theme.DeviceDefault", 
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
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

        public struct PaintBkp
        {
            public int hue;
            public float sat;
            public float val;
            public int size;

        }

        AstralDevice m_device;

        BrushTypes currentTool;
        
        Vibrator vibrator;
        LinearLayout activityContent;

        BrushFragment bf;
        AirbrushFragment af;
        EraserFragment ef;
        StampFragment sf;

        PaintBkp paintBkp;
       
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
                                   
            vibrator = (Vibrator)this.ApplicationContext.GetSystemService(Context.VibratorService);
            activityContent = FindViewById<LinearLayout>(Resource.Id.ActivityContent);
            paintBkp = new PaintBkp() { hue = 0, sat = 1, val = 0.8f, size = 10 };
            InitializeAstral();
            m_device.Start();


            // Initialize and bind Brush Fragment and events
            bf = new BrushFragment();
            bf.SliderUpdated += SliderUpdate;
            bf.MicAttrChanged += MicAttrChanged;

            // Initialize and bind AirBrush Fragment and events
            af = new AirbrushFragment();
            af.AirflowChanged += Aiv_AirflowChanged;
            af.RecalibrateButtonPressed += RecalibrateButtonPressed;

            // Initialize Eraser and Stamp Fragments
            ef = new EraserFragment();
            sf = new StampFragment();

            // Default fragment: Brush
            this.FragmentManager.BeginTransaction()
                .Add(Resource.Id.ActivityContent, bf)
                .Commit();
        }

        private void RecalibrateButtonPressed(object sender)
        {
            Net.Message msg = new Net.Message("Recalibrate");
            m_device.SendMessage(msg);
        }

        private void MicAttrChanged(object sender, MicrophoneMode m)
        {
            Net.Message msg = new Net.Message("MicAttrChanged");
            msg.AddField("Mode", (int) m);
            m_device.SendMessage(msg);
        }
        #endregion
        #region events

        private void SliderUpdate(object sender, int hue, float sat, float val, int size)
        {
            //String s = String.Concat(hue, sat, val, size);
            //Log.Info("SliderUpdate", s);
            Net.Message msg = new Net.Message("SliderUpdate");
            msg.AddField("Hue", hue);
            msg.AddField("Sat", (double) sat);
            msg.AddField("Val", (double) val);
            msg.AddField("Size", size);
            m_device.SendMessage(msg);

            
        }

        private void Aiv_AirflowChanged(object sender, float airflow)
        {
            Net.Message msg = new Net.Message("AirflowChanged");
            msg.AddField("Amount", airflow);
            Log.Debug("AirbrushChangedEvent", ""+airflow);
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
            //string ipAddress = "70.77.214.69";
            string ipAddress = "192.168.1.177";
            int port = 10001;

            //Log.Debug("Astral.Droid", "Attempting connection...");
            m_device.Connect(IPAddress.Parse(ipAddress), port);
            m_device.MessageReceived += AstralMessageReceived;
            m_device.ServiceFailed += M_device_ServiceFailed;

            
        }

        private void M_device_ServiceFailed(object sender, EventArgs e)
        {
            //Log.Debug("Astral.Droid","Connection failed.");
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
                        switch ((BrushTypes) msg.GetIntField("Type"))
                        {
                            case BrushTypes.BRUSH:
                                vibrator.Vibrate(30);
                                Log.Info("ChangeTool", "Loading BRUSH tool");
                                this.FragmentManager.BeginTransaction()
                                    .Replace(Resource.Id.ActivityContent, bf)
                                    .Commit();
                                while (!bf.IsAdded)
                                {
                                    Log.Debug("ChangeTool", "Waiting for fragment to attach...");
                                }
                                bf.UpdateMicMode((MicrophoneMode)msg.GetIntField("MicMode"));
                                bf.SetColour(msg.GetIntField("Hue"), (float) msg.GetDoubleField("Sat"), (float) msg.GetDoubleField("Val"), msg.GetIntField("Size"));
                                bf.UpdateSliders();
                                break;


                            case BrushTypes.ERASER:
                                vibrator.Vibrate(20);
                                vibrator.Vibrate(20);
                                Log.Info("ChangeTool", "Loading ERASER tool");
                                this.FragmentManager.BeginTransaction()
                                    .Replace(Resource.Id.ActivityContent, ef)
                                    .Commit();
                                break;

                            case BrushTypes.AIRBRUSH:
                                vibrator.Vibrate(80);
                                Log.Info("ChangeTool", "Loading AIRBRUSH tool");
                                this.FragmentManager.BeginTransaction()
                                    .Replace(Resource.Id.ActivityContent, af)
                                    .Commit();
                                break;

                            case BrushTypes.STAMP:
                                Log.Info("ChangeTool", "Loading STAMP tool");
                                vibrator.Vibrate(50);
                                vibrator.Vibrate(20);
                                this.FragmentManager.BeginTransaction()
                                    .Replace(Resource.Id.ActivityContent, sf)
                                    .Commit();
                                break;
                        }

                        break;

                    case "BrushMic":
                        if (!bf.MicPaused())
                        {
                            int hue = msg.GetIntField("Hue");
                            float sat = (float)msg.GetDoubleField("Sat");
                            float val = (float)msg.GetDoubleField("Val");
                            int size =  msg.GetIntField("Size");

                            bf.SetColour(hue, sat, val, size);
                            bf.UpdateSliders();
                        }
                        break;

                    case "Vibrate":
                        double velocity = msg.GetDoubleField("Velocity");
                        vibrator.Vibrate((int) velocity * 2);
                        break;

                    case "Stamp":
                        sf.UpdateContent((byte[]) msg.GetField("buffer", typeof(byte[])));
                        break;
                            
                    default:
                        break;
                }

            }
        }
        #endregion
    }
}

