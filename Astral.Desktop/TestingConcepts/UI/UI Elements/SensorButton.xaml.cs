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
using Astral.Device;

namespace TestingConcepts
{
    #region SensorButtonClickEventArgs

    public class SensorButtonClickEventArgs : EventArgs
    {
        public ModuleType SensorType { get; set; }
        
        public SensorButtonClickEventArgs(ModuleType sensorType)
        {
            this.SensorType = sensorType;
        }
    }

    #endregion

    /// <summary>
    /// Interaction logic for SensorButton.xaml
    /// </summary>
    public partial class SensorButton : UserControl
    {
        //private ModuleType sensorType = ModuleType.Display;

        public ModuleType SensorType
        {
            get
            {
               // return this.sensorType;
                return (ModuleType)this.GetValue(SensorProperty);
            }
            set
            {
               // this.sensorType = value;
                this.SetValue(SensorProperty, value);
            }
        }

        public static readonly DependencyProperty SensorProperty =
        DependencyProperty.Register("Sensor Type", typeof(ModuleType),
        typeof(SensorButton), new PropertyMetadata(ModuleType.Display), OnSensorValueChanged);


        public event EventHandler<SensorButtonClickEventArgs> Click;

        private void RaiseClick(SensorButtonClickEventArgs e)
        {
            Click?.Invoke(this, e);
        }

        private static bool OnSensorValueChanged(object value)
        {
            return true;
        }

        private void HideAllCanvases()
        {
            foreach(Canvas canvas in this.Container.Children)
            {
                if(canvas != this.BackgroundBox)
                {
                    canvas.Visibility = Visibility.Hidden;
                }
            }
        }

        private void UpdateUI()
        {
            Canvas canvasForSensor = null;
            HideAllCanvases();
            switch (this.SensorType)
            {
                case ModuleType.Display:
                    canvasForSensor = this.TouchCanvas;
                    break;
                case ModuleType.Accelerometer:
                    canvasForSensor = this.AccelerometerCanvas;
                    break;
                case ModuleType.Gyroscope:
                    canvasForSensor = this.GyroCanvas;
                    break;
                case ModuleType.Compass:
                    canvasForSensor = this.CompassCanvas;
                    break;
                case ModuleType.Magnetometer:
                    canvasForSensor = this.MagnetCanvas;
                    break;
                case ModuleType.Orientation:
                    canvasForSensor = this.OrientationCanvas;
                    break;
                case ModuleType.AmbientLight:
                    canvasForSensor = this.LightCanvas;
                    break;
                case ModuleType.Microphone:
                    canvasForSensor = this.MicCanvas;
                    break;
                    // need microphone
            }
            canvasForSensor.Visibility = Visibility.Visible;
        }

        public SensorButton()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;

        }

        public SensorButton(ModuleType sensorType)
            : this()
        {
            this.SensorType = sensorType;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateUI();

            this.MouseEnter += OnMouseEnter;
            this.MouseLeave += OnMouseLeave;
            this.Cursor = Cursors.Hand;
            this.MouseLeftButtonUp += OnMouseLeftDown;
        }

        private void OnMouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            RaiseClick(new SensorButtonClickEventArgs(this.SensorType));
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            this.Box.Fill = AstralColors.Blue;
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            this.Box.Fill = AstralColors.Orange;
        }
    }
}
