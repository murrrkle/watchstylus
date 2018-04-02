using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class RangeSliderChangedEventArgs : EventArgs
    {
        public double Low { get; set; }
        public double High { get; set; }
        public double Range
        {
            get
            {
                return Math.Abs(this.High - this.Low);
            }
        }

        public RangeSliderChangedEventArgs(double low, double high)
        {
            this.Low = low;
            this.High = high;
        }
    }


    /// <summary>
    /// Interaction logic for RangeSlider.xaml
    /// </summary>
    public partial class RangeSlider : UserControl
    {
        private Rectangle selection = null;
        private double rangeClickOffset;

        private double minimum = -250;
        private double maximum = 250;
        private double currentMin;
        private double currentMax;

        public event EventHandler<RangeSliderChangedEventArgs> Changed;

        private void RaiseChanged(RangeSliderChangedEventArgs e)
        {
            if(this.Changed != null)
            {
                Changed(this, e);
            }
        }

        public RangeSlider()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
            this.currentMin = -100;
            this.currentMax = 100;

            this.LowerBoundTextBox.Visibility = Visibility.Hidden;
            this.UpperBoundTextBox.Visibility = Visibility.Hidden;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {

            // prevent the Visual Studio designer from drawing null things
            // stop rendering on the designer view
            #if DEBUG
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;
            #endif

            Window.GetWindow(this).MouseMove += OnWindowMouseMove;
            Window.GetWindow(this).MouseLeftButtonUp += OnBoundClickReleased;

            this.LowerBoundRect.MouseLeftButtonDown += OnBoundClicked;
            this.UpperBoundRect.MouseLeftButtonDown += OnBoundClicked;
            this.RangeRect.MouseLeftButtonDown += OnBoundClicked;

            this.LowerBoundText.MouseLeftButtonDown += OnBoundTextClicked;
            this.UpperBoundText.MouseLeftButtonDown += OnBoundTextClicked;

            this.LowerBoundTextBox.KeyUp += OnTextBoxEnterReleased;
            this.UpperBoundTextBox.KeyUp += OnTextBoxEnterReleased;

            UpdateMaxMinFromValues();

        }

        private void OnTextBoxEnterReleased(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                TextBox castSender = sender as TextBox;
                string textContent = castSender.Text;

                double newValue;
                if(Double.TryParse(castSender.Text, out newValue))
                {
                    if (castSender == this.LowerBoundTextBox)
                    {
                        this.currentMin = newValue;
                    }
                    else 
                    {
                        this.currentMax = newValue;
                    }
                    UpdateMaxMinFromValues();

                }
                castSender.Visibility = Visibility.Hidden;

            }
        }

        private void OnBoundTextClicked(object sender, MouseButtonEventArgs e)
        {
            TextBlock castSender = sender as TextBlock;
            if (castSender == this.LowerBoundText)
            {
                this.LowerBoundTextBox.Visibility = Visibility.Visible;
                this.LowerBoundTextBox.Text = this.LowerBoundText.Text;
                this.LowerBoundTextBox.Focus();
                this.LowerBoundTextBox.SelectAll();
            }
            else if (castSender == this.UpperBoundText)
            {
                this.UpperBoundTextBox.Visibility = Visibility.Visible;
                this.UpperBoundTextBox.Text = this.UpperBoundText.Text;
                this.UpperBoundTextBox.Focus();
                this.UpperBoundTextBox.SelectAll();
            }
            
        }

        private void OnBoundClickReleased(object sender, MouseButtonEventArgs e)
        {
            this.selection = null;

        }

        private void OnBoundClicked(object sender, MouseButtonEventArgs e)
        {
            this.selection = sender as Rectangle;
            if(this.selection == this.RangeRect)
            {
                this.rangeClickOffset = e.GetPosition(this.Container).X - Canvas.GetLeft(this.RangeRect);
            }
        }

        private void UpdateMaxMinFromValues()
        {
            double minPosition = Utils.Map(this.currentMin, this.minimum, this.maximum, 0, this.BackgroungRect.Width);
            Canvas.SetLeft(this.LowerBoundRect, minPosition);

            double maxPosition = Utils.Map(this.currentMax, this.minimum, this.maximum, 0, this.BackgroungRect.Width);
            Canvas.SetLeft(this.UpperBoundRect, maxPosition);
            
            this.LowerBoundText.Text = String.Format("{0:0.##}", this.currentMin);
            this.UpperBoundText.Text = String.Format("{0:0.##}", this.currentMax);

            MoveRangeRectangle();

            RaiseChanged(new RangeSliderChangedEventArgs(this.currentMin, this.currentMax));
        }

        private void UpdateMaxMin()
        {
            double minPosition = Canvas.GetLeft(this.LowerBoundRect);
            this.currentMin = Utils.Map(minPosition, 0, this.BackgroungRect.Width, this.minimum, this.maximum);

            double maxPosition = Canvas.GetLeft(this.UpperBoundRect);
            this.currentMax = Utils.Map(maxPosition, 0, this.BackgroungRect.Width, this.minimum, this.maximum);
            
            this.LowerBoundText.Text = String.Format("{0:0.##}", this.currentMin);
            this.UpperBoundText.Text = String.Format("{0:0.##}", this.currentMax);


            RaiseChanged(new RangeSliderChangedEventArgs(this.currentMin, this.currentMax));
        }

        private void MoveRangeRectangle()
        {
            double startX = Canvas.GetLeft(this.LowerBoundRect);
            double endX = Canvas.GetLeft(this.UpperBoundRect);
            Canvas.SetLeft(this.RangeRect, startX);
            if (endX - startX > 0)
            {
                this.RangeRect.Width = endX - startX;
            }
        }

        private void OnWindowMouseMove(object sender, MouseEventArgs e)
        {
            if(selection != null)
            {
                Canvas.SetLeft(selection, e.GetPosition(this.Container).X);


                if (this.selection != this.RangeRect)
                {
                    MoveRangeRectangle();
                }
                else
                {
                    Canvas.SetLeft(this.LowerBoundRect, e.GetPosition(this.Container).X - this.rangeClickOffset);
                    Canvas.SetLeft(this.UpperBoundRect, e.GetPosition(this.Container).X + this.RangeRect.Width - this.rangeClickOffset);
                    Canvas.SetLeft(this.RangeRect, e.GetPosition(this.Container).X - this.rangeClickOffset);
                }
                UpdateMaxMin();
            }
        }
    }
}
