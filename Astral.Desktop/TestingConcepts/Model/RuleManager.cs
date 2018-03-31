﻿using System;
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

        private RuleManager()
        {
            this.inputHandler.EscapeCaptured += OnGlobalEscapePressed;
         //   this.worker.DoWork += OnWorkerDoWork;
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
                    downRule.InputAction.InputEvent = PCInputEventType.MouseDown;
                    DiscreteRule upRule = new DiscreteRule(MobileEventType.TouchUp, newRule);
                    upRule.InputAction.InputEvent = PCInputEventType.MouseDown;
                    AddRule(downRule);
                    AddRule(upRule);
                }
            }
            // check if DestinationMapping is MoveAndMouseDown, create event for selection entered
            // do the same for thresholds?
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


        private void OnMicrophoneUpdated(object sender, AstralMicrophoneEventArgs e)
        {
            foreach (Rule rule in this.activeRules.Where(r => r.EventType == MobileEventType.AmplitudeChanged))
            {
                if (this.isActive)
                {
                    bool inRange = rule.ExecuteRule(new Point(e.MicrophoneData.Amplitude, e.MicrophoneData.Amplitude));
                    if (inRange)
                        this.inputHandler.ExecuteInputAction(rule.InputAction);
                }
            }
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
            foreach (Rule rule in this.activeRules.Where(r => r.EventType == MobileEventType.AmbientLightChanged))
            {
                if (this.isActive)
                {
                    double light = (e.AmbientLightData.AmbientLight > 1000 ? 1000 : e.AmbientLightData.AmbientLight);
                    bool inRange = rule.ExecuteRule(new Point(light, light));
                    if (inRange)
                        this.inputHandler.ExecuteInputAction(rule.InputAction);
                }
            }
        }

        private void AccelerationChanged(object sender, AccelerationDeviceModelEventArgs e)
        {
            foreach (Rule rule in this.activeRules.Where(r => r.EventType == MobileEventType.AccelerationChanged))
            {
                try
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
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine("LOL NOOB");
                }
            }

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
                bool inRange = rule.ExecuteRule(new Point(e.TouchPoint.X, e.TouchPoint.Y));
                if (inRange)
                {
                    this.inputHandler.ExecuteInputAction(rule.InputAction);
                }
            }
        }

        private void TouchMove(object sender, AstralTouchEventArgs e)
        {

            foreach (Rule rule in this.activeRules.Where(r => r.EventType == MobileEventType.TouchMove))
            {
                
                bool inRange = rule.ExecuteRule(new Point(e.TouchPoint.X, e.TouchPoint.Y));
                if(inRange)
                {
                    this.inputHandler.ExecuteInputAction(rule.InputAction);
                }
            }
        }

        private void TouchDown(object sender, AstralTouchEventArgs e)
        {
            try
            {
                foreach (Rule rule in this.activeRules.Where(r => r.EventType == MobileEventType.TouchDown))
                {
                    bool inRange = rule.ExecuteRule(new Point(e.TouchPoint.X, e.TouchPoint.Y));
                    if (inRange)
                    {
                        this.inputHandler.ExecuteInputAction(rule.InputAction);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("LOL");
            }
        }



    }
}
