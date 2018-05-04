using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Astral;
using Astral.Device;

namespace AstralBlankSample
{
    /*
     * Couple notes -
     * Run this system first (this will be the server), then run the Astral Mobile App
     * Update the Astral Mobile activity as you see fit and don't forget to update the IP Address
     */

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Instance Variables

        private NetworkManager networkManager = NetworkManager.Instance;
        private DeviceModel device;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            // Start the networking
            this.networkManager.Start();
            this.networkManager.DeviceAdded += OnDeviceConnected;
        }

        #endregion

        #region Connection Initialization

        private void OnDeviceConnected(object sender, DeviceConnectedEventArgs e)
        {
            device = new DeviceModel(e.Device, e.Session);

            // Note: all networking events are running in a separate thread
            // you have to use this dispatcher invoke to do anything with the UI
            // otherwise you'll get a weird exception you can't make sense of
            Dispatcher.Invoke(new Action(delegate
            {
                
            }));
            
            InitializeDeviceEvents();
        }

        private void InitializeDeviceEvents()
        {
            device.Display.TouchDown += OnTouchDown;
            device.Accelerometer.AccelerationChanged += AccelerometerUpdated;
            device.Microphone.MicrophoneUpdated += OnMicrophoneUpdated;

            // newer version of the accelerometer
            device.AccelerationChanged += OnAccelerationChanged;
        }

        #endregion

        private void AccelerometerUpdated(object sender, AstralAccelerometerEventArgs e)
        {
            Console.WriteLine(e.AccelerationData.X + " :: " + e.AccelerationData.Y + " :: " + e.AccelerationData.Z);
        }

        private void OnMicrophoneUpdated(object sender, AstralMicrophoneEventArgs e)
        {
            Console.WriteLine(e.MicrophoneData.Amplitude);
            // full array of values
            Console.WriteLine(e.MicrophoneData.Data);
        }

        private void OnTouchDown(object sender, AstralTouchEventArgs e)
        {
            Console.WriteLine(e.TouchPoint.X + " :: " + e.TouchPoint.Y);
        }

        private void OnAccelerationChanged(object sender, AccelerationDeviceModelEventArgs e)
        {
            // this event can get linear acceleration, acceleration and gravity values
            Console.WriteLine(e.LinearX + " :: " + e.LinearY + " :: " + e.LinearZ);
            Console.WriteLine(e.LinearMagnitude);
        }

    }
}
