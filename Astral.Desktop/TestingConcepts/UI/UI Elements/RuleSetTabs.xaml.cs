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
    public class RuleSetEventArgs : EventArgs
    {
        private string name;

        public string Name { get => this.name; set => this.name = value; }

        public RuleSetEventArgs(string name)
        {
            this.name = name;
        }
    }

    /// <summary>
    /// Interaction logic for RuleSetTabs.xaml
    /// </summary>
    public partial class RuleSetTabs : UserControl
    {
        public event EventHandler<RuleSetEventArgs> RuleSetChanged;

        private void RaiseRuleSetChanged(RuleSetEventArgs e)
        {
            if(this.RuleSetChanged != null)
            {
                this.RuleSetChanged(this, e);
            }
        }

        public RuleSetTabs()
        {
            InitializeComponent();
            this.RuleSetsPanel.Children.Clear();
            
        }

        public void AddRuleSetTab(string name)
        {
            RuleSetItem item = new RuleSetItem(name);
            item.RenderTransform = new RotateTransform(180, item.Width/2, item.Height/2);
            item.Click += OnItemClicked;

            this.RuleSetsPanel.Children.Add(item);
            if(this.RuleSetsPanel.Children.Count == 1)
            {
                item.IsChecked = true;
            }
        }

        private void OnItemClicked(object sender, MouseEventArgs e)
        {
            RuleSetItem selection = sender as RuleSetItem;
            foreach(RuleSetItem item in this.RuleSetsPanel.Children)
            {
                if(item != selection)
                {
                    item.IsChecked = false;
                }
                else
                {
                    item.IsChecked = true;
                }
            }

            RaiseRuleSetChanged(new RuleSetEventArgs(selection.RuleName));
        }
    }
}
