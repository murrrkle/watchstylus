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
    /// Interaction logic for LinearPlotterWrapper.xaml
    /// </summary>
    public partial class LinearPlotterWrapper : UserControl
    {
        private TranslateTransform yPos = new TranslateTransform(0, 0);
        private double low;
        private double high;
        private double cursor  = 0;
        private double currentValue;

        public LinearPlotterWrapper()
        {
            InitializeComponent();

            this.MarkerCanvas.Visibility = Visibility.Hidden;
            this.Plotter.MouseEnter += OnMouseEnterPlotter;
            this.Plotter.MouseLeave += OnMouseLeavePlotter;
            this.Plotter.MouseMove += OnPlotterMouseMove;
            this.MarkerCanvas.RenderTransform = yPos;

        }

        public void Update()
        {

            this.low = (this.Plotter.StartAtZero ? 0 : -this.Plotter.MaxRange);
            this.high = this.Plotter.MaxRange;
            this.MarkerLow.Text = Math.Round(this.low).ToString();
            this.MarkerHigh.Text = Math.Round(this.high).ToString();
            this.currentValue = Utils.Map(cursor, this.Plotter.Height, 0, this.low, this.high);
            this.MarkerActual.Text = Math.Round(this.currentValue).ToString();
            this.Plotter.DrawPoints();

        }

        private void OnMouseLeavePlotter(object sender, MouseEventArgs e)
        {
            this.MarkerCanvas.Visibility = Visibility.Hidden;
        }

        private void OnPlotterMouseMove(object sender, MouseEventArgs e)
        {
            this.cursor = e.GetPosition(this.Plotter).Y;
            yPos.Y = e.GetPosition(this.Plotter).Y - 11;
        }

        private void OnMouseEnterPlotter(object sender, MouseEventArgs e)
        {
            this.MarkerCanvas.Visibility = Visibility.Visible;
        }
    }
}
