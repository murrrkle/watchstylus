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

        MovingAverageFilter microphoneClean = new MovingAverageFilter();

        public event EventHandler<EventArgs> RuleAdded;

        string activeRuleSetName = "";

        private void RaiseRuleAdded(EventArgs e)
        {
            RuleAdded?.Invoke(this, e);
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
            }
        }

        #region Initialization and Constructor

        private void UpdateRule()
        {
            this.rule.EventType = this.eventType;
            this.rule.SourceRect = this.source;
            this.rule.DestinationRect = this.destination;
            if(this.rule is ContinuousRule)
            {
                (this.rule as ContinuousRule).Invert = this.invert;
                (this.rule as ContinuousRule).Easing = this.easing;
            }
            this.manager.UpdateTempRule(this.rule);

            this.DebugText.Text = DateTime.Now.TimeOfDay.ToString() + " :: " + this.eventType + " :: " + this.rule.InputAction.InputEvent; 
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.activeRuleSetName = this.manager.GetKey(this.manager.ActiveRules);
            this.manager.AddRuleSet("temp");
            this.manager.SetActiveRuleSet("temp");
            this.manager.AddRule(this.rule);

            this.timer.Interval = TimeSpan.FromMilliseconds(1000 / 60);
            this.timer.Tick += OnTimerTick;
            this.timer.Start();

            this.TouchPlotter.SelectionChanged += OnSelectionChanged;
            this.Plotter.SelectionChanged += OnSelectionChanged;
            this.OrientationVisualizer.Plotter.SelectionChanged += OnSelectionChanged;
            this.MicPlotter.SelectionChanged += OnSelectionChanged;
            this.AccPlotter.SelectionChanged += OnSelectionChanged;

            this.DoButton.Click += OnDoClick;

            this.deviceModel.Session.InputSelectionWindowClosed += OnInputSelectionWindowClosed;

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

            this.AddButton.Click += OnAddRuleButtonClicked;

            this.MouseOrKeyboardControl.Clicked += OnMouseOrKeyboardSelected;

            this.AllMouseCommands.Visibility = Visibility.Hidden;
            this.AllKeyboardCanvas.Visibility = Visibility.Hidden;

            this.KeyboardBar.Selection += OnKeyBoardBarSelection;

            this.EnterKeyTextBox.MouseLeftButtonUp += OnEnterKeyTextBoxClicked;
            this.EnterKeyTextBox.KeyUp += OnEnterKeyTextBoxKeyUp;

            InitializeTouchCanvas();

            this.MicCanvas.Visibility = Visibility.Hidden;

        }

        private void InitializeTouchCanvas()
        {
            this.ToggleTouchDown.Click += (s, e) =>
            {
                this.ToggleTouchMove.IsChecked = false;
                this.ToggleTouchUp.IsChecked = false;
                this.eventType = MobileEventType.TouchDown;
                UpdateRule();
            };

            this.ToggleTouchMove.Click += (s, e) =>
            {
                this.ToggleTouchDown.IsChecked = false;
                this.ToggleTouchUp.IsChecked = false;
                this.eventType = MobileEventType.TouchMove;
                UpdateRule();
            };

            this.ToggleTouchUp.Click += (s, e) =>
            {
                this.ToggleTouchMove.IsChecked = false;
                this.ToggleTouchDown.IsChecked = false;
                this.eventType = MobileEventType.TouchUp;
                UpdateRule();
            };
        }

        private void OnEnterKeyTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            this.EnterKeyTextBox.Text = e.Key.ToString();
            this.rule.InputAction.Argument = e.Key;
            UpdateRule();
        }

        private void OnEnterKeyTextBoxClicked(object sender, MouseButtonEventArgs e)
        {
            this.EnterKeyTextBox.Focus();
            this.EnterKeyTextBox.SelectAll();
        }

        private void OnKeyBoardBarSelection(object sender, KeyboardButtonBarEventArgs e)
        {
            this.Rule = new DiscreteRule(this.eventType);
            switch (e.State)
            {
                case KeyboardButtonBarState.Down:
                    this.rule.InputAction.InputEvent = PCInputEventType.KeyDown;
                    break;
                case KeyboardButtonBarState.Up:
                    this.rule.InputAction.InputEvent = PCInputEventType.KeyUp;
                    break;
                case KeyboardButtonBarState.Press:
                    this.rule.InputAction.InputEvent = PCInputEventType.KeyPress;
                    break;
                default:
                    break;
            }
            UpdateRule();
        }

        private void KeyboardSelected()
        {
            this.AllKeyboardCanvas.Visibility = Visibility.Visible;
            this.AllMouseCommands.Visibility = Visibility.Hidden;
        }

        private void MouseSelected()
        {
            this.AllMouseCommands.Visibility = Visibility.Visible;
            this.AllKeyboardCanvas.Visibility = Visibility.Hidden;
        }

        private void OnMouseOrKeyboardSelected(object sender, MouseOrKeyboardEventArgs e)
        {
            if(e.MouseOrKey == MouseOrKeyboard.Keyboard)
            {
                KeyboardSelected();
            }
            else
            {
                MouseSelected();
            }
        }

        private void OnAddRuleButtonClicked(object sender, RoutedEventArgs e)
        {
            this.rule.Name = this.RuleNameTextBox.Text;
            UpdateRule();

            this.manager.SetActiveRuleSet(this.activeRuleSetName);
            this.manager.AddRule(this.rule);

            this.manager.RemoveRuleSet("temp");
            Console.WriteLine("RAISE");
            RaiseRuleAdded(new EventArgs());
            
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
            this.MicCanvas.Visibility = Visibility.Hidden;
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
                        this.deviceModel.Accelerometer.AccelerationChanged -= OnAccelerationUpdated;
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
                    case ModuleType.Microphone:
                        this.deviceModel.Microphone.MicrophoneUpdated -= OnMicrophoneUpdated;
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
                        this.AccelerometerCanvas.Visibility = Visibility.Visible;
                        this.eventType = MobileEventType.AccelerationMagnitudeChanged;
                        this.deviceModel.Accelerometer.AccelerationChanged += OnAccelerationUpdated;
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
                    case ModuleType.Microphone:
                        this.eventType = MobileEventType.AmplitudeChanged;
                        this.deviceModel.Microphone.MicrophoneUpdated += OnMicrophoneUpdated;
                        this.MicCanvas.Visibility = Visibility.Visible;
                        this.MicPlotter.MaxRange = 1000;
                        this.MicPlotter.StartAtZero = true;
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
            this.MicPlotter.DrawPoints();
            this.AccPlotter.DrawPoints();
        }

        private void OnAccelerationUpdated(object sender, AstralAccelerometerEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                this.AccPlotter.PushPoint(Utils.Magnitude(e.AccelerationData.X, e.AccelerationData.Y, e.AccelerationData.Z));

            }));
        }

        private void OnMicrophoneUpdated(object sender, AstralMicrophoneEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                this.microphoneClean.ComputeAverage(e.MicrophoneData.Amplitude);
                double micValue = (microphoneClean.Average > 1000 ? 1000 : microphoneClean.Average);
                this.MicPlotter.PushPoint(micValue);
                this.MicReading.Text = micValue + " :: " + MicPlotter.SelectionInRuleCoordinates.Left + " :: " + this.MicPlotter.SelectionInRuleCoordinates.Right;
            }));
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
            //   this.source = this.TouchPlotter.SelectionInRuleCoordinates;
            this.source = this.AccPlotter.SelectionInRuleCoordinates;
            //else
            //this.source = this.MicPlotter.SelectionInRuleCoordinates;
            UpdateRule();
        }

        private void OnInputSelectionWindowClosed(object sender, Astral.UI.SelectionWindowEventArgs e)
        {
            if(e.Reason == Astral.UI.ClosingReason.OK)
            {
                Console.WriteLine(this.deviceModel.InputRegion);
                this.destination = this.deviceModel.InputRegion;
            }
        }

        private void OnDoClick(object sender, RoutedEventArgs e)
        {
            // show mouse capture window
            //  this.source = this.TouchPlotter.SelectionInDeviceCoords;
            this.deviceModel.ShowInputWindow();
         //   this.destination = this.mouseCapture.Selection;

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
