using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Net;

using Foundation;
using UIKit;

using Astral.Device;
using Astral.Net;

using Astral.iOS.Sensors;

namespace Astral.iOS
{
    public partial class ViewController : UIViewController
    {
        #region Class Members
        #region Core Class Members
        private AstralDevice m_device;
        #endregion

        #region Sensor Class Members
        private iOSAccelerometer m_accelerationSensor;

        private iOSGyroscope m_gyroscopeSensor;

        private iOSCompass m_magnetometerSensor;
        #endregion
        #endregion

        #region Constructors
        public ViewController(IntPtr handle) : base(handle)
        {
            RegisterApplicationDefaults();

            // main setup
            InitializeDevice();
        }
        #endregion

        #region Initialization
        private void RegisterApplicationDefaults()
        {
            object[] keys = { "ip_preference", "port_preference" };
            object[] values = { "127.0.0.1", "10001" };

            NSDictionary appDefaults = NSDictionary.FromObjectsAndKeys(values, keys);

            NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults;
            defaults.RegisterDefaults(appDefaults);
        }

        private void InitializeDevice()
        {
            string deviceClass = UIDevice.CurrentDevice.Model;
            string deviceName = UIDevice.CurrentDevice.Name;

            m_device = new AstralDevice(deviceClass, deviceName);
            
            // display
            Display display = new Display(new Size(
                (int)(UIScreen.MainScreen.Bounds.Size.Width * UIScreen.MainScreen.Scale),
                (int)(UIScreen.MainScreen.Bounds.Size.Height * UIScreen.MainScreen.Scale)),
                DeviceOrientation.Portrait, TouchCapabilities.Multi, ConnectivityType.ContinuousStream);
            m_device.AddModule(display);

            // accelerometer
            m_accelerationSensor = new iOSAccelerometer();
            m_device.AddModule(m_accelerationSensor);

            // gyroscope
            m_gyroscopeSensor = new iOSGyroscope();
            m_device.AddModule(m_gyroscopeSensor);

            // magnetometer
            m_magnetometerSensor = new iOSCompass();
            m_device.AddModule(m_magnetometerSensor);
        }

        private void InitializeAstral()
        {
            // add the corresponding handlers to the views
            ScreenshotContainer.Display = m_device[ModuleType.Display] as Display;

            // begin connection
            NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults;

            string ipAddress = defaults.StringForKey("ip_preference");
            int port = Convert.ToInt32(defaults.StringForKey("port_preference"));
            
            m_device.Connect(IPAddress.Parse(ipAddress), port);

            // m_service.ScreenshotReceived += RawScreenshotReceived;

            // handle other messages
            m_device.MessageReceived += AstralMessageReceived;

            /* m_service.Initialized += AstralInitialized;
            m_service.Terminated += AstralTerminated;
            m_service.Failed += AstralFailed; */
        }
        #endregion

        #region Objective-C Specific
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Perform any additional setup after loading the view, typically from a nib.
        }

        public override void ViewDidAppear(bool animated)
        {
            // finalize initialization
            InitializeAstral();

            // finally, start the Astral engine
            m_device.Start();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.     
        }
        #endregion

        #region Evnet Handler
        #region Astral Event Handler
        /* private void AstralInitialized(object sender, AstralClient client)
        {
            m_service.MessageReceived += AstralMessageReceived;

            // let's send a test message for now
            // TODO: send device specs here
            Message testMsg = new Message("Test");
            testMsg.AddField("BOOL", true);
            testMsg.AddField("INT", 123);
            testMsg.AddField("DOUBLE", 456.789);
            testMsg.AddField("STRING", "Hello World!");

            m_service.SendMessage(testMsg);
        }

        private void AstralTerminated(object sender, AstralClient client)
        {
            // ok, we're done for today
            m_service.MessageReceived -= AstralMessageReceived;
        }

        private void AstralFailed(object sender, AstralClient client)
        {
            m_service.MessageReceived -= AstralMessageReceived;

            UIAlertController alert = UIAlertController.Create("Astral Error",
                                                               "The Astral service could not be started, "
                                                               + "because the host could not be found. "
                                                               + "Please check your connection settings!",
                                                               UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
            PresentViewController(alert, animated: true, completionHandler: null);
        } */


        /* private void RawScreenshotReceived(object sender, ScreenshotBuffer screenshot)
        {
            InvokeOnMainThread(() =>
            {
                ScreenshotContainer.UpdateContent(screenshot);
            });
        } */

        private void AstralMessageReceived(object sender, Message msg)
        {
            try
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
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + "\n" + e.StackTrace);
            }
        }
        #endregion
        #endregion
    }
}