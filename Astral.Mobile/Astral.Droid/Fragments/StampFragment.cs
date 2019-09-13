using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Astral.Droid.Fragments
{
    public class StampFragment : Fragment
    {
        public UI.ScreenshotView sv;

        [Register("Astral.Droid.Fragment.StampFragment")]
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            return inflater.Inflate(Resource.Layout.StampUI, container, false);

            //return base.OnCreateView(inflater, container, savedInstanceState);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            sv = view.FindViewById<UI.ScreenshotView>(Resource.Id.StampDisplayView);
            //sv = view.FindViewById<UI.ScreenshotView>(Resource.Id.);
        }

        public void UpdateContent(byte[] bytes)
        {
            sv.UpdateContent(bytes);
        }
    }
}