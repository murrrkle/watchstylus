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
using Astral;

namespace AstralBlankSample.Utilities
{
    public enum BrushShapes
    {
        ELLIPSE = 0,
        SQUARE = 1,
        STAMP = 2,
    }

    public enum BrushTypes
    {
        BRUSH = 0,
        ERASER = 1,
        STAMP = 2,
        AIRBRUSH = 3
    }

    class Brush
    {
        public Color Color
        {
            get; set;
        }

        public int Radius
        {
            get; set;
        }

        public BrushTypes BrushType
        {
            get; set;
        }

        public BrushShapes BrushShape
        {
            get; set;
        }

        public int MicAttribute // 0 = For Hue, 1 = For Saturation, 2 = For Value, 3 = for Radius
        {
            get; set;
        }

        public double Hue { get; set; }
        public double Val { get; set; }
        public double Sat { get; set; }

        public Brush()
        {
            Hue = 0;
            Val = 0.8;
            Sat = 1;
            Radius = 20;
            SetColor(Hue, Val, Sat, Radius);
            BrushType = BrushTypes.BRUSH;
            BrushShape = BrushShapes.ELLIPSE;
            MicAttribute = 4;
        }

        public Brush(Color color, int radius, BrushTypes bt, BrushShapes bs, int ma)
        {
            Color = color;
            Radius = radius;
            BrushType = bt;
            BrushShape = bs;
            MicAttribute = ma;
        }

        public void SetColor(double hue, double val, double sat, int size)
        {
            Hue = hue;
            Val = val;
            Sat = sat;
            Radius = size;


            int r, g, b = 0;
            HlsToRgb(hue, val, sat, out r, out g, out b);
            Color = System.Windows.Media.Color.FromRgb((byte)r, (byte)g, (byte)b);
        }

        public static void HlsToRgb(double h, double l, double s, out int r, out int g, out int b)
        {
            double p2;
            if (l <= 0.5) p2 = l * (1 + s);
            else p2 = l + s - l * s;

            double p1 = 2 * l - p2;
            double double_r, double_g, double_b;
            if (s == 0)
            {
                double_r = l;
                double_g = l;
                double_b = l;
            }
            else
            {
                double_r = QqhToRgb(p1, p2, h + 120);
                double_g = QqhToRgb(p1, p2, h);
                double_b = QqhToRgb(p1, p2, h - 120);
            }

            // Convert RGB to the 0 to 255 range.
            r = (int)(double_r * 255.0);
            g = (int)(double_g * 255.0);
            b = (int)(double_b * 255.0);
        }

        private static double QqhToRgb(double q1, double q2, double hue)
        {
            if (hue > 360) hue -= 360;
            else if (hue < 0) hue += 360;

            if (hue < 60) return q1 + (q2 - q1) * hue / 60;
            if (hue < 180) return q2;
            if (hue < 240) return q1 + (q2 - q1) * (240 - hue) / 60;
            return q1;
        }
    }
}
