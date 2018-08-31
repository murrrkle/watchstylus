using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Astral.Droid.Sensors;
using Android.Graphics;

namespace Astral.Droid
{
    [Activity(Label = "BrushActivity")]

    public class BrushActivity : LiveSensorActivity
    {
        ImageView biv;
        Bitmap bm;
        Paint p;
        Canvas c;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.BrushUI);
            // Create your application here


        }
        protected override void OnStart()
        {
            base.OnStart();

            biv = FindViewById<ImageView>(Resource.Id.BrushImageView);

            if (biv != null)
            {
            bm = Bitmap.CreateBitmap(320, 320, Bitmap.Config.Rgb565);
            c = new Canvas(bm);

            }
        }


    }
}