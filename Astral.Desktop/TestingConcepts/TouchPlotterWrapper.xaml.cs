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
    /// Interaction logic for TouchPlotterWrapper.xaml
    /// </summary>
    public partial class TouchPlotterWrapper : UserControl
    {
        private TranslateTransform xPos = new TranslateTransform();
        private TranslateTransform yPos = new TranslateTransform();
        private Point cursor = new Point(0, 0);
        private double xMarker;
        private double yMarker;


        public TouchPlotterWrapper()
        {
            InitializeComponent();

            this.MarkerCanvasX.Visibility = Visibility.Hidden;
            this.MarkerCanvasY.Visibility = Visibility.Hidden;

            this.TouchPlotter.MouseEnter += OnMouseEnterPlotter;
            this.TouchPlotter.MouseLeave += OnMouseLeavePlotter;
            this.TouchPlotter.MouseMove += OnMouseMovePlotter;

            this.MarkerCanvasX.RenderTransform = this.xPos;
            this.MarkerCanvasY.RenderTransform = this.yPos;
        }

        public void Update()
        {
            this.xMarker = Utils.Map(this.cursor.X, 0, this.TouchPlotter.Width, 0, this.TouchPlotter.DeviceResolution.Width);
            this.yMarker = Utils.Map(this.cursor.Y, 0, this.TouchPlotter.Height, 0, this.TouchPlotter.DeviceResolution.Height);
            this.MarkerActualX.Text = Math.Round(xMarker).ToString();
            this.MarkerActualY.Text = Math.Round(yMarker).ToString();

            this.TouchPlotter.DrawPoints();
        }

        private void OnMouseMovePlotter(object sender, MouseEventArgs e)
        {
            this.cursor = e.GetPosition(this.TouchPlotter);
            
            this.xPos.X = this.cursor.X -13;
            this.yPos.Y = this.cursor.Y - 11;
        }

        private void OnMouseLeavePlotter(object sender, MouseEventArgs e)
        {
            this.MarkerCanvasX.Visibility = Visibility.Hidden;
            this.MarkerCanvasY.Visibility = Visibility.Hidden;
        }

        private void OnMouseEnterPlotter(object sender, MouseEventArgs e)
        {
            this.MarkerCanvasX.Visibility = Visibility.Visible;
            this.MarkerCanvasY.Visibility = Visibility.Visible;
        }
    }
}
