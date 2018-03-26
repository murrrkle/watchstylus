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

        #region Touch Class Members
        private Dictionary<int, Ellipse> m_touchPts;
        #endregion
        #endregion

        #region Constructors
        public MainWindow()
        {
            InitializeComponent();

            m_touchPts = new Dictionary<int, Ellipse>();
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

                // add selection window event handlers (for demonstration)
                m_session.CaptureSelectionWindowClosed += SessionCaptureSelectionWindowClosed;
                m_session.InputSelectionWindowClosed += SessionInputSelectionWindowClosed;
            }
        }

        private void AstralSessionTerminated(object sender, AstralSession session)
        {
            if (session != null
                && session.Equals(m_session))
            {
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
        #endregion
    }
    #endregion
}
