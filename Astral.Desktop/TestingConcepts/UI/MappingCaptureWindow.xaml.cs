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
using System.Windows.Shapes;

namespace TestingConcepts
{
    /// <summary>
    /// Interaction logic for MappingCaptureWindow.xaml
    /// </summary>
    public partial class MappingCaptureWindow : Window
    {
        // hardcoded for now so I can work with it as if it was the result
        private Rect m_selection = new Rect(new Point(1141, 1545), new Point(3325, 1545)); //new Rect(new Point(1980, 1331), new Point(3306, 1331));

        public Rect Selection
        {
            get
            {
                return this.m_selection;
            }
        }

        public MappingCaptureWindow()
        {
            InitializeComponent();
        }
    }
}
