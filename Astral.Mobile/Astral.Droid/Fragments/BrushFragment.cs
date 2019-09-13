
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Astral.Droid.UI;
using System;

namespace Astral.Droid.Fragments
{
    public enum MicrophoneMode
    {
        NONE = 0,
        HUE = 1,
        SAT = 2, 
        VALUE = 3,
        SIZE = 4
    }
    [Register("Astral.Droid.Fragments.BrushFragment")]
    public class BrushFragment : Fragment
    {
        public delegate void SliderUpdatedHandler(object sender, int hue, float sat, float val, int size);
        public delegate void MicAttrChangedHandler(object sender, MicrophoneMode m);

        public event SliderUpdatedHandler SliderUpdated;
        public event MicAttrChangedHandler MicAttrChanged;


        private View fragView;

        private int hue;
        private float sat;
        private float val;
        private int size;

        private bool micPaused;

        private Button micModeToggle;
        private Button micPauseToggle;

        private SeekBar hueSeekBar;
        private SeekBar satSeekBar;
        private SeekBar valSeekBar;
        private SeekBar sizeSeekBar;

        #region Getters
        public float GetHue()
        {
            return hue;
        }

        public float GetSat()
        {
            return sat;
        }

        public float GetVal()
        {
            return val;
        }

        public float GetSize()
        {
            return size;
        }

        public bool MicPaused()
        {
            return micPaused;
        }

        #endregion
        public MicrophoneMode MicMode { get; set; }

        public BrushFragment()
        {
            MicMode = MicrophoneMode.NONE;
            micPaused = false;
        }

        public void SetColour(int h, float s, float v, int size)
        {
            hue = h;
            sat = s;
            val = v;
            this.size = size;
            //og.Debug("colour params", String.Concat(hue, " ", sat, " ", val, " ", this.size));

            BrushCanvasView bcv = fragView.FindViewById<BrushCanvasView>(Resource.Id.BrushPreview);
            bcv.UpdateColour(Color.HSVToColor(new float[] { hue, sat, val }), size);
        }

        public void UpdateSliders()
        {
            hueSeekBar.Progress = (int) hue;
            satSeekBar.Progress = (int) (sat * 100);
            valSeekBar.Progress = (int) (val * 100);
            sizeSeekBar.Progress = (int) size;

        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            return inflater.Inflate(Resource.Layout.BrushUI, container, false);

            //return base.OnCreateView(inflater, container, savedInstanceState);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            fragView = view;

            hueSeekBar = view.FindViewById<SeekBar>(Resource.Id.HueSeekbar);
            satSeekBar = view.FindViewById<SeekBar>(Resource.Id.SatSeekbar);
            valSeekBar = view.FindViewById<SeekBar>(Resource.Id.ValSeekbar);
            sizeSeekBar = view.FindViewById<SeekBar>(Resource.Id.SizeSeekbar);

            micModeToggle = view.FindViewById<Button>(Resource.Id.MicChangeButton);
            micPauseToggle = view.FindViewById<Button>(Resource.Id.MicPauseButton);

            hueSeekBar.ProgressChanged += HueSeekBar_ProgressChanged;
            satSeekBar.ProgressChanged += SatSeekBar_ProgressChanged;
            valSeekBar.ProgressChanged += ValSeekBar_ProgressChanged;
            sizeSeekBar.ProgressChanged += SizeSeekBar_ProgressChanged;

            micModeToggle.Click += MicModeToggle_Click;
            micPauseToggle.Click += MicPauseToggle_Click;
        }



        #region events
        private void MicPauseToggle_Click(object sender, EventArgs e)
        {
            if (micPaused) // Unpause mic input
            {
                micPaused = false;
                micPauseToggle.Text = "Pause";
            }
            else // pause mic input
            {
                micPaused = true;
                micPauseToggle.Text = "Resume";
            }
        }

        private void MicModeToggle_Click(object sender, EventArgs e)
        {
            hueSeekBar.Enabled = true;
            satSeekBar.Enabled = true;
            valSeekBar.Enabled = true;
            sizeSeekBar.Enabled = true;

            switch (MicMode)
            {
                case MicrophoneMode.NONE: // if None, change to Hue
                    MicMode = MicrophoneMode.HUE;
                    micModeToggle.Text = GetString(Resource.String.hue);
                    hueSeekBar.Enabled = false;
                    break;
                case MicrophoneMode.HUE: // if Hue, change to Sat
                    MicMode = MicrophoneMode.SAT;
                    micModeToggle.Text = GetString(Resource.String.sat);
                    satSeekBar.Enabled = false;
                    break;
                case MicrophoneMode.SAT: // if Sat, change to Val
                    MicMode = MicrophoneMode.VALUE;
                    micModeToggle.Text = GetString(Resource.String.val);
                    valSeekBar.Enabled = false;
                    break;
                case MicrophoneMode.VALUE: // if Val, change to Size
                    MicMode = MicrophoneMode.SIZE;
                    micModeToggle.Text = GetString(Resource.String.size);
                    sizeSeekBar.Enabled = false;
                    break;
                case MicrophoneMode.SIZE: // if Size, change to None
                    MicMode = MicrophoneMode.NONE;
                    micModeToggle.Text = GetString(Resource.String.none);
                    break;
            }
            MicAttrChanged?.Invoke(this, MicMode);
        }

        private void SizeSeekBar_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            size = e.SeekBar.Progress;
            SetColour(hue, sat, val, size);
            SliderUpdated?.Invoke(this, hue, sat, val, size);
         }

        private void ValSeekBar_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            val = e.SeekBar.Progress / 100.0f;
            SetColour(hue, sat, val, size);
            SliderUpdated?.Invoke(this, hue, sat, val, size);
        }

        private void SatSeekBar_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            sat = e.SeekBar.Progress / 100.0f;
            SetColour(hue, sat, val, size);
            SliderUpdated?.Invoke(this, hue, sat, val, size);
        }

        private void HueSeekBar_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            hue = e.SeekBar.Progress;
            SetColour(hue, sat, val, size);
            SliderUpdated?.Invoke(this, hue, sat, val, size);
        }
        #endregion

        public override void OnStart()
        {
            base.OnStart();
            SetColour(0, 1, 0.8f, 10);
            satSeekBar.Progress = 100;
            valSeekBar.Progress = 80;
            sizeSeekBar.Progress = 10;
        }

        public void UpdateMicMode(MicrophoneMode mode)
        {
            MicMode = mode;

            hueSeekBar.Enabled = true;
            satSeekBar.Enabled = true;
            valSeekBar.Enabled = true;
            sizeSeekBar.Enabled = true;

            switch (MicMode)
            {
                case MicrophoneMode.NONE: // if None, change to Hue
                    MicMode = MicrophoneMode.HUE;
                    micModeToggle.Text = GetString(Resource.String.hue);
                    hueSeekBar.Enabled = false;
                    break;
                case MicrophoneMode.HUE: // if Hue, change to Sat
                    MicMode = MicrophoneMode.SAT;
                    micModeToggle.Text = GetString(Resource.String.sat);
                    satSeekBar.Enabled = false;
                    break;
                case MicrophoneMode.SAT: // if Sat, change to Val
                    MicMode = MicrophoneMode.VALUE;
                    micModeToggle.Text = GetString(Resource.String.val);
                    valSeekBar.Enabled = false;
                    break;
                case MicrophoneMode.VALUE: // if Val, change to Size
                    MicMode = MicrophoneMode.SIZE;
                    micModeToggle.Text = GetString(Resource.String.size);
                    sizeSeekBar.Enabled = false;
                    break;
                case MicrophoneMode.SIZE: // if Size, change to None
                    MicMode = MicrophoneMode.NONE;
                    micModeToggle.Text = GetString(Resource.String.none);
                    break;
            }
        }
    }
}