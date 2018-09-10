using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Astral.Droid.UI
{
    [Register("Astral.Droid.UI.BrushImageView")]
    public class BrushImageView : View
    {
        private Paint paint;
        private float size;

        public BrushImageView(Context context, IAttributeSet attrs = null) :
            base(context, attrs)
        {
            Initialize();
            
        }

        public BrushImageView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }
        private void Initialize()
        {
            paint = new Paint() { Color = Color.Black };
            size = 20;
        }

        public void SetBrush(Color c, float size)
        {
            paint.Color = c;
            this.size = size;
        }

        protected override void OnDraw(Canvas canvas)
        {
            canvas.DrawCircle((float) Resources.DisplayMetrics.WidthPixels/2, (float) Resources.DisplayMetrics.HeightPixels/2, size, paint);
        }
    }
}