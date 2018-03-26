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

namespace TestingConcepts
{
    /// <summary>
    /// Interaction logic for RuleDisplayControl.xaml
    /// </summary>
    public partial class RuleDisplayControl : UserControl
    {
        private Rule rule;

        public Rule RuleItem
        {
            get
            {
                return this.rule;
            }
        }

        public RuleDisplayControl()
        {
            InitializeComponent();

            this.ActiveSensor.BackgroundBox.Visibility = Visibility.Hidden;
            this.ActiveSensor.ApplyDarkTheme();
        }

        public RuleDisplayControl(Rule rule)
            : this()
        {
            this.rule = rule;
            this.RuleNameText.Text = rule.Name;
            this.ActiveSensor.SensorType = Utils.EventToModule(rule.EventType);
        }
    }
}
