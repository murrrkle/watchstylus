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
        AccelerationMagnitudeChanged = 4,
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


        // add microphone and step counter
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
