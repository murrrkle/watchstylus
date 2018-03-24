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
    /// Interaction logic for Linear3DSensorVisualizationSet.xaml
    /// </summary>
    public partial class Linear3DSensorVisualizationSet : UserControl
    {

        public string SensorName
        {
            get
            {
                return this.SensorNameLabel.Text;
            }
            set
            {
                this.SensorNameLabel.Text = value;
            }
        }

        public Linear3DSensorVisualizationSet()
        {
            InitializeComponent();
        }

        public void UpdateValues(double x, double y, double z)
        {

            double magnitude = Utils.Magnitude(x, y, z);
            
            this.PlotterX.PushPoint(x);
            this.PlotterY.PushPoint(y);
            this.PlotterZ.PushPoint(z);
            this.PlotterMagnitude.PushPoint(magnitude);
            this.PlotterXYZ.PushPoint(x, y, z);

            this.XText.Text = String.Format("{0:0.0}", x);
            this.YText.Text = String.Format("{0:0.0}", y);
            this.ZText.Text = String.Format("{0:0.0}", z);
            this.MagnitudeText.Text = String.Format("{0:0.0}", magnitude);
        }
    }
}
