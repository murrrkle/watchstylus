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
using Astral.Droid.Models;

namespace Astral.Droid.UI
{
    [Register("Astral.Droid.UI.BrushImageView")]
    public class BrushImageView : View
    {
        private Paint paint;
        private LinearLayout layout;
        private float size;

        public SeekBar hSlider;
        public SeekBar sSlider;
        public SeekBar vSlider;


        float hue;
        float sat;
        float val;

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
            size = 30;

            layout = new LinearLayout(this.Context)
            {
                Orientation = Orientation.Vertical
            };

            hSlider = new SeekBar(this.Context);
            sSlider = new SeekBar(this.Context);
            vSlider = new SeekBar(this.Context);

            layout.AddView(hSlider);
            layout.AddView(sSlider);
            layout.AddView(vSlider);
        }

        public void SetBrush(float h, float s, float v, float size)
        {
            hue = h;
            sat = s;
            val = v;

            paint.Color = Color.HSVToColor(new float[] { hue, sat, val });
            this.size = size;
        }

        protected override void OnDraw(Canvas canvas)
        {
            canvas.DrawCircle((float) Resources.DisplayMetrics.WidthPixels/2, (float) Resources.DisplayMetrics.HeightPixels/2, size, paint);
        }
    }
}