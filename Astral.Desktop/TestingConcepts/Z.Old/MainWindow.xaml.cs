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
using System.Windows.Threading;

using Astral;
using Astral.Device;



using Orientation = Astral.Device.Orientation;

namespace TestingConcepts
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MultiTouchPlotter mtplotter;
        
        private AstralService m_service;
        private AstralSession m_session;

        private void InitializeService()
        {
            m_session = null;

            m_service = AstralService.GetInstance();
            m_service.SessionEstablished += AstralSessionEstablished;
            m_service.SessionTerminated += AstralSessionTerminated;
            m_service.Start();
        }

        public MainWindow()
        {
            InitializeComponent();

         //   mtplotter = new MultiTouchPlotter(1080, 1920);
          //  mtplotter.Width = 200;
            //mtplotter.Height = 200;
            
         //   this.Container.Children.Add(mtplotter);
            this.Loaded += OnLoaded;
        }

        KeyboardHook hook;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {

            InitializeService();
            // timer only taking place because I don't have a
            // sensor stream at the moment
            DispatcherTimer t = new DispatcherTimer();
            t.Interval = TimeSpan.FromMilliseconds(1000/60);
            //t.Start();
            t.Tick += T_Tick;
            hook = new KeyboardHook();
            hook.HookKeyboard();
            hook.OnKeyPressed += Hook_OnKeyPressed;
            this.Closed += OnClosed;
        }

        private void OnClosed(object sender, EventArgs e)
        {
            hook.UnHookKeyboard();
        }

        private void Hook_OnKeyPressed(object sender, KeyPressedArgs e)
        {
            Console.WriteLine(e.KeyPressed);
            if(e.KeyPressed == Key.Escape)
            {
                Console.WriteLine("Rules would be disabled here");
            }

        }

        private void T_Tick(object sender, EventArgs e)
        {

            //mtplotter.Plot(800, 800);
            //double val = (DateTime.Now.Millisecond % 10);
            //Console.WriteLine(val);
            //this.accelPlotter.PushPoint(val);
            //this.accelPlotter.DrawPoints();
        }

        private void AstralSessionEstablished(object sender, AstralSession session)
        {
            if (session != null
                && m_session == null)
            {
                m_session = session;
                Dispatcher.Invoke(
                    new Action(
                        delegate ()
                        {
                          //  DeviceInfoLabel.Content = m_session.Device.Name + " [" + m_session.Device.Class + "]";
                        }));


                m_session.SelectRegion();

                Display display = m_session.Device[ModuleType.Display] as Display;
                display.TouchDown += OnTouchDown; 
                display.TouchMove += OnTouchMove;
                display.TouchUp += OnTouchUp;
                Console.WriteLine("CONNECTED");

                //Accelerometer accelerometer = m_session.Device[ModuleType.Accelerometer] as Accelerometer;
                //accelerometer.AccelerationChanged += OnAccelerationUpdated;

                //Gyroscope gyroscope = m_session.Device[ModuleType.Gyroscope] as Gyroscope;
                //gyroscope.RotationChanged += OnGyroscopeUpdated;

                //Orientation orientation = m_session.Device[ModuleType.Orientation] as Orientation;
                //orientation.OrientationChanged += OnOrientationChanged;

             //   AmbientLight light = m_session.Device[ModuleType.AmbientLight] as AmbientLight;
            //    light.AmbientLightChanged += OnAmbientLightChanged;

                //Magnetometer magnetometer = m_session.Device[ModuleType.Magnetometer] as Magnetometer;
              //  magnetometer.MagnetometerChanged += OnMagnetometerChanged;
            }
        }

        private void OnMagnetometerChanged(object sender, AstralMagnetometerEventArgs e)
        {
            Console.WriteLine(e.MagnetometerData.X);
        }

        private void OnAmbientLightChanged(object sender, AstralAmbientLightEventArgs e)
        {
            //Console.WriteLine(e.AmbientLightData.AmbientLight);
        }

        private void OnOrientationChanged(object sender, AstralOrientationEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                this.OrientationDataView.UpdateValues(e.OrientationData.YawDegrees, e.OrientationData.PitchDegrees, e.OrientationData.RollDegrees);
            }));
        }

        private void OnGyroscopeUpdated(object sender, AstralGyroscopeEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                this.GyroscopeDataView.UpdateValues(e.GyroscopeData.X, e.GyroscopeData.Y, e.GyroscopeData.Z);
            }));
        }

        KeyboardSimulator keyboard = new KeyboardSimulator();

        private void OnAccelerationUpdated(object sender, AstralAccelerometerEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                this.AccelerometerDataView.UpdateValues(e.AccelerationData.X, e.AccelerationData.Y, e.AccelerationData.Z);
                //double magnitude = Utils.Magnitude(e.AccelerationData.X, e.AccelerationData.Y, e.AccelerationData.Z);
                //this.accelPlotterMagnitude.PushPoint(magnitude);
                //this.accelPlotterX.PushPoint(e.AccelerationData.X);
                //this.accelPlotterY.PushPoint(e.AccelerationData.Y);
                //this.accelPlotterZ.PushPoint(e.AccelerationData.Z);

                //this.plotter3D.PushPoint(e.AccelerationData.X, e.AccelerationData.Y, e.AccelerationData.Z);
               // this.plotter3D.DrawPoints();
            }));
        }

        private void OnTouchUp(object sender, AstralTouchEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                
            //    mtplotter.Plot((int)e.TouchPoint.X, (int)e.TouchPoint.Y);
            }));
        }

        private void OnTouchMove(object sender, AstralTouchEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
            //    mtplotter.Plot((int)e.TouchPoint.X, (int)e.TouchPoint.Y);
            }));
        }

        private void OnTouchDown(object sender, AstralTouchEventArgs e)
        {
            // Console.WriteLine(e.TouchPoint.Id + " :: " + e.TouchPoint.X);
            Dispatcher.Invoke(new Action(delegate
            {
           //     mtplotter.Plot((int)e.TouchPoint.X, (int)e.TouchPoint.Y);
            }));
        }

        private void AstralSessionTerminated(object sender, AstralSession session)
        {

        }

    }
}
