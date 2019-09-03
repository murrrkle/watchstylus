using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;

namespace Astral.Droid.UI
{
    [Register("Astral.Droid.UI.BrushCanvasView")]

    public class BrushCanvasView : View
    {
        private float size;
        private Paint Colour;
        public BrushCanvasView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public BrushCanvasView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        private void Initialize()
        {
            Colour = new Paint();
        }

        public void UpdateColour(Color c, float s)
        {
            Colour.Color = c;
            size = s;
            PostInvalidate();
        }
        
        protected override void OnDraw(Canvas c)
        {
            //c.DrawCircle((float)Resources.DisplayMetrics.WidthPixels / 3, (float)Resources.DisplayMetrics.HeightPixels / 4, size, Colour);
            c.DrawCircle(this.Width/2, this.Height/2, size, Colour);
        }
    }
}