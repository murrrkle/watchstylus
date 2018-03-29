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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TestingConcepts
{
    /// <summary>
    /// Interaction logic for AstralWindow.xaml
    /// </summary>
    public partial class AstralWindow : Window
    {
        private NetworkManager networkManager;
        private RuleManager ruleManager;
        private RuleEditingWindow ruleEditingWindow;

        public AstralWindow()
        {
            InitializeComponent();

            this.ruleManager = RuleManager.Instance;

            this.networkManager = NetworkManager.Instance;
            this.networkManager.Start();
            this.networkManager.DeviceAdded += OnDeviceConnected;

            //Dispatcher.Invoke(new Action(delegate
            //{
             this.ruleEditingWindow = new RuleEditingWindow(this.ruleManager);
            //}));

            this.AddRuleSetCanvas.Visibility = Visibility.Hidden;

            this.RuleSetsTab.AddRuleSetButton.Click += AddRuleSetButtonClicked;
            this.RuleSetsTab.RuleSetChanged += OnRuleSetChanged;

            this.NewRuleSetOkay.Click += OnNewSetRuleOkayClicked;
            this.NewRuleSetCancel.Click += OnNewSetRuleCancelClicked;
            this.NewRuleSetNameTextBox.PreviewKeyDown += OnNewRuleSetNameTextBoxKeyDown;

            this.RuleSetsTab.AddRuleSetTab("Default");
            this.ruleManager.AddRuleSet("Default");
            this.ruleManager.SetActiveRuleSet("Default");

            this.AddRuleButton.Click += OnAddNewRule;
            this.Closed += OnClosed;

            this.ruleEditingWindow.RuleAdded += OnRuleAdded;

            this.DebugText.Visibility = Visibility.Hidden;
            this.MouseRightButtonDown += (s, e) => { this.DebugText.Visibility = (this.DebugText.Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden); };
        }

        private void OnRuleAdded(object sender, EventArgs e)
        {
            this.DebugText.Text = "";
            this.ActiveRuleContainer.Children.Clear();
            foreach(Rule r in this.ruleManager.ActiveRules)
            {
                if(r.Name != null || r.Name != "")
                {
                    RuleDisplayControl ruleDisplay = new RuleDisplayControl(r);
                    this.ActiveRuleContainer.Children.Add(ruleDisplay);
                    this.DebugText.Text += r.ToString() + "\n";
                }
            }
            this.ruleEditingWindow.Close();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnDeviceConnected(object sender, DeviceConnectedEventArgs e)
        {
            DeviceModel deviceModel = new DeviceModel(e.Device, e.Session);
            this.ruleManager.DeviceModel = deviceModel;
            this.ruleEditingWindow.DeviceModel = deviceModel;
            Dispatcher.Invoke(new Action(delegate
            {
                this.DeviceInfoText.Text = e.Device.Class + " Connected";
                if(e.Device.Class.ToLower().Contains("nexus"))
                {
                    this.AndroidLogoIcon.Visibility = Visibility.Visible;
                    this.PhoneIcon.Visibility = Visibility.Visible;
                }
                else if(e.Device.Class.ToLower().Contains("iphone"))
                {
                    this.PhoneIcon.Visibility = Visibility.Visible;
                    this.AppleLogoIcon.Visibility = Visibility.Visible;
                }
                else if(e.Device.Class.ToLower().Contains("watch"))
                {
                    this.WatchIcon.Visibility = Visibility.Visible;
                }
            }));

        }

        private void OnAddNewRule(object sender, RoutedEventArgs e)
        {
            this.ruleEditingWindow = new RuleEditingWindow(this.ruleManager);
            this.ruleEditingWindow.RuleAdded += OnRuleAdded;
            this.ruleEditingWindow.DeviceModel = ruleManager.DeviceModel;
            this.ruleEditingWindow.Show();
        }

        #region AddRuleCanvas UI Handling

        private void OnNewRuleSetNameTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.RuleSetsTab.AddRuleSetTab(this.NewRuleSetNameTextBox.Text.Replace("\n", ""));
                HideAddRuleSetCanvas();
            }
        }

        private void OnNewSetRuleCancelClicked(object sender, RoutedEventArgs e)
        {
            HideAddRuleSetCanvas();
        }

        private void HideAddRuleSetCanvas()
        {
            Storyboard disappear = this.FindResource("AddRuleSetCanvasDisappear") as Storyboard;
            disappear.Completed += (s, e) => { this.AddRuleSetCanvas.Visibility = Visibility.Hidden; };
            disappear.Begin();
        }

        private void OnNewSetRuleOkayClicked(object sender, RoutedEventArgs e)
        {
            string newRuleSetName = this.NewRuleSetNameTextBox.Text;
            this.RuleSetsTab.AddRuleSetTab(newRuleSetName);
            HideAddRuleSetCanvas();

            if (!this.ruleManager.ContainsRuleSet(newRuleSetName))
            {
                this.ruleManager.AddRuleSet(newRuleSetName);
            }
        }

        #endregion

        /// <summary>
        /// When the active ruleset is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRuleSetChanged(object sender, RuleSetEventArgs e)
        {
            this.ruleManager.SetActiveRuleSet(e.Name);
        }

        private void AddRuleSetButtonClicked(object sender, RoutedEventArgs e)
        {
            this.AddRuleSetCanvas.Visibility = Visibility.Visible;
            Storyboard appear = this.FindResource("AddRuleSetCanvasAppear") as Storyboard;
            appear.Begin();
        }
    }
}
