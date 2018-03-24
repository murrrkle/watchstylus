using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TestingConcepts
{
    public class AstralColors
    {
        #region Static Public Properties

        public static SolidColorBrush Red = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E64D3C"));
        public static SolidColorBrush Teal = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6FB6B9"));
        public static SolidColorBrush Blue = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C5379"));
        public static SolidColorBrush Yellow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F1C02B"));
        public static SolidColorBrush Orange = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F19C34"));
        public static SolidColorBrush White = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
        public static SolidColorBrush LightGray = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f1f2f2"));
        public static SolidColorBrush Black = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));

        #endregion

        #region Static Constructor

        static AstralColors()
        {
            // Freezing brushes to optimize performance
            Red.Freeze();
            Teal.Freeze();
            Blue.Freeze();
            Yellow.Freeze();
            Orange.Freeze();
            White.Freeze();
            LightGray.Freeze();
        }

        #endregion



    }
}
