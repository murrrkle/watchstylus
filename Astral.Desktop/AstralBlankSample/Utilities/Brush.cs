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

        public Brush()
        {
            Color = Colors.Black;
            Radius = 5;
            BrushType = BrushTypes.BRUSH;
            BrushShape = BrushShapes.ELLIPSE;
        }

        public Brush(Color color, int radius, BrushTypes bt, BrushShapes bs)
        {
            Color = color;
            Radius = radius;
            BrushType = bt;
            BrushShape = bs;
        }
    }
}
