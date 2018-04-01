using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;

namespace TestingConcepts
{
    public class SelectionEventArgs : EventArgs
    {
        private Rect selection;

        public Rect Selection
        {
            get { return selection; }
        }

        public SelectionEventArgs(Rect selection)
        {
            this.selection = selection;
        }

    }

    public class PlotterBase : Image
    {
        protected RenderTargetBitmap content;
        protected DrawingVisual visual = new DrawingVisual();

        protected Rect selection = new Rect(0, 0, 0, 0);

        protected Point mousePosition = new Point(-1, -1);
        protected Point boundingBoxStart = new Point(-1, -1);
        protected bool isMouseDown = false;

        public event EventHandler<SelectionEventArgs> SelectionChanged;

        protected virtual void OnSelectionChanged(SelectionEventArgs e)
        {
            SelectionChanged?.Invoke(this, e);
        }

        public Rect Selection
        {
            get
            {
                return this.selection;
            }
            set
            {
                this.selection = value;
            }
        }

        public virtual Rect SelectionInRuleCoordinates
        {
            get
            {
                return this.selection;
            }
        }

        protected int dpi = 200;

        public int Dpi
        {
            get
            {
                return this.dpi;
            }
            set
            {
                this.dpi = value;
                ReinitializeImage();
            }
        }

        public PlotterBase()
            : base()
        {
            // When loaded we can get width and height and initialize things
            this.Loaded += OnLoaded;
            this.SizeChanged += OnSizeChanged;

            this.MouseLeftButtonDown += OnMouseLeftDown;
            this.MouseLeftButtonUp += OnMouseLeftUp;
            this.MouseMove += OnMouseMove;
        }

        protected virtual void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (this.isMouseDown)
            {
                this.mousePosition = e.GetPosition(this);
                this.selection = new Rect(this.boundingBoxStart, this.mousePosition);
                //OnSelectionChanged(new SelectionEventArgs(this.selection));
            }
        }

        protected virtual void OnMouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            this.mousePosition = e.GetPosition(this);
            this.isMouseDown = false;
            this.selection = new Rect(this.boundingBoxStart, this.mousePosition);
            OnSelectionChanged(new SelectionEventArgs(this.selection));
        }

        protected virtual void OnMouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            this.isMouseDown = true;
            this.mousePosition = e.GetPosition(this);
            this.boundingBoxStart = e.GetPosition(this);
        }

        public virtual void DrawPoints()
        {

        }

        protected virtual void ReinitializeImage()
        {
            double newWidth = this.Width * dpi / 96;
            double newHeight = this.Height * dpi / 96;

            this.content = new RenderTargetBitmap((int)newWidth, (int)newHeight, dpi, dpi, PixelFormats.Pbgra32);
            this.Stretch = Stretch.Fill;
            this.Source = content;
            
            GC.Collect();
            GC.WaitForPendingFinalizers();

            using (var context = this.visual.RenderOpen())
            {
                // draw background
                context.DrawRectangle(AstralColors.LightGray, null, new Rect(0, 0, this.Width, this.Height));
                context.Close();
            }
            this.content.Render(visual);

        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

        }

        protected virtual void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ReinitializeImage();
        }

        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            // prevent the Visual Studio designer from drawing null things
            // stop rendering on the designer view
#if DEBUG
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;
#endif

            ReinitializeImage();
        }


    }
}
