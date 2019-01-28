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
            biv.hSlider.ProgressChanged += HSlider_ProgressChanged;
            biv.sSlider.ProgressChanged += SSlider_ProgressChanged;
            biv.vSlider.ProgressChanged += VSlider_ProgressChanged;
            biv.zSlider.ProgressChanged += ZSlider_ProgressChanged;
            biv.toggle.Click += Toggle_Click;

            currentTool = BrushTypes.BRUSH;
            biv.MicAttribute = 4;
            activityContent.AddView(biv);
            
        }

        private void Toggle_Click(object sender, EventArgs e)
        {
            switch (biv.MicAttribute)
            {
                case 0:
                    biv.MicAttribute = 1;
                    biv.toggle.Text = "S";
                    break;
                case 1:
                    biv.MicAttribute = 2;
                    biv.toggle.Text = "V";
                    break;
                case 2:
                    biv.MicAttribute = 3;
                    biv.toggle.Text = "R";
                    break;
                case 3:
                    biv.MicAttribute = 4;
                    biv.toggle.Text = "N";
                    break;
                case 4:
                    biv.MicAttribute = 0;
                    biv.toggle.Text = "H";
                    break;
            }
            Net.Message msg = new Net.Message("MicAttrChanged");
            msg.AddField("Attr", biv.MicAttribute);
            m_device.SendMessage(msg);
        }

        private void ZSlider_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (biv.MicAttribute != 3)
            {

            float prevhue = biv.hue;
            float prevval = biv.val;
            float prevsat = biv.sat;

            biv.SetBrush(prevhue, prevsat, prevval, e.Progress);
            biv.PostInvalidate();

            Net.Message msg = new Net.Message("SizeChanged");
            msg.AddField("Amount", (int)e.Progress);
            m_device.SendMessage(msg);
            }
        }

        private void VSlider_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (biv.MicAttribute != 2)
            {
                float prevhue = biv.hue;
                float prevsat = biv.sat;
                float prevsize = biv.size;

                biv.SetBrush(prevhue, prevsat, e.Progress / 100, prevsize);
                biv.PostInvalidate();

                Net.Message msg = new Net.Message("ValChanged");
                msg.AddField("Amount", (double)e.Progress / 100);
                m_device.SendMessage(msg);
            }
        }

        private void SSlider_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (biv.MicAttribute != 1)
            {
                float prevhue = biv.hue;
                float prevval = biv.val;
                float prevsize = biv.size;

                biv.SetBrush(prevhue, e.Progress / 100, prevval, prevsize);
                biv.PostInvalidate();

                Net.Message msg = new Net.Message("SatChanged");
                msg.AddField("Amount", (double)e.Progress / 100);
                m_device.SendMessage(msg);
            }
        }

        private void HSlider_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (biv.MicAttribute != 0)
            {
                float prevsat = biv.sat;
                float prevval = biv.val;
                float prevsize = biv.size;

                biv.SetBrush(e.Progress, prevsat, prevval, prevsize);
                biv.PostInvalidate();

                Net.Message msg = new Net.Message("HueChanged");
                msg.AddField("Amount", e.Progress);
                m_device.SendMessage(msg);
            }
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
            //string ipAddress = "70.77.214.69";
            string ipAddress = "192.168.0.39";
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
                            float sat = (float)msg.GetDoubleField("Sat");
                            float val = (float)msg.GetDoubleField("Val");
                            float size = (float) msg.GetIntField("Size");
                            
                            //if (biv.MicAttribute == 0)
                       
                                biv.hSlider.Progress = (int) hue;

                        
                            //if (biv.MicAttribute == 1) 
                                biv.sSlider.Progress = (int) (sat * 100);

                            //if (biv.MicAttribute == 2)
                                biv.vSlider.Progress = (int) (val * 100);
                            //if (biv.MicAttribute == 3)
                                biv.zSlider.Progress = (int) size;
                        

                            biv.SetBrush(hue, sat, val, size);
                            biv.PostInvalidate();
                            break;

                    case "Vibrate":
                        double velocity = msg.GetDoubleField("Velocity");
                        Int64[] pattern = new long[] { 0, (Int64)velocity, 0, (Int64)velocity };
                        vibrator.Vibrate(pattern, 1);
                        break;

                    case "Stamp":
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

