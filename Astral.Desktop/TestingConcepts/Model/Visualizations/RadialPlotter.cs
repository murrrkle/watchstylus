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
        private Pen pen = new Pen(AstralColors.Black, 2);
        private Pen anglePen = new Pen(AstralColors.Red, 4);

        private Point center;
        private double mouseAngle = 0;
        private double lastAngle = 0;
        private double sensorAngle = 0;

        private double selectionStartAngle;
        private double selectionEndAngle;

        private bool increasing = false;
        private bool crossOver = false;
        private bool selectionCrossed = false;


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


        public bool SelectionCrosses
        {
            get
            {
                return this.selectionCrossed;
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
            this.mousePosition = e.GetPosition(this);
            this.selectionStartAngle = MousePointToAngle(this.mousePosition);
        }

        protected override void OnMouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            this.isMouseDown = false;
            this.mousePosition = e.GetPosition(this);
            this.crossOver = false;
            this.selectionEndAngle = this.mouseAngle;
            
            double start = Utils.RadiansToDegrees(this.selectionStartAngle);
            double end = Utils.RadiansToDegrees(this.selectionEndAngle);
            this.selection = new Rect(new Point(start, start), new Point(end, end));

            // this will only be here for now while we debug
          //  DrawPoints();

            OnSelectionChanged(new SelectionEventArgs(this.selection));
        }


        protected override void OnMouseMove(object sender, MouseEventArgs e)
        {
            this.mousePosition = e.GetPosition(this);
            if (this.isMouseDown)
            {
                if (lastAngle > mouseAngle)
                {
                    this.increasing = false;
                }
                else
                {
                    this.increasing = true;
                }

                this.lastAngle = this.mouseAngle;
                this.selectionEndAngle = this.mouseAngle;

                double start = Utils.RadiansToDegrees(this.selectionStartAngle);
                double end = Utils.RadiansToDegrees(this.selectionEndAngle);
                this.selection = new Rect(new Point(start, start), new Point(end, end));
            }
            
            // this will only be here for now while we debug
          //  DrawPoints();
        }

        #region Arc Stuff

        protected void GetMouseAngle()
        {
            this.mouseAngle = MousePointToAngle(this.mousePosition);
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
            GetMouseAngle();
            Point endPoint = PointFromAngle(this.mouseAngle);

            using (var context = this.visual.RenderOpen())
            {
                // redraw background
                context.DrawRectangle(AstralColors.White, null, new Rect(0, 0, this.Width, this.Height));

                // draw the ellipse
                context.DrawEllipse(AstralColors.LightGray, null, this.Center, Radius, Radius);

                // questionably draw the selection
                if (this.selection != null)
                {
                    // OMG HAXX !!11!1!
                    if (increasing)
                    {
                        if (Math.Abs(lastAngle - mouseAngle) > 5)
                        {
                            crossOver = true;
                        }

                    }
                    if (crossOver)
                    {
                        double lowAngle = Math.Min(this.selectionStartAngle, this.selectionEndAngle);
                        double highAngle = Math.Max(this.selectionStartAngle, this.selectionEndAngle);
                        context.DrawDrawing(DrawArc(0, lowAngle));
                        context.DrawDrawing(DrawArc(highAngle, (Math.PI * 2) - 0.0000001));
                    }
                    else
                    {
                        context.DrawDrawing(DrawArc(this.selectionStartAngle, this.selectionEndAngle));
                    }
                }



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
