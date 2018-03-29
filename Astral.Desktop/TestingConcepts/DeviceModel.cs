using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Astral;
using Astral.Device;

using Orientation = Astral.Device.Orientation;

namespace TestingConcepts
{
    #region Enum MobileEventType

    public enum MobileEventType
    {
        None = -1,
        TouchDown = 0,
        TouchUp = 1,
        TouchMove = 2,
        AccelerationChanged = 3,
        AmbientLightChanged = 5,
        CompassChanged = 6,
        MagnetometerChanged = 7,
        MagnetometerMagnitudeChanged = 8,
        MagnetometerAngleChanged = 9,
        GyroscopeChanged = 10,
        OrientationChanged = 11,
        AmplitudeChanged = 12,
        FrequencyChanged = 13
    }

    #endregion

    #region AccelerationDeviceModelEventArgs

    public class AccelerationDeviceModelEventArgs : EventArgs
    {
        private double accelerationX, accelerationY, accelerationZ;
        private double linearX, linearY, linearZ;
        private double gravityX, gravityY, gravityZ;

        #region Properties

        public double Magnitude
        {
            get
            {
                return Utils.Magnitude(this.accelerationX, this.accelerationY, this.accelerationZ);
            }
        }

        public double LinearMagnitude
        {
            get
            {
                return Utils.Magnitude(this.linearX, this.linearY, this.linearZ);
            }
        }

        public double GravityMagnitude
        {
            get
            {
                return Utils.Magnitude(this.gravityX, this.gravityY, this.gravityZ);
            }
        }

        public double AccelerationX
        {
            get
            {
                return this.accelerationX;
            }
            internal set
            {
                this.accelerationX = value;
            }
        }

        public double AccelerationY
        {
            get
            {
                return this.accelerationY;
            }
            internal set
            {
                this.accelerationY = value;
            }
        }

        public double AccelerationZ
        {
            get
            {
                return this.accelerationZ;
            }
            internal set
            {
                this.accelerationZ = value;
            }
        }

        public double GravityX
        {
            get
            {
                return this.gravityX;
            }
        }

        public double GravityY
        {
            get
            {
                return this.gravityY;
            }
        }

        public double GravityZ
        {
            get
            {
                return this.gravityZ;
            }
        }

        public double LinearX
        {
            get
            {
                return this.linearX;
            }
        }

        public double LinearY
        {
            get
            {
                return this.linearY;
            }
        }

        public double LinearZ
        {
            get
            {
                return this.linearZ;
            }
        }

        #endregion

        private void SetGravityAndLinear()
        {
            // get the gravity
            float alpha = 0.8f;

            // Isolate the force of gravity with the low-pass filter.
            this.gravityX = alpha * gravityX + (1 - alpha) * accelerationX;
            this.gravityY = alpha * gravityY + (1 - alpha) * accelerationY;
            this.gravityZ = alpha * gravityZ + (1 - alpha) * accelerationZ;

            // now that we have gravity get the linear
            // using a high pass filter
            linearX = accelerationX - gravityX;
            linearY = accelerationY - gravityY;
            linearZ = accelerationZ - gravityZ;
        }

        public AccelerationDeviceModelEventArgs(double x, double y, double z)
        {
            this.accelerationX = x;
            this.accelerationY = y;
            this.accelerationZ = z;
            SetGravityAndLinear();
        }
    }

    #endregion

    #region Class DeviceModel

    public class DeviceModel
    {
        #region Instance Variables
        private AstralDevice device;
        private Display display;
        private Accelerometer accelerometer;
        private Compass compass;
        private Magnetometer magnetometer;
        private AmbientLight ambientLight;
        private Orientation orientation;
        private Gyroscope gyroscope;
        private Microphone microphone;
        private AstralSession session;

        private MovingAverageFilter accFilterX = new MovingAverageFilter();
        private MovingAverageFilter accFilterY = new MovingAverageFilter();
        private MovingAverageFilter accFilterZ = new MovingAverageFilter();

        #endregion

        #region Properties
        public AstralDevice Device { get => device; set => device = value; }
        public Display Display { get => display; set => display = value; }
        public Accelerometer Accelerometer { get => accelerometer; set => accelerometer = value; }
        public Compass Compass { get => compass; set => compass = value; }
        public Magnetometer Magnetometer { get => magnetometer; set => magnetometer = value; }
        public AmbientLight AmbientLight { get => ambientLight; set => ambientLight = value; }
        public Orientation Orientation { get => orientation; set => orientation = value; }
        public Gyroscope Gyroscope { get => gyroscope; set => gyroscope = value; }
        public Microphone Microphone { get => microphone; set => microphone = value; }

        public string Name { get => this.device.Name; }
        public string Class { get => this.device.Class; }

        public AstralSession Session { get => session; }

        public Rect InputRegion
        {
            get
            {
                return this.session.InputRegion;
            }
            set
            {
                this.session.SetInputRegion(value);
            }
        }

        public Rect CaptureRegion
        {
            get
            {
                return this.session.CaptureRegion;
            }
            set
            {
                this.session.SetCaptureRegionLocation(value.TopLeft);
                this.session.SetCaptureRegionWidth((int)value.Width);
            }
        }

        #endregion

        public event EventHandler<AccelerationDeviceModelEventArgs> AccelerationChanged;

        private void RaiseAccelerationChanged(AccelerationDeviceModelEventArgs e)
        {
            AccelerationChanged?.Invoke(this, e);
        }

        #region Constructor

        public DeviceModel(AstralDevice device, AstralSession session)
        {
            this.Device = device;
            this.session = session;

            if (device.HasDisplay) this.Display = device[ModuleType.Display] as Display;
            if (device.HasAccelerometer) this.accelerometer = device[ModuleType.Accelerometer] as Accelerometer;
            if (device.HasAmbientLight) this.ambientLight = device[ModuleType.AmbientLight] as AmbientLight;
            if (device.HasCompass) this.compass = device[ModuleType.Compass] as Compass;
            if (device.HasMagnetometer) this.magnetometer = device[ModuleType.Magnetometer] as Magnetometer;
            if (device.HasGyroscope) this.gyroscope = device[ModuleType.Gyroscope] as Gyroscope;
            if (device.HasOrientation) this.orientation = device[ModuleType.Orientation] as Orientation;
            if (device.HasMicrophone) this.microphone = device[ModuleType.Microphone] as Microphone;

            this.accelerometer.AccelerationChanged += OnAccelerationChanged;
            
        }

        private void OnAccelerationChanged(object sender, AstralAccelerometerEventArgs e)
        {
            this.accFilterX.ComputeAverage(e.AccelerationData.X);
            this.accFilterY.ComputeAverage(e.AccelerationData.Y);
            this.accFilterZ.ComputeAverage(e.AccelerationData.Z);

            RaiseAccelerationChanged(new AccelerationDeviceModelEventArgs(this.accFilterX.Average, this.accFilterY.Average, this.accFilterZ.Average));
        }

        #endregion

        public void ShowCaptureWindow()
        {
            this.session.ShowCaptureSelectionWindow();
        }

        public void ShowInputWindow()
        {
            this.session.ShowInputSelectionWindow();
        }

    }

    #endregion
}
