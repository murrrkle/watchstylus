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
    public class MultiTouchPathPlotter : MultiTouchPlotter
    {
        protected List<Point> path;
        protected bool isRecording = true;

        public MultiTouchPathPlotter(int resolutionX, int resolutionY)
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

        public MultiTouchPathPlotter()
            : this(1920, 1080)
        {

        }

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

            if(this.isMouseDown && isRecording)
            {
                this.path.Add(new Point(x, y));
            }
        }

        #endregion

        private int FindClosestIndex(Point touchPoint)
        {
            Point nearest = new Point(99999, 99999);
            double nearestDist = 10000000000;
            foreach(Point p in this.path)
            {
                double distance = Utils.DistanceBetweenTwoPoints(touchPoint, p);
                if(distance < nearestDist)
                {
                    nearest = p;
                    nearestDist = distance;
                }
            }
            return path.IndexOf(nearest);
        }

    }
}
