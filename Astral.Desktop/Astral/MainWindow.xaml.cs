using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

using Astral.Device;
using Astral.UI;

namespace Astral
{
    #region Partial Class 'MainWindow'
    public partial class MainWindow : Window
    {
        #region Static Class Members
        private const double TouchEllipseSize = 14.0;
        #endregion

        #region Class Members
        #region Astral Class Members
        private AstralService m_service;

        private AstralSession m_session;
        #endregion

        #region Imaging Class Members
        private RenderTargetBitmap m_waveBitmap;

        private List<short[]> m_samples = new List<short[]>();
        #endregion

        #region Touch Class Members
        private Dictionary<int, Ellipse> m_touchPts;
        #endregion

        #region Timer Class Members
        private DispatcherTimer m_fpsTimer;
        #endregion
        #endregion

        #region Constructors
        public MainWindow()
        {
            InitializeComponent();

            m_touchPts = new Dictionary<int, Ellipse>();

            m_fpsTimer = new DispatcherTimer();
            m_fpsTimer.Interval = TimeSpan.FromMilliseconds(50.0);
            m_fpsTimer.Tick += FramesPerSecondTimerTick;
        }
        #endregion

        #region Initialization
        private void InitializeAstral()
        {
            InitializeService();
        }

        private void InitializeService()
        {
            m_session = null;

            m_service = AstralService.GetInstance();
            m_service.SessionEstablished += AstralSessionEstablished;
            m_service.SessionTerminated += AstralSessionTerminated;
            m_service.Start();
        }
        #endregion

        #region UI Handling
        private void EnableUI()
        {
            Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        // device name
                        DeviceInfoLabel.Content = m_session.Device.Name + " [" + m_session.Device.Class + "]";

                        // touch enabled?
                        if (m_session.Device[ModuleType.Display] != null)
                        {
                            TouchPanel.IsEnabled = true;
                            CaptureButton.IsEnabled = true;
                            InputButton.IsEnabled = true;
                        }

                        // accelerometer enabled?
                        if (m_session.Device[ModuleType.Accelerometer] != null)
                        {
                            AccelerometerPanel.IsEnabled = true;
                            MotionPanel.IsEnabled = true;
                        }

                        // gyroscope enabled?
                        if (m_session.Device[ModuleType.Gyroscope] != null)
                        {
                            GyroscopePanel.IsEnabled = true;
                            MotionPanel.IsEnabled = true;
                        }

                        // compass enabled?
                        if (m_session.Device[ModuleType.Compass] != null)
                        {
                            CompassPanel.IsEnabled = true;
                            MotionPanel.IsEnabled = true;
                        }

                        MicrophonePanel.IsEnabled = m_session.Device.HasMicrophone;
                    }));
        }

        private void DisableUI()
        {
            Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        // device name
                        DeviceInfoLabel.Content = "Not connected";

                        // disable all
                        CaptureButton.IsEnabled = false;
                        InputButton.IsEnabled = false;

                        TouchPanel.IsEnabled = false;

                        MotionPanel.IsEnabled = false;
                        AccelerometerPanel.IsEnabled = false;
                        GyroscopePanel.IsEnabled = false;
                        CompassPanel.IsEnabled = false;
                    }));
        }
        #endregion

        #region Shutdown
        private void Shutdown()
        {
            // shutdown everything
            if (m_session != null)
            {
                m_session.Stop();
                m_session = null;
            }

            if (m_service != null)
            {
                m_service.Stop();

                m_service.SessionEstablished -= AstralSessionEstablished;
                m_service.SessionTerminated -= AstralSessionTerminated;

                m_service = null;
            }

            if (m_fpsTimer != null)
            {
                m_fpsTimer.Stop();
                m_fpsTimer = null;
            }

            GC.Collect();
        }
        #endregion

        #region Event Handler
        #region Window Event Handler
        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            InitializeAstral();
        }

        private void MainWindowClosing(object sender, CancelEventArgs e)
        {
            Shutdown();
        }
        #endregion

        #region UI Event Handler
        private void OnCaptureButtonClicked(object sender, RoutedEventArgs e)
        {
            if (m_session != null)
            {
                m_session.ShowCaptureSelectionWindow();
            }
        }

        private void OnInputButtonClicked(object sender, RoutedEventArgs e)
        {
            if (m_session != null)
            {
                m_session.ShowInputSelectionWindow();
            }
        }

        private void OnTouchCheckBoxCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (m_session != null)
            {
                CheckBox checkBox = sender as CheckBox;
                if ((bool)checkBox.IsChecked)
                {
                    Display display = m_session.Device[ModuleType.Display] as Display;
                    display.TouchDown += OnDisplayTouchDown;
                    display.TouchMove += OnDisplayTouchMove;
                    display.TouchUp += OnDisplayTouchUp;
                }
                else
                {
                    Display display = m_session.Device[ModuleType.Display] as Display;
                    display.TouchDown -= OnDisplayTouchDown;
                    display.TouchMove -= OnDisplayTouchMove;
                    display.TouchUp -= OnDisplayTouchUp;

                    Dispatcher.Invoke(
                       new Action(
                           delegate ()
                           {
                               TouchCanvas.Children.Clear();
                               m_touchPts.Clear();
                           }));
                }
            }
        }

        private void OnMicrophoneCheckBoxCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (m_session != null)
            {
                CheckBox checkBox = sender as CheckBox;
                if ((bool)checkBox.IsChecked)
                {
                    Microphone microphone = m_session.Device[ModuleType.Microphone] as Microphone;
                    microphone.MicrophoneUpdated += OnMicrophoneUpdated;
                }
                else
                {
                    Microphone microphone = m_session.Device[ModuleType.Microphone] as Microphone;
                    microphone.MicrophoneUpdated -= OnMicrophoneUpdated;

                    // also reset the rendering
                    m_waveBitmap.Clear();
                    AmplitudeValue.Height = 0.0;
                }
            }
        }

        private void OnAccelerometerCheckBoxCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (m_session != null)
            {
                CheckBox checkBox = sender as CheckBox;
                if ((bool)checkBox.IsChecked)
                {
                    Accelerometer accelerometer = m_session.Device[ModuleType.Accelerometer] as Accelerometer;
                    accelerometer.AccelerationChanged += OnAccelerometerAccelerationChanged;
                }
                else
                {
                    Accelerometer accelerometer = m_session.Device[ModuleType.Accelerometer] as Accelerometer;
                    accelerometer.AccelerationChanged -= OnAccelerometerAccelerationChanged;

                    Dispatcher.Invoke(
                        new Action(
                            delegate ()
                            {
                                AccelerometerXLabel.Content = "N/A";
                                AccelerometerYLabel.Content = "N/A";
                                AccelerometerZLabel.Content = "N/A";
                            }));
                }
            }
        }

        private void OnGyroscopeCheckBoxCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (m_session != null)
            {
                CheckBox checkBox = sender as CheckBox;
                if ((bool)checkBox.IsChecked)
                {
                    Gyroscope gyroscope = m_session.Device[ModuleType.Gyroscope] as Gyroscope;
                    gyroscope.RotationChanged += OnGyroscopeRotationChanged;
                }
                else
                {
                    Gyroscope gyroscope = m_session.Device[ModuleType.Gyroscope] as Gyroscope;
                    gyroscope.RotationChanged -= OnGyroscopeRotationChanged;

                    Dispatcher.Invoke(
                        new Action(
                            delegate ()
                            {
                                GyroscopeXLabel.Content = "N/A";
                                GyroscopeYLabel.Content = "N/A";
                                GyroscopeZLabel.Content = "N/A";
                            }));
                }
            }
        }

        private void OnCompassCheckBoxCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (m_session != null)
            {
                CheckBox checkBox = sender as CheckBox;
                if ((bool)checkBox.IsChecked)
                {
                    Compass compass = m_session.Device[ModuleType.Compass] as Compass;
                    compass.HeadingChanged += OnCompassHeadingChanged;
                }
                else
                {
                    Compass compass = m_session.Device[ModuleType.Compass] as Compass;
                    compass.HeadingChanged -= OnCompassHeadingChanged;

                    Dispatcher.Invoke(
                        new Action(
                            delegate ()
                            {
                                CompassHeadingLabel.Content = "N/A";
                            }));
                }
            }
        }
        #endregion

        #region Astral Event Handler
        private void AstralSessionEstablished(object sender, AstralSession session)
        {
            if (session != null
                && m_session == null)
            {
                m_session = session;
                EnableUI();

                if (m_session.Device.HasAccelerometer) m_session.Device[ModuleType.Accelerometer].Accuracy = 0.01; // force
                if (m_session.Device.HasGyroscope) m_session.Device[ModuleType.Gyroscope].Accuracy = 0.1; // torque
                if (m_session.Device.HasCompass) m_session.Device[ModuleType.Compass].Accuracy = 1.0; // deg

                // add selection window event handlers (for demonstration)
                m_session.CaptureSelectionWindowClosed += SessionCaptureSelectionWindowClosed;
                m_session.InputSelectionWindowClosed += SessionInputSelectionWindowClosed;

                m_fpsTimer.Start();
            }
        }

        private void AstralSessionTerminated(object sender, AstralSession session)
        {
            if (session != null
                && session.Equals(m_session))
            {
                m_fpsTimer.Stop();

                // remove handlers
                // remove selection window event handlers (for demonstration)
                m_session.CaptureSelectionWindowClosed -= SessionCaptureSelectionWindowClosed;
                m_session.InputSelectionWindowClosed -= SessionInputSelectionWindowClosed;

                m_session = null;

                DisableUI();
            }
        }

        private void PositionTouchEllipse(Ellipse ellipse, Point rawTouchPoint)
        {
            double virtualWidth = TouchCanvas.ActualWidth;
            double virtualHeight = TouchCanvas.ActualHeight;

            Display display = m_session.Device[ModuleType.Display] as Display;
            double deviceWidth = display.Width;
            double deviceHeight = display.Height;

            double xFactor = virtualWidth / deviceWidth;
            double yFactor = virtualHeight / deviceHeight;

            Canvas.SetLeft(ellipse, rawTouchPoint.X * xFactor - TouchEllipseSize / 2.0);
            Canvas.SetTop(ellipse, rawTouchPoint.Y * yFactor - TouchEllipseSize / 2.0);
        }

        private void OnDisplayTouchDown(object sender, AstralTouchEventArgs e)
        {
            lock (this)
            {
                if (!(m_touchPts.ContainsKey(e.TouchPoint.Id)))
                {
                    Dispatcher.Invoke(
                        new Action(
                            delegate ()
                            {
                                Ellipse touchEllipse = new Ellipse
                                {
                                    Width = TouchEllipseSize,
                                    Height = TouchEllipseSize,
                                    Fill = new SolidColorBrush(Color.FromArgb(64, 255, 0, 0)),
                                    Stroke = Brushes.Red,
                                    StrokeThickness = 1.0
                                };

                                m_touchPts.Add(e.TouchPoint.Id, touchEllipse);

                                PositionTouchEllipse(touchEllipse, new Point(e.TouchPoint.X, e.TouchPoint.Y));
                                TouchCanvas.Children.Add(touchEllipse);
                            }));
                }
            }
        }

        private void OnDisplayTouchMove(object sender, AstralTouchEventArgs e)
        {
            lock (this)
            {
                if (m_touchPts.ContainsKey(e.TouchPoint.Id))
                {
                    Dispatcher.Invoke(
                        new Action(
                            delegate ()
                            {
                                Ellipse touchEllipse = m_touchPts[e.TouchPoint.Id];
                                PositionTouchEllipse(touchEllipse, new Point(e.TouchPoint.X, e.TouchPoint.Y));
                            }));
                }
            }
        }

        private void OnDisplayTouchUp(object sender, AstralTouchEventArgs e)
        {
            lock (this)
            {
                if (m_touchPts.ContainsKey(e.TouchPoint.Id))
                {
                    Dispatcher.Invoke(
                        new Action(
                            delegate ()
                            {
                                Ellipse touchEllipse = m_touchPts[e.TouchPoint.Id];
                                PositionTouchEllipse(touchEllipse, new Point(e.TouchPoint.X, e.TouchPoint.Y));

                                TouchCanvas.Children.Remove(touchEllipse);
                                m_touchPts.Remove(e.TouchPoint.Id);
                            }));
                }
            }
        }

        private void OnAccelerometerAccelerationChanged(object sender, AstralAccelerometerEventArgs e)
        {
            Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        AccelerometerXLabel.Content = e.AccelerationData.X.ToString("F4");
                        AccelerometerYLabel.Content = e.AccelerationData.Y.ToString("F4");
                        AccelerometerZLabel.Content = e.AccelerationData.Z.ToString("F4");
                    }));
        }

        private void OnGyroscopeRotationChanged(object sender, AstralGyroscopeEventArgs e)
        {
            Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        GyroscopeXLabel.Content = e.GyroscopeData.X.ToString("F4");
                        GyroscopeYLabel.Content = e.GyroscopeData.Y.ToString("F4");
                        GyroscopeZLabel.Content = e.GyroscopeData.Z.ToString("F4");
                    }));
        }

        private void OnCompassHeadingChanged(object sender, AstralCompassEventArgs e)
        {
            Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        CompassHeadingLabel.Content = e.CompassData.Heading.ToString("F4");
                    }));
        }

        private void OnMicrophoneUpdated(object sender, AstralMicrophoneEventArgs e)
        {
            // let's update the sound image
            Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        // amplitude first
                        int maxAmplitude = 200;
                        double amplitudeFactor = e.MicrophoneData.Amplitude / maxAmplitude;
                        AmplitudeValue.Height = amplitudeFactor * AmplitudeCanvas.ActualHeight;

                        // now the waveform
                        if (m_samples.Count >= 4)
                        {
                            m_samples.RemoveAt(0);
                        }
                        m_samples.Add(e.MicrophoneData.Data);

                        int width = (int)MicrophoneBorder.ActualWidth - 2;
                        int height = (int)MicrophoneBorder.ActualHeight - 2;

                        if (m_waveBitmap == null)
                        {
                            m_waveBitmap = new RenderTargetBitmap(
                                width, height, 96.0, 96.0, PixelFormats.Pbgra32);
                        }

                        DrawingVisual visual = new DrawingVisual();
                        using (DrawingContext context = visual.RenderOpen())
                        {
                            // draw axis
                            context.DrawLine(new Pen(Brushes.Black, 1.0), 
                                new Point(0, height / 2 - 0.5), 
                                new Point(width, height / 2 - 0.5));

                            // draw data
                            List<short> allData = new List<short>();
                            foreach (short[] subSamples in m_samples)
                            {
                                foreach (short sample in subSamples)
                                {
                                    allData.Add(sample);
                                }
                            }

                            int stepping = 2;
                            for (int i = 0; i < allData.Count - stepping; i+=stepping)
                            {
                                double factor1 = i / (double)allData.Count;
                                double factor2 = (i + stepping) / (double)allData.Count;
                                double scale = 0.005;

                                Point start = new Point(
                                    factor1 * width, 
                                    height / 2 - 0.5 + allData[i] * scale);
                                Point end = new Point(
                                    factor2 * width,
                                    height / 2 - 0.5 + allData[i + stepping] * scale);

                                context.DrawLine(new Pen(Brushes.Green, 1.0), start, end);
                            }
                        }

                        m_waveBitmap.Clear();
                        m_waveBitmap.Render(visual);

                        MicrophoneCanvas.Source = m_waveBitmap;
                    }));
        }
        #endregion

        #region ISelectionWindow Event Handler
        private void SessionInputSelectionWindowClosed(object sender, SelectionWindowEventArgs e)
        {
            Debug.WriteLine("Input Window closed: REASON = " + e.Reason);
        }

        private void SessionCaptureSelectionWindowClosed(object sender, SelectionWindowEventArgs e)
        {
            Debug.WriteLine("Capture Window closed: REASON = " + e.Reason);
        }
        #endregion

        #region FPS Timer Event Handler
        private void FramesPerSecondTimerTick(object sender, EventArgs e)
        {
            if (m_session != null)
            {
                try
                {
                    double fps = m_session.FramesPerSecond;
                    if (fps >= 0.0)
                    {
                        FPSLabel.Content = fps.ToString("F2") + " fps";
                    }
                    else
                    {
                        FPSLabel.Content = "N/A";
                    }
                }
                catch { }
            }
        }
        #endregion
        #endregion
    }
    #endregion
}
