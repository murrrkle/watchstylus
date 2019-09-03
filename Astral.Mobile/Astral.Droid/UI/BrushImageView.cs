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
    public class BrushImageView : LinearLayout
    {
        private Paint paint;
        private LinearLayout layout;

        public SeekBar hSlider;
        public SeekBar sSlider;
        public SeekBar vSlider;
        public SeekBar zSlider;

        public Button toggle;
        public int MicAttribute; // 0 = For Hue, 1 = For Saturation, 2 = For Value, 3 = for Radius, 4 = OFF

        public float size { get; set; }
        public float hue {get;set;}
        public float sat { get; set; }
        public float val { get; set; }

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
            MicAttribute = 4;
            paint = new Paint();
            hue = 0;
            sat = 1;
            val = 0.8f;
            size = 10;
            SetBrush(hue, sat, val, size);

            this.SetWillNotDraw(false);

            layout = new LinearLayout(this.Context)
            {
                Orientation = Orientation.Vertical,
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
                
            };

            LinearLayout.LayoutParams ll = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            ll.SetMargins(240, 15, 20, 15);

            //= Resources.DisplayMetrics.HeightPixels / 4;
            //layout.LayoutParameters = ll;

            LinearLayout buttonLayout = new LinearLayout(this.Context)
            {
                Orientation = Orientation.Horizontal,
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };

            buttonLayout.LayoutParameters = ll;


            toggle = new Button(this.Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent),
                Text = "N",
            };
            buttonLayout.AddView(toggle);

             hSlider = new SeekBar(this.Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Max = 360,
                
            };
            sSlider = new SeekBar(this.Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Max = 100
            };
            vSlider = new SeekBar(this.Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Max = 100
            };
            zSlider = new SeekBar(this.Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Max = 50
            };
            LinearLayout hSliderLayout = new LinearLayout(this.Context)
            {
                Orientation = Orientation.Horizontal,
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            hSliderLayout.AddView(new TextView(this.Context)
            {
                Text = "H",
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)

            });
            hSliderLayout.AddView(hSlider);

            LinearLayout sSliderLayout = new LinearLayout(this.Context)
            {
                Orientation = Orientation.Horizontal,
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            sSliderLayout.AddView(new TextView(this.Context)
            {
                Text = "S",
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)

            });
            sSliderLayout.AddView(sSlider);

            LinearLayout vSliderLayout = new LinearLayout(this.Context)
            {
                Orientation = Orientation.Horizontal,
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            vSliderLayout.AddView(new TextView(this.Context)
            {
                Text = "V",
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)

            });
            vSliderLayout.AddView(vSlider);

            LinearLayout zSliderLayout = new LinearLayout(this.Context)
            {
                Orientation = Orientation.Horizontal,
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            zSliderLayout.AddView(new TextView(this.Context)
            {
                Text = "R",
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)

            });
            zSliderLayout.AddView(zSlider);

            layout.AddView(buttonLayout);
            layout.AddView(hSliderLayout);
            layout.AddView(sSliderLayout);
            layout.AddView(vSliderLayout);
            layout.AddView(zSliderLayout);

            this.AddView(layout);

            sSlider.Progress = (int) sat * 100;
            vSlider.Progress = (int) val * 100;
            zSlider.Progress = (int) size;
            
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
            canvas.DrawCircle((float) Resources.DisplayMetrics.WidthPixels/3, (float) Resources.DisplayMetrics.HeightPixels/4, size + 20, paint);
        }
    }
}