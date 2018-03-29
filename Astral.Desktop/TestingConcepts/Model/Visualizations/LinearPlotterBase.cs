using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TestingConcepts
{
    public class LinearPlotterBase : PlotterBase
    {

        protected SolidColorBrush stroke = AstralColors.Red;
        protected Pen defaultPen;

        protected int numberOfValues = 30;
        protected double maxRange = 10;
        protected List<double> points = new List<double>();

        protected double offset = 0;

        protected bool isMaxManuallySet = false;
        protected bool startAtZero = false;

        public SolidColorBrush Stroke
        {
            get
            {
                return this.stroke;
            }
            set
            {
                this.stroke = value;
                this.defaultPen = new Pen(stroke, 2);
            }
        }

        public bool StartAtZero
        {
            get
            {
                return this.startAtZero;
            }
            set
            {
                this.startAtZero = value;
            }
        }

        public double MaxRange
        {
            get
            {
                return this.maxRange;
            }
            set
            {
                this.maxRange = value;
                this.isMaxManuallySet = true;
            }
        }

        public LinearPlotterBase()
            : base()
        {

        }

        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            // subtract one from number of values because we start at 0, not 1
            this.offset = this.Width / (numberOfValues - 1);
            base.OnLoaded(sender, e);
        }

        public void PushPoint(double value)
        {
            if (Math.Abs(value) > this.maxRange && !this.isMaxManuallySet)
            {
                this.maxRange = value;
            }

            double newValue = 0;
            if(!startAtZero)
            {
                newValue = Utils.Map(value, -this.maxRange, this.maxRange, this.Height, 0);
            }
            else
            {
                newValue = Utils.Map(value, 0, this.maxRange, this.Height, 0);
            }
            points.Add(newValue);

            if (points.Count > this.numberOfValues)
            {
                points.RemoveAt(0);
            }
            DrawPoints();
        }


        protected unsafe void DrawPointsForList(DrawingContext dc, double[] points, Pen color)
        {
            if (points.Length > 1)
            {
                fixed(double* pointerToPoints = points)
                {
                    double* currentPoint = (double*)pointerToPoints;
                    Point begin = new Point(0, *currentPoint);
                    currentPoint++;
                    Point initial = new Point(offset, *currentPoint);
                    dc.DrawLine(color, begin, initial);

                    for(int i = 1; i < points.Length; i++)
                    {
                        Point start = new Point(i * offset, *currentPoint);
                        Point end = new Point((i + 1) * offset, *(currentPoint+1));
                        dc.DrawLine(color, start, end);
                        currentPoint++;
                    }
                }
            }
        }

        public virtual void DrawPoints()
        {

            using (var context = this.visual.RenderOpen())
            {
                // redraw background
                context.DrawRectangle(AstralColors.LightGray, null, new Rect(0, 0, this.Width, this.Height));

                DrawPointsForList(context, this.points.ToArray(), this.defaultPen);
                context.Close();
            }

            this.content.Render(visual);

        }

    }
}
