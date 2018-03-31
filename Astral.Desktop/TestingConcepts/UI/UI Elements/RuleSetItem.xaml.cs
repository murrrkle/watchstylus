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
    /// Interaction logic for RuleSetItem.xaml
    /// </summary>
    public partial class RuleSetItem : UserControl
    {
        private bool isChecked = false;
        private string ruleName = "rule name";

        public bool IsChecked
        {
            get
            {
                return this.isChecked;
            }
            set
            {
                this.isChecked = value;
                SetVisualFromCheckState();
            }
        }
        
        public string RuleName
        {
            get
            {
                return this.ruleName;
            }
            set
            {
                this.ruleName = value;
                this.RuleSetLabel.Text = this.ruleName.ToUpper();
            }
        }

        public event EventHandler<MouseEventArgs> Click;

        private void RaiseClick(MouseEventArgs e)
        {
            if(Click != null)
            {
                Click(this, e);
            }
        }

        public RuleSetItem()
        {
            InitializeComponent();

            this.Container.MouseLeftButtonDown += OnMouseLeftDown;
            this.MouseEnter += OnMouseEnter;
            this.MouseLeave += OnMouseLeave;
        }

        public RuleSetItem(string Name)
            : this()
        {
            this.RuleName = Name;
        }


        public void SetVisualFromCheckState()
        {

            if(this.isChecked)
            {
                this.BGRectangle.Fill = AstralColors.Teal;
                this.RuleSetLabel.Foreground = AstralColors.White;
            }
            else
            {
                this.BGRectangle.Fill = AstralColors.LightGray;
                this.RuleSetLabel.Foreground = AstralColors.Black;
            }

        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            SetVisualFromCheckState();
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            this.BGRectangle.Fill = AstralColors.Yellow;
            this.RuleSetLabel.Foreground = AstralColors.White;
        }

        private void OnMouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            this.IsChecked = !this.isChecked;
            this.RaiseClick(e);
        }
    }
}
