﻿using System;
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
using System.Windows.Shapes;

using Astral;
using Astral.Device;
using System.Windows.Threading;
using System.ComponentModel;

namespace TestingConcepts
{
    /// <summary>
    /// Interaction logic for RuleEditingWindow.xaml
    /// </summary>
    public partial class RuleEditingWindow : Window
    {
        private AstralDevice device;
        private DeviceModel deviceModel = null;

        private ModuleType currentModule = ModuleType.Display;

        private MappingCaptureWindow mouseCapture;
       
        private RuleManager manager;
        private DispatcherTimer timer = new DispatcherTimer();
      
        Rule rule = new ContinuousRule(MobileEventType.TouchMove);

        private ModuleType activeSensor = ModuleType.Display;

        // rule properties that get sent to the manager for update
        private MobileEventType eventType = MobileEventType.None;
        private Rect source = new Rect();
        private Rect destination = new Rect();
        private bool invert = false;
        private EasingType easing = EasingType.Linear;

        public AstralDevice Device
        {
            get
            {
                return this.device;
            }
            set
            {
                this.device = value;
                this.deviceModel = new DeviceModel(this.device);

                // remove later
                //this.manager.AddRule(this.rule);
          //      (rule as ContinuousRule).DestinationMouseMapping = AstralContinuousRuleMapping.Scroll;
            }
        }

        public Rule Rule
        {
            get
            {
                UpdateRule();
                return this.rule;
            }
            set
            {
                this.rule = value;
                UpdateRule();
                this.manager.SetActiveRuleSet("temp");
            }
        }

        #region Initialization and Constructor

        private void UpdateRule()
        {
            this.rule.SourceRect = this.source;
            this.rule.DestinationRect = this.destination;
            if(this.rule is ContinuousRule)
            {
                (this.rule as ContinuousRule).Invert = this.invert;
                (this.rule as ContinuousRule).Easing = this.easing;
            }
            this.manager.UpdateTempRule(this.rule);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.manager.AddRuleSet("temp");
            this.manager.SetActiveRuleSet("temp");
            this.manager.AddRule(this.rule);

            this.timer.Interval = TimeSpan.FromMilliseconds(1000 / 60);
            this.timer.Tick += OnTimerTick;
            this.timer.Start();

            this.TouchPlotter.SelectionChanged += OnSelectionChanged;
            this.Plotter.SelectionChanged += OnSelectionChanged;
            this.OrientationVisualizer.Plotter.SelectionChanged += OnSelectionChanged;

            this.DoButton.Click += OnDoClick;

            this.EasingButton.Click += OnEasingButtonClicked;
            this.EasingSelector.PropertyChanged += OnEasingPropertyChanged;
            this.EasingSelector.InvertCheckBox.Click += OnInvertCheck;

            Dispatcher.Invoke(new Action(delegate
            {
                this.mouseCapture = new MappingCaptureWindow();
            }));

            foreach(Canvas c in this.Container.Children.OfType<Canvas>())
            {
                c.Visibility = Visibility.Hidden;
            }

            this.RuleMouseButtonBar.Selection += OnNewMouseButtonBarSelection;
            this.ScrollRangeSlider.Changed += OnScrollRangeSliderChanged;

            this.ClickInRangeCheckBox.Click += OnClickRangeChanged;

            this.DragGrid.MouseLeftButtonDown += WindowDragClick;
            this.ExitButton.Click += OnExit;

            this.MoveMouseButton.Click += OnMoveMouseButtonClick;
            this.MouseClickCheckBox.Click += OnMouseClickCheckBox;
        }

        private void OnMouseClickCheckBox(object sender, RoutedEventArgs e)
        {
            if(this.MouseClickCheckBox.IsChecked.Value)
            {
                this.rule.InputAction.InputEvent = PCInputEventType.MouseClick;
            }
            else
            {
                if(this.RuleMouseButtonBar.CurrentMouseBarState == MouseButtonBarState.Down)
                {
                    this.rule.InputAction.InputEvent = PCInputEventType.MouseDown;
                }
                else
                {
                    this.rule.InputAction.InputEvent = PCInputEventType.MouseUp;
                }
            }
        }

        private void OnMoveMouseButtonClick(object sender, RoutedEventArgs e)
        {
            // set a position to move the mouse cursor
        }

        private void OnClickRangeChanged(object sender, RoutedEventArgs e)
        {
            this.rule.ChecksBounds = this.ClickInRangeCheckBox.IsChecked.Value;
            UpdateRule();
        }

        private void OnInvertCheck(object sender, RoutedEventArgs e)
        {
            this.invert = this.EasingSelector.InvertCheckBox.IsChecked.Value;
            UpdateRule();
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void WindowDragClick(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void OnScrollRangeSliderChanged(object sender, RangeSliderChangedEventArgs e)
        {
            if(this.Rule.InputAction.InputEvent == PCInputEventType.MouseScroll)
            {
                this.destination = new Rect(new Point(e.Low, e.Low), new Point(e.High, e.High));
                UpdateRule();
            }
        }

        private void HideMouseCanvases()
        {
            this.ScrollCanvas.Visibility = Visibility.Hidden;
            this.EasingButton.Visibility = Visibility.Hidden;
            this.MouseMoveCanvas.Visibility = Visibility.Hidden;
            this.MouseUpCanvas.Visibility = Visibility.Hidden;
        }

        private void OnNewMouseButtonBarSelection(object sender, MouseButtonBarEventArgs e)
        {
            HideMouseCanvases();
            switch (e.State)
            {
                case MouseButtonBarState.Down:
                    this.Rule = new DiscreteRule(this.eventType);
                    this.Rule.InputAction.InputEvent = PCInputEventType.MouseDown;
                    this.MouseUpCanvas.Visibility = Visibility.Visible;
                    break;
                case MouseButtonBarState.Up:
                    this.Rule = new DiscreteRule(this.eventType);
                    this.Rule.InputAction.InputEvent = PCInputEventType.MouseDown; 
                    break;
                case MouseButtonBarState.Move:
                    this.Rule = new ContinuousRule(this.eventType);
                    this.Rule.InputAction.InputEvent = PCInputEventType.MouseMove;
                    this.Rule.Dimension = MappingDimension.XY;
                    this.MouseMoveCanvas.Visibility = Visibility.Visible;
                    this.EasingButton.Visibility = Visibility.Visible;
                    break;
                case MouseButtonBarState.Scroll:
                    this.Rule = new ContinuousRule(this.eventType);
                    this.Rule.InputAction.InputEvent = PCInputEventType.MouseScroll;
                    this.Rule.Dimension = MappingDimension.Y;
                    Console.WriteLine(this.eventType);
                    this.ScrollCanvas.Visibility = Visibility.Visible;
                    this.EasingButton.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        private void OnEasingPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.EasingSelector.Visibility = Visibility.Hidden;
            this.easing = this.EasingSelector.EasingType;
            UpdateRule();
        }

        private void OnEasingButtonClicked(object sender, RoutedEventArgs e)
        {
            this.EasingSelector.Visibility = Visibility.Visible;
        }
                

        private void InitializeSensorButtons()
        {
            ModuleType[] allModules = (ModuleType[])Enum.GetValues(typeof(ModuleType));
            foreach(ModuleType module in allModules)
            {
                SensorButton button = new SensorButton(module);

                this.SensorButtonPanel.Children.Add(button);
                button.Click += OnSensorButtonClicked;
            }
        }

        private void HideSensorCanvases()
        {
            this.TouchCanvas.Visibility = Visibility.Hidden;
            this.CompassCanvas.Visibility = Visibility.Hidden;
            this.OrientationCanvas.Visibility = Visibility.Hidden;
        }

        private void UnhookAllEvents(ModuleType sensorType)
        {
            ModuleType[] allModules = (ModuleType[])Enum.GetValues(typeof(ModuleType));
            foreach (ModuleType module in allModules)
            {
                switch (module)
                {
                    case ModuleType.Display:
                        this.deviceModel.Display.TouchDown -= OnTouchDown;
                        this.deviceModel.Display.TouchMove -= OnTouchMoved;
                        this.deviceModel.Display.TouchUp -= OnTouchUp;
                        break;
                    case ModuleType.Accelerometer:
                        break;
                    case ModuleType.Gyroscope:
                        break;
                    case ModuleType.Compass:
                     //   this.deviceModel.Compass.HeadingChanged -= OnCompassChanged;
                        break;
                    case ModuleType.Magnetometer:
                        break;
                    case ModuleType.Orientation:
                     //   this.deviceModel.Orientation.OrientationChanged -= OnOrientationChanged;
                        break;
                    case ModuleType.AmbientLight:
                        break;
                    default:
                        break;
                }
            }

        }

        private void OnSensorButtonClicked(object sender, SensorButtonClickEventArgs e)
        {
            if(deviceModel != null)
            {
                UnhookAllEvents(activeSensor);
                HideSensorCanvases();

                this.activeSensor = e.SensorType;
                switch (e.SensorType)
                {
                    case ModuleType.Display:
                        this.deviceModel.Display.TouchDown += OnTouchDown;
                        this.deviceModel.Display.TouchMove += OnTouchMoved;
                        this.deviceModel.Display.TouchUp += OnTouchUp;
                        this.TouchPlotter.DeviceResolution = new Size(this.deviceModel.Display.Width, this.deviceModel.Display.Height);
                        this.TouchCanvas.Visibility = Visibility.Visible;
                        // change later
                        
                        this.eventType = MobileEventType.TouchMove;
                        break;
                    case ModuleType.Accelerometer:
                        break;
                    case ModuleType.Gyroscope:
                        break;
                    case ModuleType.Compass:
                        this.deviceModel.Compass.HeadingChanged += OnCompassChanged;
                        this.CompassCanvas.Visibility = Visibility.Visible;
                        this.eventType = MobileEventType.CompassChanged;

                        break;
                    case ModuleType.Magnetometer:
                        break;
                    case ModuleType.Orientation:
                        this.deviceModel.Orientation.OrientationChanged += OnOrientationChanged;
                        this.CompassCanvas.Visibility = Visibility.Visible;
                        this.eventType = MobileEventType.OrientationChanged;

                        break;
                    case ModuleType.AmbientLight:
                        break;
                    default:
                        break;
                }
                UpdateRule();
            }

        }



        public RuleEditingWindow()
        {
            InitializeComponent();
            InitializeSensorButtons();
        }

        public RuleEditingWindow(RuleManager ruleManager)
            : this()
        {
            this.manager = ruleManager;
                        
            this.Loaded += OnLoaded;
        }

        #endregion



        private void OnTimerTick(object sender, EventArgs e)
        {
            if (this.currentModule == ModuleType.Display)
            {
                this.TouchPlotter.DrawPoints();
            }
            this.Plotter.DrawPoints();
        }


        private void OnTouchDown(object sender, AstralTouchEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                this.TouchPlotter.AddTouchPoint(e.TouchPoint.Id, new Point(e.TouchPoint.X, e.TouchPoint.Y));
                
            }));
        }

        private void OnOrientationChanged(object sender, AstralOrientationEventArgs e)
        {
            this.Plotter.UpdateValue(Utils.Map(e.OrientationData.PitchDegrees, -90, 90, 270, 90));
            Dispatcher.Invoke(new Action(delegate
            {
                this.CompassTextBlock.Text = string.Format("{0:0}", e.OrientationData.PitchDegrees);
                //Console.WriteLine(e.OrientationData.PitchDegrees);
                Console.WriteLine(RuleString());
            }));
        }

        private void OnCompassChanged(object sender, AstralCompassEventArgs e)
        {
            this.Plotter.UpdateValue(e.CompassData.Heading);
            Dispatcher.Invoke(new Action(delegate
            {
                this.CompassTextBlock.Text = string.Format("{0:0}", e.CompassData.Heading);
           //     Console.WriteLine(RuleString());
            }));
        }

        private void OnTouchMoved(object sender, AstralTouchEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                this.TouchPlotter.UpdatePoint(e.TouchPoint.Id, new Point(e.TouchPoint.X, e.TouchPoint.Y));

            }));
        }

        private void OnTouchUp(object sender, AstralTouchEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                this.TouchPlotter.RemovePoint(e.TouchPoint.Id, new Point(e.TouchPoint.X, e.TouchPoint.Y));

                //m.LeftUp();
            }));
        }


        private void OnSelectionChanged(object sender, SelectionEventArgs e)
        {
            if(sender is MultiTouchPlotter)
            { 
                Display display = this.deviceModel.Display;

                double topX = Utils.Map(Math.Min(e.Selection.TopLeft.X, e.Selection.BottomLeft.X), 0, this.TouchPlotter.Width, 0, display.Width);
                double topY = Utils.Map(Math.Min(e.Selection.TopLeft.Y, e.Selection.BottomLeft.Y), 0, this.TouchPlotter.Height, 0, display.Height);
                double selectionWidth = Utils.Map(e.Selection.Width, 0, this.TouchPlotter.Width, 0, display.Width);
                double selectionHeight = Utils.Map(e.Selection.Height, 0, this.TouchPlotter.Height, 0, display.Height);

                string top = String.Format("{0:0000}, {1:0000} ", topX, topY);
                string size = String.Format("{0:0000} x {1:0000} ", selectionWidth, selectionHeight);

                this.SelectionTopLabel.Text = top;
                this.SelectionSizeLabel.Text = size;
            }

            //if(this.eventType == MobileEventType.TouchMove)
            // this.source = this.TouchPlotter.SelectionInDeviceCoords;
            //else
                this.source = this.Plotter.Selection;
            UpdateRule();
        }


        private void OnDoClick(object sender, RoutedEventArgs e)
        {
            // show mouse capture window
          //  this.source = this.TouchPlotter.SelectionInDeviceCoords;
            this.destination = this.mouseCapture.Selection;

            UpdateRule();
        }

        private string RuleString()
        {
            string s = this.source + "\n"
                + this.destination + "\n"
                + this.eventType + "\n"
                + this.rule.InputAction.InputEvent + "\n"
                + this.rule.InputAction.Argument + "\n"
                + "================= \n";

            return s;

        }
    }
}