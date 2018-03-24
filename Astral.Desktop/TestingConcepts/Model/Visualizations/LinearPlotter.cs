using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TestingConcepts
{
    public enum LinearPlotterDimension
    {
        Magnitude,
        X,
        Y,
        Z
    }

    public class LinearPlotter : LinearPlotterBase
    {
        public LinearPlotterDimension Dimension
        {
            get
            {
                return (LinearPlotterDimension)this.GetValue(DimensionProperty);
            }
            set
            {
                this.SetValue(DimensionProperty, value);
            }
        }

        public static readonly DependencyProperty DimensionProperty =
            DependencyProperty.Register("Dimension", typeof(LinearPlotterDimension), 
                typeof(LinearPlotter), new PropertyMetadata(LinearPlotterDimension.Magnitude), OnDimensionChanged);

        private static bool OnDimensionChanged(object value)
        {
            Console.WriteLine("Changed");
            return true;
        }
        

        public LinearPlotter(LinearPlotterDimension dimension)
            : base()
        {
            this.Dimension = dimension;
        }

        public LinearPlotter()
            : base()
        {
            
        }

        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            
            switch (this.Dimension)
            {
                case LinearPlotterDimension.Magnitude:
                    this.Stroke = AstralColors.Blue;
                    break;
                case LinearPlotterDimension.X:
                    this.Stroke = AstralColors.Red;
                    break;
                case LinearPlotterDimension.Y:
                    this.Stroke = AstralColors.Orange;
                    break;
                case LinearPlotterDimension.Z:
                    this.Stroke = AstralColors.Teal;
                    break;
            }
            base.OnLoaded(sender, e);
        }

    }
}
