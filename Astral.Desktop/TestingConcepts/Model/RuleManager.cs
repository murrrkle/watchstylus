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
    public class RuleManager
    {
        internal class WorkerArugment
        {
            public Point Point { get; set; }
            public MobileEventType EventType { get; set; }

            public WorkerArugment(Point p, MobileEventType m)
            {
                this.Point = p;
                this.EventType = m;
            }
        }

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

        private Rule medleyRule = null;

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
        }

        public DeviceModel DeviceModel
        {
            get
            {
                return this.deviceModel;
            }
            set
            {
                this.deviceModel = value;
                EnableHandlers(this.deviceModel);
            }
        }

        public Rule MedleyRule
        {
            get
            {
                return this.medleyRule;
            }
            set
            {
                this.medleyRule = value;
                this.medleyRule.IsMedleyRule = true;
            }
        }

        public event EventHandler<EventArgs> MedleyExecuted;

        private void RaiseMedley(EventArgs e)
        {
            MedleyExecuted?.Invoke(this, e);
        }

        public void ClearMedleyRule()
        {
            if (this.medleyRule != null)
            {
                foreach (List<Rule> ruleSet in this.allRuleSets.Values)
                {
                    ruleSet.Remove(medleyRule);
                }
                this.medleyRule = null;
            }
        }

        public void AddMedley(Rule rule)
        {
            this.medleyRule = rule;
            rule.IsMedleyRule = true;
            foreach (List<Rule> ruleSet in this.allRuleSets.Values)
            {
                ruleSet.Add(medleyRule);
            }

            this.medleyRule.MedleyRuleExecuted += (s, e) => { NextRuleSet(); RaiseMedley(new EventArgs()); };
        }

        Dictionary<MobileEventType, BackgroundWorker> workers = new Dictionary<MobileEventType, BackgroundWorker>();

        private RuleManager()
        {
            this.inputHandler.EscapeCaptured += OnGlobalEscapePressed;
            this.worker.DoWork += OnWorkerDoWork;

            foreach(MobileEventType eventType in Enum.GetValues(typeof(MobileEventType)))
            {
                BackgroundWorker w = new BackgroundWorker();
                this.workers.Add(eventType, w);
                w.DoWork += OnWorkerDoWork;
            }
        }

        public string GetKey(List<Rule> rules)
        {
            string ruleSetName = "";
            foreach(string key in this.allRuleSets.Keys)
            {
                if(this.allRuleSets[key] == rules)
                {
                    ruleSetName = key;
                }
            }
            return ruleSetName;
        }

        public void RemoveRule(Rule rule)
        {
            if(rule.IsMedleyRule)
            {
                ClearMedleyRule();
            }
            foreach(Rule r in this.activeRules)
            {
                if(r.Name == rule.Name)
                {
                    this.activeRules.Remove(r);
                    break;
                }
            }
        }

        private void OnWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            List<Rule> activeRulesClone = activeRules.ToList<Rule>();
            MobileEventType currentEventType = (e.Argument as WorkerArugment).EventType;
            Point p = (e.Argument as WorkerArugment).Point;
            
            foreach (Rule rule in this.activeRules.ToList().Where(r => r.EventType == currentEventType))
            {
                bool inRange = rule.ExecuteRule(p);
                if (inRange)
                {
                    this.inputHandler.ExecuteInputAction(rule.InputAction);
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
            if (!this.allRuleSets.ContainsKey(name))
            {
                this.allRuleSets.Add(name, new List<Rule>());
            }
        }

        public void RemoveRuleSet(string name)
        {
            this.allRuleSets.Remove(name);
        }

        public bool ContainsRuleSet(string name)
        {
            return this.allRuleSets.ContainsKey(name);
        }

        public void RefreshRules()
        {
            foreach(List<Rule> ruleSet in this.allRuleSets.Values)
            {
                List<Rule> rulesWithChildren = ruleSet.Where(r => r.Child != null).ToList();
                foreach(Rule rule in rulesWithChildren)
                {
                    if(rule.ExecutedOnce)
                    {
                        rule.ExecutedOnce = false;
                    }
                }
            }
        }

        public void AddRule(Rule newRule)
        {
            this.activeRules.Add(newRule);
            Console.WriteLine(newRule.Name + " :: " + newRule.InputAction.InputEvent + " :: " + newRule.ChecksBounds);
            if(newRule.ChecksBounds)
            {
                if(newRule.ChecksBounds)
                {
                    newRule.SelectionEntered += OnRuleSelectionEntered;
                    newRule.SelectionExited += OnRuleSelectionExited;

                    //DiscreteRule downRule = new DiscreteRule(MobileEventType.TouchDown, newRule);
                    //downRule.InputAction.InputEvent = PCInputEventType.MouseDown;
                    //DiscreteRule upRule = new DiscreteRule(MobileEventType.TouchUp, newRule);
                    //upRule.InputAction.InputEvent = PCInputEventType.MouseUp;
                    //AddRule(downRule);
                    //AddRule(upRule);
                }
            }
            // check if DestinationMapping is MoveAndMouseDown, create event for selection entered
            // do the same for thresholds?
        }

        private void OnRuleSelectionExited(object sender, SelectionCrossedEventArgs e)
        {
            if (sender as Rule is ContinuousRule)
            {
                this.inputHandler.MouseLeftUp();
            }
            else if(sender as Rule is DiscreteRule)
            {
                Rule rule = sender as DiscreteRule;
                if(rule.InputAction.InputEvent == PCInputEventType.KeyPress)
                {
                    this.inputHandler.KeyUp((rule.InputAction.Argument as System.Windows.Input.Key?).GetValueOrDefault());
                }
            }
        }

        private void OnRuleSelectionEntered(object sender, SelectionCrossedEventArgs e)
        {
            if (sender as Rule is ContinuousRule)
            {
                this.inputHandler.MouseLeftDown();
            }
            else if (sender as Rule is DiscreteRule)
            {
                Rule rule = sender as DiscreteRule;
                if (rule.InputAction.InputEvent == PCInputEventType.KeyPress)
                {
                    this.inputHandler.KeyDown((rule.InputAction.Argument as System.Windows.Input.Key?).GetValueOrDefault());
                }
            }
        }

        public void NextRuleSet()
        {
            for(int i= 0; i < this.allRuleSets.Count; i++)
            {
               if(this.allRuleSets.Values.ElementAt(i) == this.activeRules)
                {
                    Console.WriteLine("Active: " + this.allRuleSets.Keys.ElementAt(i));
                    this.activeRules = (this.allRuleSets.Values.ElementAt((i + 1) % this.allRuleSets.Count));
                    break;
                }
            }
        }

        List<Rule> activeRulesClone = new List<Rule>();

        internal void GiveChildToRule(Rule child, string parentName)
        {
            for(int i = 0; i < this.activeRules.Count; i++)
            {
                if(this.activeRules[i].Name == parentName)
                {
                    this.activeRules[i].Child = child;
                    Console.WriteLine("Rule " + activeRules[i].Name + " now has child " + this.activeRules[i].Child.Name);
                    this.activeRulesClone = new List<Rule>(this.activeRules);
                }
            }
        }

        internal void SwapWithClone()
        {
            this.activeRules = this.activeRulesClone;
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



        #region Handling Subscription to Device

        public void EnableHandlers(DeviceModel device)
        {
            device.Display.TouchDown += TouchDown;
            device.Display.TouchMove += TouchMove;
            device.Display.TouchUp += TouchUp;

            if (device.Device.HasCompass)
            {
                device.Compass.Accuracy = 10.0;
                device.Compass.HeadingChanged += CompassUpdated;
            }

            if (device.Device.HasAccelerometer)
            {
                device.Accelerometer.Accuracy = 0.25;
                device.AccelerationChanged += AccelerationChanged;
            }

            if (device.Device.HasAmbientLight)
            {
                device.AmbientLight.Accuracy = 1.0;
                device.AmbientLight.AmbientLightChanged += AmbientLightChanged;
            }

            if (device.Device.HasGyroscope)
            {
                device.Gyroscope.Accuracy = 0.5;
                device.Gyroscope.RotationChanged += RotationChanged;
            }

            if (device.Device.HasMagnetometer)
            {
                device.Magnetometer.Accuracy = 1.0;
                device.Magnetometer.MagnetometerChanged += MagnetometerChanged;
            }

            if (device.Device.HasOrientation) device.Orientation.OrientationChanged += OrientationChanged;
            if (device.Device.HasMicrophone) device.Microphone.MicrophoneUpdated += OnMicrophoneUpdated;
            
        }

        private void DisableHandlers(DeviceModel device)
        {
            device.Display.TouchDown -= TouchDown;
            device.Display.TouchMove -= TouchMove;
            device.Display.TouchUp -= TouchUp;

            device.Compass.HeadingChanged -= CompassUpdated;
            device.AccelerationChanged -= AccelerationChanged;
            device.AmbientLight.AmbientLightChanged -= AmbientLightChanged;
            device.Gyroscope.RotationChanged -= RotationChanged;
            device.Magnetometer.MagnetometerChanged -= MagnetometerChanged;
            device.Orientation.OrientationChanged -= OrientationChanged;
            device.Microphone.MicrophoneUpdated -= OnMicrophoneUpdated;
        }

        #endregion

        private void RunWorkerFromDictionary(Point value, MobileEventType eventType)
        {
            if (!this.workers[eventType].IsBusy)
            {
                this.workers[eventType].RunWorkerAsync(new WorkerArugment(value, eventType));
            }
        }

        private void OnMicrophoneUpdated(object sender, AstralMicrophoneEventArgs e)
        {
            MobileEventType eventType = MobileEventType.AmplitudeChanged;
            double amplitude = (e.MicrophoneData.Amplitude > 1000 ? 1000 : e.MicrophoneData.Amplitude);
            Point value = new Point(amplitude, amplitude);
            RunWorkerFromDictionary(value, eventType);
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
            MobileEventType eventType = MobileEventType.AmbientLightChanged;
            double light = (e.AmbientLightData.AmbientLight > 1000 ? 1000 : e.AmbientLightData.AmbientLight);
            Point value = new Point(light, light);
            RunWorkerFromDictionary(value, eventType);
            
        }

        private void AccelerationChanged(object sender, AccelerationDeviceModelEventArgs e)
        {
            foreach (Rule rule in this.activeRules.ToList().Where(r => r.EventType == MobileEventType.AccelerationChanged))
            {               
                    if (this.isActive)
                    {
                        double x, y, z, mag;
                        if (rule.FilterInfo == null) rule.FilterInfo = "None";
                        if (rule.FilterInfo.Contains("None"))
                        {
                            x = e.AccelerationX; y = e.AccelerationY; z = e.AccelerationZ; mag = e.Magnitude;
                        }
                        else if (rule.FilterInfo.Contains("Gravity"))
                        {
                            x = e.GravityX; y = e.GravityY; z = e.GravityZ; mag = e.GravityMagnitude;
                        }
                        else
                        {
                            x = e.LinearX; y = e.LinearY; z = e.LinearZ; mag = e.LinearMagnitude;
                        }

                        double value = (rule.ArgumentInfo == RuleArgument.Magnitude ? mag :
                            rule.ArgumentInfo == RuleArgument.X ? x :
                            rule.ArgumentInfo == RuleArgument.Y ? y : z);

                        bool inRange = rule.ExecuteRule(new Point(value, value));
                        if (inRange)
                            this.inputHandler.ExecuteInputAction(rule.InputAction);
                    }
                
            }
        }

        private void CompassUpdated(object sender, AstralCompassEventArgs e)
        {
            MobileEventType eventType = MobileEventType.CompassChanged;
            Point value = new Point(e.CompassData.Heading, e.CompassData.Heading);
            RunWorkerFromDictionary(value, eventType);
        }

        private void TouchUp(object sender, AstralTouchEventArgs e)
        {
            MobileEventType eventType = MobileEventType.TouchUp;
            Point value = new Point(e.TouchPoint.X, e.TouchPoint.Y);
            RunWorkerFromDictionary(value, eventType);
        }

        private void TouchMove(object sender, AstralTouchEventArgs e)
        {
            MobileEventType eventType = MobileEventType.TouchMove;
            Point value = new Point(e.TouchPoint.X, e.TouchPoint.Y);
            RunWorkerFromDictionary(value, eventType);
        }

        private void TouchDown(object sender, AstralTouchEventArgs e)
        {
            MobileEventType eventType = MobileEventType.TouchDown;
            Point value = new Point(e.TouchPoint.X, e.TouchPoint.Y);
            RunWorkerFromDictionary(value, eventType);
        }       

    }
}
