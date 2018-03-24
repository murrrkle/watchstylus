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
    public class MultiTouchPlotter : PlotterBase
    {
        protected Size deviceResolution = new Size(300, 300);

        protected Dictionary<int, Point> touchPoints = new Dictionary<int, Point>();

        public Size DeviceResolution
        {
            get
            {
                return this.DeviceResolution;
            }
            set
            {
                this.deviceResolution = value;
                ResizePlotter();
            }
        }

        public Rect SelectionInDeviceCoords
        {
            get
            {
                double topX = Utils.Map(Math.Min(Selection.TopLeft.X, Selection.TopRight.X), 0, this.Width, 0, this.deviceResolution.Width);
                double topY = Utils.Map(Math.Min(Selection.TopLeft.Y, Selection.BottomLeft.Y), 0, this.Height, 0, this.deviceResolution.Height);
                double selectionWidth = Utils.Map(Selection.Width, 0, this.Width, 0, this.deviceResolution.Width);
                double selectionHeight = Utils.Map(Selection.Height, 0, this.Height, 0, this.deviceResolution.Height);
                return new Rect(topX, topY, selectionWidth, selectionHeight);
            }
        }

        #region Constructors

        public MultiTouchPlotter(int resolutionX, int resolutionY)
        {
            // When loaded we can get width and height and initialize things
            this.Loaded += OnLoaded;
            this.SizeChanged += OnSizeChanged;

            this.deviceResolution.Width = resolutionX;
            this.deviceResolution.Height = resolutionY;

            this.MouseMove += OnMouseMove;
            this.MouseLeftButtonDown += OnMouseLeftDown;
            this.MouseLeftButtonUp += OnMouseLeftUp;
        }

        public MultiTouchPlotter()
            : this(1920, 1080)
        {

        }

        #endregion

        #region Mouse Events

        protected new void OnMouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            this.isMouseDown = false;
        }

        protected new void OnMouseMove(object sender, MouseEventArgs e)
        {
            double x = e.GetPosition(this.Parent as Panel).X;
            double y = e.GetPosition(this.Parent as Panel).Y;
            this.mousePosition.X = x;
            this.mousePosition.Y = y;
        }

        #endregion

        #region Overrides and Basic Rendering

        protected override void ReinitializeImage()
        {
            if(double.IsNaN(this.Height))
            {
                // whenever the resolution changes, we keep the width the same but we have to adapt the height
                double widthToHeightRatio = (this.deviceResolution.Height / this.deviceResolution.Width);
                this.Height = (int)(this.Width * widthToHeightRatio);
            }
            base.ReinitializeImage();
            
        }

        protected void ResizePlotter()
        {
            // whenever the resolution changes, we keep the width the same but we have to adapt the height
            double widthToHeightRatio = (this.deviceResolution.Height / this.deviceResolution.Width);

            this.Height = (int)(this.Width * widthToHeightRatio);
                        
            ReinitializeImage();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

        }

        protected override void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ReinitializeImage();
        }

        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            ReinitializeImage();
        }

        #endregion

        public void AddTouchPoint(int id, Point position)
        {
            this.touchPoints.Add(id, position);

        }

        public void UpdatePoint(int id, Point position)
        {
            if (this.touchPoints.ContainsKey(id))
            {
                this.touchPoints[id] = position;
            }
        }

        public void RemovePoint(int id, Point position)
        {
            if(this.touchPoints.ContainsKey(id))
            {
                this.touchPoints.Remove(id);
            }
        }

        public override void DrawPoints()
        {
            using (var context = this.visual.RenderOpen())
            {
                // redraw background
                context.DrawRectangle(AstralColors.LightGray, null, new Rect(0, 0, this.Width, this.Height));

                // draw bounding box
                context.DrawRectangle(AstralColors.Yellow, null, this.selection);

                // draw all touch points
                foreach (int i in this.touchPoints.Keys)
                {
                    Point p = touchPoints[i];
                    double xPos = Utils.Map(p.X, 0, this.deviceResolution.Width, 0, this.Width);
                    double yPos = Utils.Map(p.Y, 0, this.deviceResolution.Height, 0, this.Height);
                    double radius = Utils.Map(50, 0, this.deviceResolution.Width, 0, this.Width);
                    context.DrawEllipse(AstralColors.Red, null, new Point(xPos, yPos), radius, radius);
                }

                context.Close();
            }
            this.content.Render(visual);
        }

        public void Plot(int x, int y)
        {
            double xPos = Utils.Map(x, 0, this.deviceResolution.Width, 0, this.Width);
            double yPos = Utils.Map(y, 0, this.deviceResolution.Height, 0, this.Height);

            double radius = Utils.Map(50, 0, this.deviceResolution.Width, 0, this.Width);
            using (var context = this.visual.RenderOpen())
            {
                // redraw background
                context.DrawRectangle(AstralColors.LightGray, null, new System.Windows.Rect(0, 0, this.Width, this.Height));

                if(this.isMouseDown)
                {
                    context.DrawRectangle(AstralColors.Yellow, null, new Rect(this.boundingBoxStart, this.mousePosition));
                }

                context.DrawEllipse(AstralColors.Red, null, new Point(xPos, yPos), radius, radius);
                context.DrawEllipse(AstralColors.Orange, null, this.mousePosition, radius/4, radius/4);
                context.Close();
            }
            this.content.Render(visual);
        }



    }
}
