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
using Astral.Device;
using TouchPoint = Astral.Device.TouchPoint;

namespace TestingConcepts
{
    public static class Utils
    {

        public static bool IsInsideRect(this TouchPoint p, Rect rect)
        {
            return (p.X >= rect.TopLeft.X && p.X <= rect.TopRight.X && p.Y >= rect.TopLeft.Y && p.Y <= rect.BottomLeft.Y);
        }

        public static bool IsInsideRect(this Point p, Rect rect)
        {
            return (p.X >= rect.TopLeft.X && p.X <= rect.TopRight.X && p.Y >= rect.TopLeft.Y && p.Y <= rect.BottomLeft.Y);
        }

        public static double DistanceBetweenTwoPoints(Point p1, Point p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }

        public static double Magnitude(double x, double y, double z)
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }

        public static double Map(double value, double min, double max, double newMin, double newMax)
        {
            if(value == min)
            {
                return newMin;
            }
            return (((newMax - newMin) / (max - min)) * (value - min)) + newMin;
        }


        public static double CubicMap(double value, double min, double max, double newMin, double newMax)
        {
            double y;

            // adjust minimum and maximum so they are from 0 to 1
            double reducedX = (value - min) / (max - min);

            // apply function
            y = 0.5 * (Math.Pow((2 * reducedX - 1), 3) + 1);

            // adjust minimum and maximum to new values
            y = (newMax - newMin) * y + newMin;

            return y;
        }

        public static double RadiansToDegrees(double radians)
        {
            return (radians * 180 / Math.PI);
        }

        public static double DegreesToRadians(double degrees)
        {
            return (degrees * Math.PI / 180);
        }

        public static double Average(params double[] values)
        {
            return (double)(values.Sum() / values.Length);

        }

        private static byte Lerp(byte a, byte b, double step)
        {
            return (byte)(a + (b - a) * step);
        }

        public static Color InterpolateColors(double step, Color color1, Color color2)
        {
            return Color.FromArgb(Lerp(color1.A, color2.A, step),
                                    Lerp(color1.R, color2.R, step),
                                    Lerp(color1.G, color2.G, step),
                                    Lerp(color1.B, color2.B, step));
        }

        public static Color InterpolateColors(double step, params Color[] colors)
        {
            double offset = (double)1 / (colors.Length - 1);

            if (step == 0)
            {
                return colors[0];
            }

            if (step == 1)
            {
                return colors[colors.Length - 1];
            }
            for (int i = 0; i < colors.Length - 1; i++)
            {
                double substep = i * offset;
                double substepMax = substep + offset;
                if (step > substep && step <= substepMax)
                {
                    double newStep = Utils.Map(step, substep, substepMax, 0, 1);

                    return InterpolateColors(step, colors[i], colors[i + 1]);
                }
            }

            return Colors.Transparent;

        }

        // Arc drawing methods by Harvey Green:
        // https://social.msdn.microsoft.com/Forums/vstudio/en-US/ea83436b-e6d8-4014-8c9c-1e5e241c6f53/drawarc-an-extension-method-to-drawingcontext?forum=wpf

        /// <summary>
        /// Draw an Arc of an ellipse or circle. Static extension method of DrawingContext.
        /// </summary>
        /// <param name="dc">DrawingContext</param>
        /// <param name="pen">Pen for outline. set to null for no outline.</param>
        /// <param name="brush">Brush for fill. set to null for no fill.</param>
        /// <param name="rect">Box to hold the whole ellipse described by the arc</param>
        /// <param name="startDegrees">Start angle of the arc degrees within the ellipse. 0 degrees is a line to the right.</param>
        /// <param name="sweepDegrees">Sweep angle, -ve = Counterclockwise, +ve = Clockwise</param>
        public static void DrawArc(this DrawingContext dc, Pen pen, Brush brush, Rect rect, double startDegrees, double sweepDegrees)
        {
            GeometryDrawing arc = CreateArcDrawing(rect, startDegrees, sweepDegrees);
            dc.DrawGeometry(brush, pen, arc.Geometry);
        }

        /// <summary>
        /// Create an Arc geometry drawing of an ellipse or circle
        /// </summary>
        /// <param name="rect">Box to hold the whole ellipse described by the arc</param>
        /// <param name="startDegrees">Start angle of the arc degrees within the ellipse. 0 degrees is a line to the right.</param>
        /// <param name="sweepDegrees">Sweep angle, -ve = Counterclockwise, +ve = Clockwise</param>
        /// <returns>GeometryDrawing object</returns>
        private static GeometryDrawing CreateArcDrawing(Rect rect, double startDegrees, double sweepDegrees)
        {
            // degrees to radians conversion
            double startRadians = startDegrees * Math.PI / 180.0;
            double sweepRadians = sweepDegrees * Math.PI / 180.0;

            // x and y radius
            double dx = rect.Width / 2;
            double dy = rect.Height / 2;

            // determine the start point 
            double xs = rect.X + dx + (Math.Cos(startRadians) * dx);
            double ys = rect.Y + dy + (Math.Sin(startRadians) * dy);

            // determine the end point 
            double xe = rect.X + dx + (Math.Cos(startRadians + sweepRadians) * dx);
            double ye = rect.Y + dy + (Math.Sin(startRadians + sweepRadians) * dy);

            // draw the arc into a stream geometry
            StreamGeometry streamGeom = new StreamGeometry();
            using (StreamGeometryContext ctx = streamGeom.Open())
            {
                bool isLargeArc = Math.Abs(sweepDegrees) > 180;
                SweepDirection sweepDirection = sweepDegrees < 0 ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;

                ctx.BeginFigure(new Point(xs, ys), false, false);
                ctx.ArcTo(new Point(xe, ye), new Size(dx, dy), 0, isLargeArc, sweepDirection, true, false);
            }

            // create the drawing
            GeometryDrawing drawing = new GeometryDrawing();
            drawing.Geometry = streamGeom;
            return drawing;
        }
    }
}
