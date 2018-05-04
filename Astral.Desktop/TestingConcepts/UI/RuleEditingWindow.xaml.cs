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

        private bool isMedley = false;

        private RuleManager manager;
        private DispatcherTimer timer = new DispatcherTimer();
      
        Rule rule = new ContinuousRule(MobileEventType.TouchMove);

        private ModuleType activeSensor = ModuleType.Display;

        private PlotterBase activePlotter = null;

        // rule properties that get sent to the manager for update
        private MobileEventType eventType = MobileEventType.None;
        private Rect source = new Rect();
        private Rect destination = new Rect();
        private bool invert = false;
        private EasingType easing = EasingType.Linear;

        private Rule parentRule = null;

        MovingAverageFilter microphoneClean = new MovingAverageFilter();

        public event EventHandler<EventArgs> RuleAdded;

        string activeRuleSetName = "";

        private RuleArgument accelerationArg = RuleArgument.Magnitude;

        private void RaiseRuleAdded(EventArgs e)
        {
            RuleAdded?.Invoke(this, e);
        }

        public bool IsMedley
        {
            get
            {
                return this.isMedley;
            }
            set
            {
                this.isMedley = value;
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

            double x = this.destination.Left;
            double y = this.destination.Top;
            double w = this.destination.Width;
            double h = this.destination.Height;
            Rect dstClone = new Rect(x, y, w, h);

            if(this.activePlotter == Plotter)
            {
                this.source = new Rect(0, 0, 359.9, 359.9);
            }

            this.rule.SourceRect = this.source;
            this.rule.DestinationRect = dstClone;
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

            Rule rule = new ContinuousRule(MobileEventType.TouchMove);
            this.activeRuleSetName = this.manager.GetKey(this.manager.ActiveRules);
            this.manager.AddRuleSet("temp");
            this.manager.SetActiveRuleSet("temp");
            this.manager.AddRule(this.rule);

            this.timer.Interval = TimeSpan.FromMilliseconds(1000 / 60);
            this.timer.Tick += OnTimerTick;
            this.timer.Start();

           // this.AccelerometerPlotter.Plotter.MaxRange = 10;

            // Initialize Canvases for Sensors
            InitializeTouchCanvas();
            InitializeAcceleration();

            InitializeKeyButtons();

            // Selection events
            this.TouchPlotter.TouchPlotter.SelectionChanged += OnSelectionChanged;
            this.Plotter.SelectionChanged += OnSelectionChanged;
            this.OrientationVisualizer.Plotter.SelectionChanged += OnSelectionChanged;
            this.MicPlotter.Plotter.SelectionChanged += OnSelectionChanged;
            this.AccelerometerPlotter.Plotter.SelectionChanged += OnSelectionChanged;
            this.LightPlotter.Plotter.SelectionChanged += OnSelectionChanged;

            this.DoButton.Click += OnSelectMouseCoordinates;
            this.MoveMouseButton.Click += OnSelectMouseCoordinates;

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

            this.MicCanvas.Visibility = Visibility.Hidden;

        }


        #region Initialize Acceleration

        private void InitializeAcceleration()
        {
            this.AccelerometerPlotter.Plotter.StartAtZero = true;
            this.AccMagButton.Click += (s, e) => 
            {
                this.accelerationArg = RuleArgument.Magnitude;
                this.AccelerometerPlotter.Plotter.Stroke = AstralColors.Blue;
                this.AccArgText.Text = "MAGNITUDE";
                this.AccArgText.Foreground = AstralColors.Blue;
                this.rule.ArgumentInfo = this.accelerationArg;
                this.AccelerometerPlotter.Plotter.StartAtZero = true;
                rule.FilterInfo = (AccFilterBox.SelectedItem as ComboBoxItem).Content.ToString();
                UpdateRule();
            };
            this.AccXButton.Click += (s, e) =>
            {
                this.accelerationArg = RuleArgument.X;
                this.AccelerometerPlotter.Plotter.Stroke = AstralColors.Red;
                this.AccArgText.Text = "X";
                this.AccArgText.Foreground = AstralColors.Red;
                this.rule.ArgumentInfo = this.accelerationArg;
                this.AccelerometerPlotter.Plotter.StartAtZero = false;
                rule.FilterInfo = (AccFilterBox.SelectedItem as ComboBoxItem).Content.ToString();
                UpdateRule();
            };
            this.AccYButton.Click += (s, e) =>
            {
                this.accelerationArg = RuleArgument.Y;
                this.AccelerometerPlotter.Plotter.Stroke = AstralColors.Orange;
                this.AccArgText.Text = "Y";
                this.AccArgText.Foreground = AstralColors.Orange;
                this.rule.ArgumentInfo = this.accelerationArg;
                this.AccelerometerPlotter.Plotter.StartAtZero = false;
                rule.FilterInfo = (AccFilterBox.SelectedItem as ComboBoxItem).Content.ToString(); 
                UpdateRule();
            };
            this.AccZButton.Click += (s, e) =>
            {
                this.accelerationArg = RuleArgument.Z;
                this.AccelerometerPlotter.Plotter.Stroke = AstralColors.Teal;
                this.AccArgText.Text = "Z";
                this.AccArgText.Foreground = AstralColors.Teal;
                this.rule.ArgumentInfo = this.accelerationArg;
                this.AccelerometerPlotter.Plotter.StartAtZero = false;
                rule.FilterInfo = (AccFilterBox.SelectedItem as ComboBoxItem).Content.ToString();
                UpdateRule();
            };

        }

        #endregion

        #region Initialize Touch

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

        #endregion

        #region Keyboard Rule Methods

        private void KeyboardSelected()
        {
            this.AllKeyboardCanvas.Visibility = Visibility.Visible;
            this.AllMouseCommands.Visibility = Visibility.Hidden;
        }

        private void InitializeKeyButtons()
        {
            this.PlayPauseButton.Click += OnKeyButtonPressed;
            this.PreviousSongButton.Click += OnKeyButtonPressed;
            this.NextSongButton.Click += OnKeyButtonPressed;
            this.MailButton.Click += OnKeyButtonPressed;
            this.PrintScrButton.Click += OnKeyButtonPressed;
            this.VolumeDownButton.Click += OnKeyButtonPressed;
            this.VolumeUpButton.Click += OnKeyButtonPressed;
            this.MuteButton.Click += OnKeyButtonPressed;
            this.AltButton.Click += (s, e) =>
            {
                if (AltButton.IsChecked.Value)
                {
                    this.EnterKeyTextBox.Text = Key.LeftAlt.ToString();
                    this.rule.InputAction.Argument = Key.LeftAlt;
                    UpdateRule();
                }
            };

        }

        private void OnKeyButtonPressed(object sender, RoutedEventArgs e)
        {
            Key argument = Key.Space;
            if (sender == this.PlayPauseButton) argument = Key.MediaPlayPause;
            if (sender == this.PreviousSongButton) argument = Key.MediaPreviousTrack;
            if (sender == this.NextSongButton) argument = Key.MediaNextTrack;
            if (sender == this.PrintScrButton) argument = Key.PrintScreen;
            if (sender == this.MailButton) argument = Key.LaunchMail;
            if (sender == this.VolumeDownButton) argument = Key.VolumeDown;
            if (sender == this.VolumeUpButton) argument = Key.VolumeUp;
            if (sender == this.MuteButton) argument = Key.VolumeMute;

            this.EnterKeyTextBox.Text = argument.ToString();
            this.rule.InputAction.Argument = argument;
            UpdateRule();
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

        #endregion

        #region Mouse Rule Methods

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

        private void OnInputSelectionWindowClosed(object sender, Astral.UI.SelectionWindowEventArgs e)
        {
            this.Show();
            if (e.Reason == Astral.UI.ClosingReason.OK)
            {
                Console.WriteLine(this.deviceModel.InputRegion);
                double x = this.deviceModel.InputRegion.Left;
                double y = this.deviceModel.InputRegion.Top;
                double w = this.deviceModel.InputRegion.Width;
                double h = this.deviceModel.InputRegion.Height;
                Rect rect = new Rect(x, y, w, h);
                this.destination = rect;
                UpdateRule();
            }
        }

        private void OnSelectMouseCoordinates(object sender, RoutedEventArgs e)
        {
            this.Hide();
            this.deviceModel.ShowInputWindow();
        }

        /// <summary>
        /// For mouse move events to click when entering or leaving a range
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickRangeChanged(object sender, RoutedEventArgs e)
        {
            this.rule.ChecksBounds = this.ClickInRangeCheckBox.IsChecked.Value;
            UpdateRule();
        }

        /// <summary>
        /// To perform a click instead of just an up or down event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseClickCheckBox(object sender, RoutedEventArgs e)
        {
            if (this.MouseClickCheckBox.IsChecked.Value)
            {
                this.rule.InputAction.InputEvent = PCInputEventType.MouseClick;
            }
            else
            {
                if (this.RuleMouseButtonBar.CurrentMouseBarState == MouseButtonBarState.Down)
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

        private void OnScrollRangeSliderChanged(object sender, RangeSliderChangedEventArgs e)
        {
            if (this.Rule.InputAction.InputEvent == PCInputEventType.MouseScroll)
            {
                this.destination = new Rect(new Point(e.Low, e.Low), new Point(e.High, e.High));
                UpdateRule();
            }
        }


        #endregion

        private void OnAddRuleButtonClicked(object sender, RoutedEventArgs e)
        {
            this.rule.Name = this.RuleNameTextBox.Text;
            UpdateRule();

            this.manager.SetActiveRuleSet(this.activeRuleSetName);
            if (parentRule == null)
            {
                if(isMedley)
                {
                    this.rule.Name = "MedleyRule";
                    this.manager.AddMedley(this.rule);
                }
                else
                {
                    this.manager.AddRule(this.rule);
                }
            }
            else
            {
                // find rule by name in active rules
                // give child
                
                this.rule.ExecutedOnce = false;

                this.manager.GiveChildToRule(this.rule, parentRule.Name);
                Console.WriteLine("parent - ");
                Console.WriteLine(this.manager.ActiveRules[0]);
                Console.WriteLine();
                Console.WriteLine("child -");
                Console.WriteLine(this.manager.ActiveRules[0].Child);

                this.manager.AddRule(this.rule);
            }
            this.manager.RemoveRuleSet("temp");
            Console.WriteLine("RAISE");
            UnhookAllEvents();
            RaiseRuleAdded(new EventArgs());
            this.deviceModel.Session.InputSelectionWindowClosed -= OnInputSelectionWindowClosed;

        }

        #region Easings

        private void OnInvertCheck(object sender, RoutedEventArgs e)
        {
            this.invert = this.EasingSelector.InvertCheckBox.IsChecked.Value;
            UpdateRule();
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

        #endregion

        #region Window Related Events
        
        private void OnExit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void WindowDragClick(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        #endregion



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
            this.AccelerometerCanvas.Visibility = Visibility.Hidden;
            this.LightCanvas.Visibility = Visibility.Hidden;
        }

        private void UnhookAllEvents()
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
                        if (this.deviceModel.Device.HasAccelerometer)
                            this.deviceModel.AccelerationChanged -= OnAccelerationUpdated;
                        break;
                    case ModuleType.Gyroscope:
                        break;
                    case ModuleType.Compass:
                        if (this.deviceModel.Device.HasCompass)
                            this.deviceModel.Compass.HeadingChanged -= OnCompassUpdated;
                        break;
                    case ModuleType.Magnetometer:
                        break;
                    case ModuleType.Orientation:
                     //   this.deviceModel.Orientation.OrientationChanged -= OnOrientationChanged;
                        break;
                    case ModuleType.AmbientLight:
                        if (this.deviceModel.Device.HasAmbientLight)
                            this.deviceModel.AmbientLight.AmbientLightChanged -= OnAmbientLightUpdated;
                        break;
                    case ModuleType.Microphone:
                        if (this.deviceModel.Device.HasMicrophone)
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
                if (this.SplashCanvas.Visibility == Visibility.Visible)
                {
                    this.SplashCanvas.Visibility = Visibility.Hidden;
                }

                UnhookAllEvents();
                HideSensorCanvases();

                this.activeSensor = e.SensorType;
                switch (e.SensorType)
                {
                    case ModuleType.Display:
                        this.deviceModel.Display.TouchDown += OnTouchDown;
                        this.deviceModel.Display.TouchMove += OnTouchMoved;
                        this.deviceModel.Display.TouchUp += OnTouchUp;
                        this.TouchPlotter.TouchPlotter.DeviceResolution = new Size(this.deviceModel.Display.Width, this.deviceModel.Display.Height);
                        this.activePlotter = this.TouchPlotter.TouchPlotter;
                        this.TouchCanvas.Visibility = Visibility.Visible;
                        // change later
                        
                        this.eventType = MobileEventType.TouchMove;
                        break;
                    case ModuleType.Accelerometer:
                        this.AccelerometerCanvas.Visibility = Visibility.Visible;
                        this.eventType = MobileEventType.AccelerationChanged;
                        this.deviceModel.AccelerationChanged += OnAccelerationUpdated;
                        this.activePlotter = this.AccelerometerPlotter.Plotter;
                        this.rule.FilterInfo = this.AccNoFilter.Content.ToString();
                        break;
                    case ModuleType.Gyroscope:
                        break;
                    case ModuleType.Compass:
                        this.deviceModel.Compass.HeadingChanged += OnCompassUpdated;
                        this.CompassCanvas.Visibility = Visibility.Visible;
                        this.eventType = MobileEventType.CompassChanged;
                        this.activePlotter = Plotter;

                        break;
                    case ModuleType.Magnetometer:
                        break;
                    case ModuleType.Orientation:
                        this.deviceModel.Orientation.OrientationChanged += OnOrientationUpdated;
                        this.CompassCanvas.Visibility = Visibility.Visible;
                        this.eventType = MobileEventType.OrientationChanged;

                        break;
                    case ModuleType.AmbientLight:
                        this.eventType = MobileEventType.AmbientLightChanged;
                        this.deviceModel.AmbientLight.AmbientLightChanged += OnAmbientLightUpdated;
                        this.LightCanvas.Visibility = Visibility.Visible;
                        this.LightPlotter.Plotter.Stroke = AstralColors.Orange;
                        this.LightPlotter.Plotter.StartAtZero = true;
                        this.activePlotter = this.LightPlotter.Plotter;
                        break;
                    case ModuleType.Microphone:
                        this.eventType = MobileEventType.AmplitudeChanged;
                        this.deviceModel.Microphone.MicrophoneUpdated += OnMicrophoneUpdated;
                        this.MicCanvas.Visibility = Visibility.Visible;
                        this.MicPlotter.Plotter.MaxRange = 600;
                        this.MicPlotter.Plotter.StartAtZero = true;
                        this.activePlotter = this.MicPlotter.Plotter;
                        break;
                    default:
                        break;
                }
                UpdateRule();
            }

        }


        #region Constructors

        public RuleEditingWindow()
        {
            InitializeComponent();
            InitializeSensorButtons();

            // temp
            //this.AccelerometerPlotter.Plotter.MaxRange = 10;
        }

        public RuleEditingWindow(RuleManager ruleManager)
            : this()
        {
            this.manager = ruleManager;
                        
            this.Loaded += OnLoaded;
        }

        public RuleEditingWindow(RuleManager ruleManager, Rule parentRule)
            : this()
        {
            this.manager = ruleManager;

            this.Loaded += OnLoaded;
            this.parentRule = parentRule;
            this.NewCapScreenButton.Visibility = Visibility.Visible;
            this.NewCapScreenButton.Click += OnNewCapScreen;
        }

        Rect originalCapScreen;
        Rect newCapScreen;
        private void OnNewCapScreen(object sender, RoutedEventArgs e)
        {
            //this.deviceModel.ShowCaptureWindow();
            //this.Hide();
            //this.originalCapScreen = new Rect(deviceModel.CaptureRegion.X, deviceModel.CaptureRegion.Y, deviceModel.CaptureRegion.Width, deviceModel.CaptureRegion.Height);
            //this.deviceModel.Session.CaptureSelectionWindowClosed += CaptureWindowClosed;
        }

        private void CaptureWindowClosed(object sender, Astral.UI.SelectionWindowEventArgs e)
        {
            this.Show();
            Console.WriteLine("CAPS");
            Console.WriteLine(originalCapScreen);
            newCapScreen = new Rect(deviceModel.CaptureRegion.X, deviceModel.CaptureRegion.Y, deviceModel.CaptureRegion.Width, deviceModel.CaptureRegion.Height);
            Console.WriteLine(newCapScreen);
            this.deviceModel.CaptureRegion = this.originalCapScreen;
            this.deviceModel.Session.CaptureSelectionWindowClosed -= CaptureWindowClosed;
            this.rule.CaptureRegion = newCapScreen;
            Console.WriteLine(this.deviceModel.CaptureRegion);
        }

        #endregion

        #endregion



        private void OnTimerTick(object sender, EventArgs e)
        {
            //if (this.currentModule == ModuleType.Display)
            //{
            //    this.TouchPlotter.DrawPoints();
            //}
            //this.Plotter.DrawPoints();
            //this.MicPlotter.DrawPoints();
            this.AccelerometerPlotter.Update();
            this.LightPlotter.Update();
            this.TouchPlotter.Update();
            this.MicPlotter.Update();
            //this.AccelPlot3D.DrawPoints();
            if (this.activePlotter != null)
            {
                this.activePlotter.DrawPoints();
            }
        }

        #region Sensor Events

        private void OnAmbientLightUpdated(object sender, AstralAmbientLightEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                double light = (e.AmbientLightData.AmbientLight > 1000 ? 1000 : e.AmbientLightData.AmbientLight);
                this.LightPlotter.Plotter.PushPoint(light);
                LightBar.Width = Utils.Map(light, 0, this.LightPlotter.Plotter.MaxRange, 0, 305);
                this.LightValueLabel.Text = Math.Round(light).ToString();

                this.DebugText.Text = this.rule.ToString();
            }));
        }


        private void OnAccelerationUpdated(object sender, AccelerationDeviceModelEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                double x, y, z, mag;
                if(this.AccFilterBox.SelectedItem == this.AccNoFilter)
                {
                    rule.FilterInfo = (AccFilterBox.SelectedItem as ComboBoxItem).Content.ToString();
                    x = e.AccelerationX; y = e.AccelerationY; z = e.AccelerationZ; mag = e.Magnitude;
                }
                else if(this.AccFilterBox.SelectedItem == this.AccGravityFilter)
                {
                    rule.FilterInfo = AccFilterBox.SelectedItem.ToString();
                    x = e.GravityX; y = e.GravityY; z = e.GravityZ; mag = e.GravityMagnitude;
                }
                else
                {
                    rule.FilterInfo = AccFilterBox.SelectedItem.ToString();
                    x = e.LinearX; y = e.LinearY; z = e.LinearZ; mag = e.LinearMagnitude;
                }

                this.rule.ArgumentInfo = this.accelerationArg;
                UpdateRule();
                double value = (this.accelerationArg == RuleArgument.Magnitude ? mag : this.accelerationArg == RuleArgument.X ? x : this.accelerationArg == RuleArgument.Y ? y : z);
                this.AccelerometerPlotter.Plotter.PushPoint(value);
                this.AccelPlot3D.PushPoint(x, y, z);
            }));
        }

        private void OnMicrophoneUpdated(object sender, AstralMicrophoneEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                this.microphoneClean.ComputeAverage(e.MicrophoneData.Amplitude);
                double micValue = (microphoneClean.Average > 1000 ? 1000 : microphoneClean.Average);
                this.MicPlotter.Plotter.PushPoint(micValue);
             //   this.MicReading.Text = micValue + " :: " + MicPlotter.SelectionInRuleCoordinates.Left + " :: " + this.MicPlotter.SelectionInRuleCoordinates.Right;
            }));
        }

        private void OnTouchDown(object sender, AstralTouchEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                this.TouchPlotter.TouchPlotter.AddTouchPoint(e.TouchPoint.Id, new Point(e.TouchPoint.X, e.TouchPoint.Y));
                
            }));
        }

        private void OnOrientationUpdated(object sender, AstralOrientationEventArgs e)
        {
            this.Plotter.UpdateValue(Utils.Map(e.OrientationData.PitchDegrees, -90, 90, 270, 90));
            Dispatcher.Invoke(new Action(delegate
            {
                this.CompassTextBlock.Text = string.Format("{0:0}", e.OrientationData.PitchDegrees);
            }));
        }

        private void OnCompassUpdated(object sender, AstralCompassEventArgs e)
        {
            this.Plotter.UpdateValue(e.CompassData.Heading);
            Dispatcher.Invoke(new Action(delegate
            {
                this.CompassTextBlock.Text = string.Format("{0:0}", e.CompassData.Heading);
            }));
        }

        private void OnTouchMoved(object sender, AstralTouchEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                this.TouchPlotter.TouchPlotter.UpdatePoint(e.TouchPoint.Id, new Point(e.TouchPoint.X, e.TouchPoint.Y));

            }));
        }

        private void OnTouchUp(object sender, AstralTouchEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                this.TouchPlotter.TouchPlotter.RemovePoint(e.TouchPoint.Id, new Point(e.TouchPoint.X, e.TouchPoint.Y));
            }));
        }

        #endregion

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

            this.source = this.activePlotter.SelectionInRuleCoordinates;
            Console.WriteLine("SOURCE " + this.source);

            UpdateRule();
        }

        
    }
}
