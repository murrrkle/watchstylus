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
            public float hue;
            public float sat;
            public float val;
            public float size;

        }

        AstralDevice m_device;

        BrushTypes currentTool;
        
        Vibrator vibrator;
        LinearLayout activityContent;

        BrushFragment bf;
        AirbrushFragment af;

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

            //biv = new BrushImageView(this);
            //aiv = new AirbrushImageView(this);
            //siv = new ScreenshotView(this, null);
            InitializeAstral();
            m_device.Start();



            bf = new BrushFragment();
            bf.SliderUpdated += SliderUpdate;

            af = new AirbrushFragment();
            af.AirflowChanged += Aiv_AirflowChanged;

            
            this.FragmentManager.BeginTransaction()
                .Add(Resource.Id.ActivityContent, bf)
                .Commit();

            //FragmentTransaction abrushTx = this.FragmentManager.BeginTransaction();
            //abrushTx.Add(Resource.Id.ActivityContent, af);
            //abrushTx.Commit();



            /*
            aiv.AirflowChanged += Aiv_AirflowChanged;
            biv.toggle.Click += Toggle_Click;
            */


            //currentTool = BrushTypes.BRUSH;
            //biv.MicAttribute = 4;
            //activityContent.AddView(biv);

        }
        #endregion
        #region events

        private void SliderUpdate(object sender, float hue, float sat, float val, float size)
        {
            //String s = String.Concat(hue, sat, val, size);
            //Log.Info("SliderUpdate", s);
            Net.Message msg = new Net.Message("SliderUpdate");
            msg.AddField("Hue", hue);
            msg.AddField("Sat", sat);
            msg.AddField("Val", val);
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
            string ipAddress = "10.13.179.6";
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
                        vibrator.Vibrate(10);
                        /*
                        RunOnUiThread(() =>
                        {
                            activityContent.RemoveAllViews();
                        });
                        */
                        switch ((BrushTypes)msg.GetIntField("Type"))
                        {
                            case (int) BrushTypes.BRUSH:
                                Log.Info("DEBUG", "Loading AIRBRUSH tool");
                                this.FragmentManager.BeginTransaction()
                                    .Replace(Resource.Id.ActivityContent, bf)
                                    .Commit();
                                break;
                                //Log.Info(debugTag, "Loading BRUSH tool");
                                //StartBrushTool();
                                break;

                            case BrushTypes.ERASER:
                                //StartEraserTool();
                                // load brush UI 
                                //Log.Info(debugTag, "Loading ERASER tool");
                                break;

                            case BrushTypes.AIRBRUSH:

                                //StartAirbrushTool();
                                // load brush UI 
                                Log.Info("DEBUG", "Loading AIRBRUSH tool");
                                this.FragmentManager.BeginTransaction()
                                    .Replace(Resource.Id.ActivityContent, af)
                                    .Commit();
                                break;

                            case BrushTypes.STAMP:
                                //StartStampTool();
                                
                                //SetContentView(Resource.Layout.Main);

                                // load brush UI 
                                //Log.Info(debugTag, "Loading STAMP tool");
                                break;
                        }

                        break;

                    case "BrushMic":
                        if (!bf.MicPaused())
                        {
                            float hue = (float) msg.GetDoubleField("Hue");
                            float sat = (float)msg.GetDoubleField("Sat");
                            float val = (float)msg.GetDoubleField("Val");
                            float size = (float) msg.GetIntField("Size");

                            bf.SetColour(hue, sat, val, size);
                        }
                        /*
                        if (biv.MicAttribute == 0)

                            biv.hSlider.Progress = (int)hue;


                        if (biv.MicAttribute == 1)
                            biv.sSlider.Progress = (int)(sat * 100);

                        if (biv.MicAttribute == 2)
                            biv.vSlider.Progress = (int)(val * 100);
                        if (biv.MicAttribute == 3)
                            biv.zSlider.Progress = (int)size;


                        biv.SetBrush(hue, sat, val, size);
                        biv.PostInvalidate();
                        */
                        break;

                    case "Vibrate":
                        double velocity = msg.GetDoubleField("Velocity");
                        Int64[] pattern = new long[] { 0, (Int64)velocity, 0, (Int64)velocity };
                        vibrator.Vibrate(pattern, 1);
                        break;

                    case "Stamp":
                        //siv.UpdateContent((byte[]) msg.GetField("buffer", typeof(byte[])));
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
            //    activityContent.AddView(siv);
            });
            vibrator.Vibrate(50);
            //currentTool = BrushTypes.STAMP;
        }

        private void StartAirbrushTool()
        {
            RunOnUiThread(() =>
            {
              //  activityContent.AddView(aiv);
            });
            vibrator.Vibrate(50);
            //currentTool = BrushTypes.AIRBRUSH;

        }

        private void StartEraserTool()
        {
            vibrator.Vibrate(50);
            //currentTool = BrushTypes.ERASER;
        }

        private void StartBrushTool()
        {
            
            RunOnUiThread(() =>
            {
               // activityContent.AddView(biv);
            });
            
            vibrator.Vibrate(50);
            //currentTool = BrushTypes.BRUSH;
            
        }
        #endregion
    }
}

