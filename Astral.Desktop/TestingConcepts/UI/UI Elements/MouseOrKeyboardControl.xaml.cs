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

namespace TestingConcepts
{
    public enum MouseOrKeyboard
    {
        Mouse,
        Keyboard
    }

    public class MouseOrKeyboardEventArgs : EventArgs
    {
        public MouseOrKeyboard MouseOrKey { get; set; }

        public MouseOrKeyboardEventArgs(MouseOrKeyboard mouseOrKeyboard)
        {
            this.MouseOrKey = mouseOrKeyboard;
        }
    }
    /// <summary>
    /// Interaction logic for MouseOrKeyboardControl.xaml
    /// </summary>
    public partial class MouseOrKeyboardControl : UserControl
    {
        public event EventHandler<MouseOrKeyboardEventArgs> Clicked;

        private SolidColorBrush blue;

        private void RaiseClick(MouseOrKeyboardEventArgs e)
        {
            if(Clicked != null)
            {
                Clicked(this, e);
            }
        }

        public MouseOrKeyboardControl()
        {
            InitializeComponent();
            this.MouseRect.Opacity = 0;
            this.KeyboardRect.Opacity = 0;
            this.MouseCanvas.MouseLeftButtonDown += OnMouseCanvasSelected;
            this.KeyboardCanvas.MouseLeftButtonDown += OnKeyboardCanvasSelected;

            this.MouseCanvas.MouseEnter += OnRectMouseEnter;
            this.KeyboardCanvas.MouseEnter += OnRectMouseEnter;
            this.MouseCanvas.MouseLeave += OnRectMouseLeave;
            this.KeyboardCanvas.MouseLeave += OnRectMouseLeave;

            this.blue = this.MouseRect.Fill as SolidColorBrush;
            this.blue.Freeze();
        }

        private void OnRectMouseLeave(object sender, MouseEventArgs e)
        {
            this.KeyboardRect.Fill = this.blue;
            this.MouseRect.Fill = this.blue;
        }

        private void OnRectMouseEnter(object sender, MouseEventArgs e)
        {
            if(sender == KeyboardCanvas)
            {
                this.KeyboardRect.Fill = AstralColors.Yellow;
            }
            else
            {
                this.MouseRect.Fill = AstralColors.Yellow;
            }
        }

        private void OnKeyboardCanvasSelected(object sender, MouseButtonEventArgs e)
        {
            this.MouseRect.Opacity = 0;
            this.KeyboardRect.Opacity = 1;
            RaiseClick(new MouseOrKeyboardEventArgs(MouseOrKeyboard.Keyboard));
        }

        private void OnMouseCanvasSelected(object sender, MouseButtonEventArgs e)
        {
            this.MouseRect.Opacity = 1;
            this.KeyboardRect.Opacity = 0;
            RaiseClick(new MouseOrKeyboardEventArgs(MouseOrKeyboard.Mouse));
        }
    }
}
