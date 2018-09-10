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
    [Register("Astral.Droid.UI.AirbrushImageView")]
    public class AirbrushImageView : View
    {

        public delegate void AirbrushEventHandler(object sender, float airflow);
        public event AirbrushEventHandler AirflowChanged;

        float PaintVolume { get; set; }
        public float TriggerY { get; set; }


        public AirbrushImageView(Context context, IAttributeSet attrs = null) :
            base(context, attrs)
        {
            Initialize();
        }

        public AirbrushImageView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        private void Initialize()
        {
            PaintVolume = 0.0f;
            TriggerY = 60.0f;
        }
        

        protected override void OnDraw(Canvas canvas)
        {
            
            float cx = (float)Resources.DisplayMetrics.WidthPixels/2;
            float cy = TriggerY;
            if (TriggerY < 60)
                cy = 60.0f;
            else if (TriggerY > 260)
                cy = 260f;

            canvas.DrawCircle(cx, cy, 50.0f, new Paint()
            {
                Color = Color.Gray
            });
        }
        
        public override bool OnTouchEvent(MotionEvent e)
        {
            TriggerY = e.GetY();

            if (e.Action is MotionEventActions.Up)
                TriggerY = 60;

            AirflowChanged?.Invoke(this, TriggerY - 60.0f);
            Invalidate();
            return true;
        }
    }
}