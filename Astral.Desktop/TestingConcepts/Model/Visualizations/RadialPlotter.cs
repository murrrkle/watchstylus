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
    public class RadialPlotter : PlotterBase
    {
        #region Class Members
        private Pen pen = new Pen(AstralColors.Black, 2);

        private Pen anglePen = new Pen(AstralColors.Red, 4);

        private Point center;
        
        // sensor value
        private double sensorAngle = 0;

        // interaction members
        private Point currMousePosition;

        private Point prevMousePosition;

        private double startAngle = 0.0;

        private double endAngle = 0.0;
        #endregion

        #region Properties
        public double Radius
        {
            get
            {
                return this.Width / 2;
            }
        }

        public Point Center
        {
            get
            {
                return this.center;
            }
        }
        #endregion

        #region Constructors
        public RadialPlotter()
        {
            this.MouseMove += OnMouseMove;
            this.MouseLeftButtonDown += OnMouseLeftDown;
            this.MouseLeftButtonUp += OnMouseLeftUp;

            this.pen.DashStyle = DashStyles.Dot;
            this.Loaded += OnLoaded;
        }

        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            base.OnLoaded(this, e);
            this.center = new Point(this.Width / 2, this.Height / 2);
        }
        #endregion

        #region Mouse Events
        protected override void OnMouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            this.isMouseDown = true;

            // get the mouse position (shifted to center)
            this.mousePosition = e.GetPosition(this);

            this.currMousePosition = AdjustMousePosition(this.mousePosition);
            this.prevMousePosition = this.currMousePosition;

            // set the selected range (in degrees), both are the same
            double angle = Utils.RadiansToDegrees(MousePointToAngle(this.mousePosition));
            this.startAngle = angle;
            this.endAngle = angle;
        }

        protected override void OnMouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            this.isMouseDown = false;
            this.mousePosition = e.GetPosition(this);

            UpdateSelection();

            // this will only be here for now while we debug
            //  DrawPoints();

            OnSelectionChanged(new SelectionEventArgs(this.selection));
        }


        protected override void OnMouseMove(object sender, MouseEventArgs e)
        {
            this.mousePosition = e.GetPosition(this);
            this.currMousePosition = AdjustMousePosition(this.mousePosition);

            if (this.isMouseDown)
            {
                // get the delta angle (to previous position)
                double deltaAngle = Vector.AngleBetween((Vector)this.currMousePosition, (Vector)this.prevMousePosition);

                this.endAngle += deltaAngle;
                this.prevMousePosition = this.currMousePosition;

                // check the range (could technically also be done while drawing)
                if (this.endAngle > this.startAngle)
                {
                    while (this.endAngle - this.startAngle >= 360.0)
                    {
                        this.endAngle -= 360.0;
                    }

                    // rounding errors
                    if (this.endAngle < this.startAngle)
                    {
                        this.endAngle = this.startAngle;
                    }
                }
                else
                {
                    while (this.startAngle - this.endAngle >= 360.0)
                    {
                        this.endAngle += 360.0;
                    }

                    // rounding errors
                    if (this.endAngle > this.startAngle)
                    {
                        this.endAngle = this.startAngle;
                    }
                }

                UpdateSelection();
            }

            // this will only be here for now while we debug
            //  DrawPoints();
        }

        #region Selection
        private void UpdateSelection()
        {
            double start = Math.Min(this.startAngle, this.endAngle);
            double end = Math.Max(this.startAngle, this.endAngle);

            start %= 360.0;
            end %= 360.0;

            while (start > end)
            {
                start -= 360.0;
            }

            this.selection = new Rect(new Point(start, start), new Point(end, end));
        }
        #endregion

        #region Arc Stuff
        protected Point AdjustMousePosition(Point mousePosition)
        {
            return (mousePosition - ((Vector)this.center));
        }

        protected double MousePointToAngle(Point p)
        {
            // convert point into unit circle value
            double x = Utils.Map(p.X, 0, this.Width, -1, 1);
            double y = Utils.Map(p.Y, 0, this.Height, 1, -1);

            double angle = Math.Atan(y / x);
            angle = (x > 0 && y < 0) ? Math.PI * 2 - Math.Abs(angle) : (x < 0 && y > 0) ? Math.PI - Math.Abs(angle) : (x < 0 && y < 0) ? Math.PI + angle : angle;
            return angle;
        }

        protected Point PointFromAngle(double angle)
        {
            double x = Utils.Map(Math.Cos(angle), -1, 1, 0, this.Width);
            double y = Utils.Map(Math.Sin(angle), 1, -1, 0, this.Height);
            return new Point(x, y);
        }
        
        private GeometryDrawing DrawArc(double startAngle, double endAngle)
        {
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext context = geometry.Open())
            {
                context.BeginFigure(Center, true, true);
                PointCollection points = new PointCollection();
                points.Add(PointFromAngle(startAngle));
                
                if(startAngle < endAngle)
                {
                    double angle = startAngle;
                    while (angle < endAngle)
                    {
                        points.Add(PointFromAngle(angle));
                        angle += Math.PI / 60;
                    }
                }

                if(startAngle > endAngle)
                {
                    double angle = startAngle;
                    while (angle > endAngle)
                    {
                        points.Add(PointFromAngle(angle));
                        angle -= Math.PI / 60;
                    }
                }

                Point p1 = PointFromAngle(endAngle);

                context.PolyLineTo(points, true, true);

            }
            geometry.Freeze();

            GeometryDrawing drawing = new GeometryDrawing();
            drawing.Geometry = geometry;
            drawing.Brush = AstralColors.Yellow;

            return drawing;
        }

        #endregion

        public override void DrawPoints()
        {
            // GetMouseAngle();

            double mouseAngle = MousePointToAngle(this.mousePosition);
            Point endPoint = PointFromAngle(mouseAngle);

            using (var context = this.visual.RenderOpen())
            {
                // redraw background
                context.DrawRectangle(AstralColors.White, null, new Rect(0, 0, this.Width, this.Height));

                // draw the ellipse
                context.DrawEllipse(AstralColors.LightGray, null, this.Center, Radius, Radius);

                // draw the selection
                context.DrawDrawing(DrawArc(Utils.DegreesToRadians(this.startAngle), 
                    Utils.DegreesToRadians(this.endAngle)));

                // draw dotted line representing current mouse intersection
                context.DrawLine(this.pen, center, endPoint);

                // draw sensor
                context.DrawLine(this.anglePen, this.center, PointFromAngle(Utils.DegreesToRadians(sensorAngle)));

                context.Close();
            }
            this.content.Render(visual);
        }

        #endregion
        
        public void UpdateValue(double sensorAngleInDegrees)
        {
            this.sensorAngle = sensorAngleInDegrees;
        }

    }
}
