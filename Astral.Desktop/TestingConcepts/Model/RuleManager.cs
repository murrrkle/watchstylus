using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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

        #endregion

        #region Constructor

        public DeviceModel(AstralDevice device)
        {
            this.Device = device;

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

    }

    #endregion
    
    public class RuleManager
    {
        private static RuleManager instance = null;
        
        public static RuleManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new RuleManager();
                }
                return instance;
            }       
        }

        private List<Rule> activeRules = new List<Rule>();


        private Dictionary<string, List<Rule>> allRuleSets = new Dictionary<string, List<Rule>>();

        public PCInputHandler inputHandler = new PCInputHandler();
        private bool isActive = true;

        BackgroundWorker worker = new BackgroundWorker();

        private DeviceModel deviceModel = null;

        public List<Rule> ActiveRules
        {
            get
            {
                return this.activeRules;
            }
            internal set
            {
                this.activeRules = value;
            }
        }

        public AstralDevice Device
        {
            get
            {
                return this.deviceModel.Device;
            }
            set
            {
                if(this.deviceModel != null)
                {
                    DisableHandlers(this.deviceModel);
                }
                this.deviceModel = new DeviceModel(value);
                EnableHandlers(this.deviceModel);
            }
        }

        private RuleManager()
        {
            this.inputHandler.EscapeCaptured += OnGlobalEscapePressed;
            this.worker.DoWork += OnWorkerDoWork;
        }

        private void OnWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            // LAND OF ALL ERRORS
            if (e.Argument != null && e.Argument is TouchPoint)
            {
                lock (this.activeRules)
                {
                    TouchPoint point = e.Argument as TouchPoint;
                    foreach (Rule rule in this.activeRules.Where(r => r.EventType == MobileEventType.TouchMove))
                    {
                        rule.ExecuteRule(new Point(point.X, point.Y));
                        this.inputHandler.ExecuteInputAction(rule.InputAction);
                    }
                }
            }
        }

        private void OnGlobalEscapePressed(object sender, EventArgs e)
        {
            this.isActive = !this.isActive;
          //  StopRules();
        }

        public void StartRules()
        {
            this.isActive = true;
        }

        public void StopRules()
        {
            this.isActive = false;
        }

        public void SetActiveRuleSet(string ruleSetName)
        {
            if(this.allRuleSets.ContainsKey(ruleSetName))
            {
                this.activeRules = this.allRuleSets[ruleSetName];
            }
        }

        public void AddRuleSet(string name)
        {
            this.allRuleSets.Add(name, new List<Rule>());
        }


        public bool ContainsRuleSet(string name)
        {
            return this.allRuleSets.ContainsKey(name);
        }

        public void AddRule(Rule newRule)
        {
            this.activeRules.Add(newRule);
            if(newRule.ChecksBounds)
            {
                newRule.SelectionEntered += OnRuleSelectionEntered;
                newRule.SelectionExited += OnRuleSelectionExited;
                if(newRule is ContinuousRule && newRule.ChecksBounds)
                {
                    DiscreteRule downRule = new DiscreteRule(MobileEventType.TouchDown, newRule);
                    DiscreteRule upRule = new DiscreteRule(MobileEventType.TouchUp, newRule);
                    AddRule(downRule);
                    AddRule(upRule);
                }
            }
            // check if DestinationMapping is MoveAndMouseDown, create event for selection entered
            // do the same for thresholds?
        }

        internal void UpdateTempRule(Rule sameRule)
        {
            this.activeRules[0] = sameRule;

            if (this.activeRules.Count > 0 && sameRule is ContinuousRule && sameRule.ChecksBounds)
            {
                DiscreteRule downRule = new DiscreteRule(MobileEventType.TouchDown, sameRule);
                downRule.InputAction.InputEvent = PCInputEventType.MouseDown;
                DiscreteRule upRule = new DiscreteRule(MobileEventType.TouchUp, sameRule);
                upRule.InputAction.InputEvent = PCInputEventType.MouseUp;

                AddRule(downRule);
                AddRule(upRule);
            }
            if(this.activeRules.Count > 1 && sameRule is ContinuousRule && !sameRule.ChecksBounds)
            {
                this.activeRules.RemoveRange(1, 2);
            }
        }

        private void OnRuleSelectionExited(object sender, SelectionCrossedEventArgs e)
        {
            if(sender as Rule is ContinuousRule)
            {
                this.inputHandler.MouseLeftUp();
            }
        }

        private void OnRuleSelectionEntered(object sender, SelectionCrossedEventArgs e)
        {
            if (sender as Rule is ContinuousRule)
            {
                this.inputHandler.MouseLeftDown();
            }
        }

        #region Handling Subscription to Device

        public void EnableHandlers(DeviceModel device)
        {
            device.Display.TouchDown += TouchDown;
            device.Display.TouchMove += TouchMove;
            device.Display.TouchUp += TouchUp;

            if (device.Device.HasCompass) device.Compass.HeadingChanged += CompassUpdated;
            if (device.Device.HasAccelerometer) device.Accelerometer.AccelerationChanged += AccelerationChanged;
            if (device.Device.HasAmbientLight) device.AmbientLight.AmbientLightChanged += AmbientLightChanged;
            if (device.Device.HasGyroscope) device.Gyroscope.RotationChanged += RotationChanged;
            if (device.Device.HasMagnetometer) device.Magnetometer.MagnetometerChanged += MagnetometerChanged;
            if (device.Device.HasOrientation) device.Orientation.OrientationChanged += OrientationChanged;
            if(device.Device.HasMicrophone) device.Microphone.MicrophoneUpdated += OnMicrophoneUpdated;
            
        }


        private void DisableHandlers(DeviceModel device)
        {
            device.Display.TouchDown -= TouchDown;
            device.Display.TouchMove -= TouchMove;
            device.Display.TouchUp -= TouchUp;

            device.Compass.HeadingChanged -= CompassUpdated;
            device.Accelerometer.AccelerationChanged -= AccelerationChanged;
            device.AmbientLight.AmbientLightChanged -= AmbientLightChanged;
            device.Gyroscope.RotationChanged -= RotationChanged;
            device.Magnetometer.MagnetometerChanged -= MagnetometerChanged;
            device.Orientation.OrientationChanged -= OrientationChanged;
            device.Microphone.MicrophoneUpdated -= OnMicrophoneUpdated;
        }

        #endregion


        private void OnMicrophoneUpdated(object sender, AstralMicrophoneEventArgs e)
        {
            Console.WriteLine("DATA!");
            //Console.WriteLine(e.MicrophoneData.Amplitude);
        }

        private void OrientationChanged(object sender, AstralOrientationEventArgs e)
        {
            foreach (Rule rule in this.activeRules.Where(r => r.EventType == MobileEventType.OrientationChanged))
            {
                if (this.isActive)
                {
                    bool inRange = rule.ExecuteRule(new Point(Utils.Map(e.OrientationData.PitchDegrees, -90, 90, 270, 90), Utils.Map(e.OrientationData.PitchDegrees, -90, 90, 270, 90)));
                    Console.WriteLine(inRange);
                    if (inRange)
                        this.inputHandler.ExecuteInputAction(rule.InputAction);
                }
            }
        }

        private void MagnetometerChanged(object sender, AstralMagnetometerEventArgs e)
        {

        }

        private void RotationChanged(object sender, AstralGyroscopeEventArgs e)
        {

        }

        private void AmbientLightChanged(object sender, AstralAmbientLightEventArgs e)
        {

        }

        private void AccelerationChanged(object sender, AstralAccelerometerEventArgs e)
        {
            //  this.activeRules.ExecuteRules(MobileEventType.AccelerationMagnitudeChanged, e.AccelerationData.Magnitude);
            //this.activeRules.ExecuteRules(MobileEventType.AccelerationChanged, e.AccelerationData.X, e.AccelerationData.Y, e.AccelerationData.Z);

        }

        private void CompassUpdated(object sender, AstralCompassEventArgs e)
        {
            foreach (Rule rule in this.activeRules.Where(r => r.EventType == MobileEventType.CompassChanged))
            {
                if (this.isActive)
                {
                    bool inRange = rule.ExecuteRule(new Point(e.CompassData.Heading, e.CompassData.Heading));
                    if(inRange)
                    this.inputHandler.ExecuteInputAction(rule.InputAction);
                }
            }
        }

        private void TouchUp(object sender, AstralTouchEventArgs e)
        {
            foreach (Rule rule in this.activeRules.Where(r => r.EventType == MobileEventType.TouchUp))
            {
                rule.ExecuteRule(new Point(e.TouchPoint.X, e.TouchPoint.Y));
                this.inputHandler.ExecuteInputAction(rule.InputAction);
            }
        }

        private void TouchMove(object sender, AstralTouchEventArgs e)
        {
            foreach (Rule rule in this.activeRules.Where(r => r.EventType == MobileEventType.TouchMove))
            {
                rule.ExecuteRule(new Point(e.TouchPoint.X, e.TouchPoint.Y));
                this.inputHandler.ExecuteInputAction(rule.InputAction);
            }
        }

        private void TouchDown(object sender, AstralTouchEventArgs e)
        {
            foreach (Rule rule in this.activeRules.Where(r => r.EventType == MobileEventType.TouchDown))
            {
                rule.ExecuteRule(new Point(e.TouchPoint.X, e.TouchPoint.Y));
                this.inputHandler.ExecuteInputAction(rule.InputAction);
            }
        }



    }
}
