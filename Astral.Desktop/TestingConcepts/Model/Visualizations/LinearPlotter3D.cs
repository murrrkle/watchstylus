using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Windows.Media;

namespace TestingConcepts
{
    internal class PlotterPoint
    {
        #region Instance Variables
        private double[] values = new double[3];
        double x;
        double y;
        double z;
        #endregion

        #region Properties

        public double X
        {
            get
            {
                return this.x;
            }
            set
            {
                values[0] = value;
                this.x = value;
            }
        }

        public double Y
        {
            get
            {
                return y;
            }
            set
            {
                values[1] = value;
                this.y = value;
            }
        }

        public double Z
        {
            get
            {
                return z;
            }
            set
            {
                values[2] = value;
                this.z = value;
            }
        }

        public double AbsoluteMax
        {
            get
            {
                return Math.Max(Math.Abs(x), Math.Max(Math.Abs(y), Math.Abs(z)));
            }
        }

        public double[] Values
        {
            get
            {
                return this.values;
            }
        }

        #endregion

        #region Constructor

        public PlotterPoint(double x, double y, double z)
        {
            this.values = new double[] { x, y, z };
            this.x = x;
            this.y = y;
            this.z = z;
        }

        #endregion

        #region Methods

        public void Remap(double maxRange, double height)
        {
            this.x = Utils.Map(this.x, -maxRange, maxRange, height, 0);
            this.y = Utils.Map(this.y, -maxRange, maxRange, height, 0);
            this.z = Utils.Map(this.z, -maxRange, maxRange, height, 0);
            this.values = new double[] { x, y, z };
        }

        #endregion
    }

    public class LinearPlotter3D : LinearPlotterBase
    {        
        private Pen penX = new Pen(AstralColors.Red, 2);
        private Pen penY = new Pen(AstralColors.Orange, 2);
        private Pen penZ = new Pen(AstralColors.Teal, 2);

        private List<double> pointsX = new List<double>();
        private List<double> pointsY = new List<double>();
        private List<double> pointsZ = new List<double>();

        private void AddPoint(double point, List<double> pointList)
        {
            pointList.Add(point);
            if(pointList.Count > this.numberOfValues)
            {
                pointList.RemoveAt(0);
            }
        }

        public void PushPoint(double x, double y, double z)
        {
            PlotterPoint point = new PlotterPoint(x, y, z);

            if (point.AbsoluteMax > this.maxRange)
            {
                this.maxRange = point.AbsoluteMax * 1.2;
            }

            point.Remap(this.maxRange, this.Height);

            AddPoint(point.X, this.pointsX);
            AddPoint(point.Y, this.pointsY);
            AddPoint(point.Z, this.pointsZ);

            DrawPoints();
        }

        public override void DrawPoints()
        {
            using (var context = this.visual.RenderOpen())
            {
                // redraw background
                context.DrawRectangle(AstralColors.LightGray, null, new Rect(0, 0, this.Width, this.Height));

                DrawPointsForList(context, pointsX.ToArray(), penX);
                DrawPointsForList(context, pointsY.ToArray(), penY);
                DrawPointsForList(context, pointsZ.ToArray(), penZ);

                context.Close();
            }
            this.content.Render(visual);
        }
    }
}
