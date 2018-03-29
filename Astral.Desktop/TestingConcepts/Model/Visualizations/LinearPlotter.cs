using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Input;

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
        
        public override Rect SelectionInRuleCoordinates
        {
            get
            {
                double min = (this.StartAtZero ? 0 : -this.maxRange);
                double yStart = Math.Round(Utils.Map(this.selection.Top, this.Height, 0, min, this.maxRange));
                double yEnd = Math.Round(Utils.Map(this.selection.Bottom, this.Height, 0, min, this.maxRange));
                double width = Math.Abs(yStart - yEnd);
                double rangeStart = Math.Min(yStart, yEnd);
                return new Rect(rangeStart, rangeStart, width, width);
            }
        }


        public LinearPlotter(LinearPlotterDimension dimension)
            : base()
        {
            this.Dimension = dimension;
            Initialize();
        }

        public LinearPlotter()
            : base()
        {
            Initialize();
        }

        protected void Initialize()
        {
            this.MouseMove += OnMouseMove;
            this.MouseLeftButtonDown += OnMouseLeftDown;
            this.MouseLeftButtonUp += OnMouseLeftUp;
        }

        #region Mouse Events

        protected override void OnMouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            this.mousePosition.X = this.Width;
            this.mousePosition.Y = e.GetPosition(this).Y;
            this.selection = new Rect(this.boundingBoxStart, this.mousePosition);
            OnSelectionChanged(new SelectionEventArgs(this.selection));
            this.isMouseDown = false;
        }

        protected override void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (this.isMouseDown)
            {
                double x = e.GetPosition(this).X;
                double y = e.GetPosition(this).Y;
                this.selection = new Rect(this.boundingBoxStart, this.mousePosition);
                this.mousePosition.X = this.Width;
                this.mousePosition.Y = y;
            }
        }
        
        protected override void OnMouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            this.isMouseDown = true;
            this.mousePosition = new Point(this.Width, e.GetPosition(this).Y);
            this.boundingBoxStart = new Point(0, e.GetPosition(this).Y);
        }


        #endregion

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

        public override void DrawPoints()
        {

            using (var context = this.visual.RenderOpen())
            {
                // redraw background
                context.DrawRectangle(AstralColors.LightGray, null, new Rect(0, 0, this.Width, this.Height));

                // draw bounding box
                context.DrawRectangle(AstralColors.Yellow, null, this.selection);

                DrawPointsForList(context, this.points.ToArray(), this.defaultPen);
                context.Close();
            }

            this.content.Render(visual);

        }

    }
}
