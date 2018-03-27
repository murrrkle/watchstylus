using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Hardware;
using Android.Media;
using Astral.Device;
using Encoding = Android.Media.Encoding;

namespace Astral.Droid.Media
{
    public class AndroidMicrophone2 : Microphone
    {
        #region Instance Variables

        private int bufferSize;
        private AudioRecord recorder;
        private short[] audioBuffer;

        private int samplingRate = (int)SamplingRate.Maximum;
        private ChannelIn channel = ChannelIn.Mono;
        private Android.Media.Encoding encoding = Android.Media.Encoding.Pcm16bit;
        private AudioSource source = AudioSource.Default;

        private Stopwatch stopwatch = new Stopwatch();

        private BackgroundWorker recordAudio = new BackgroundWorker();

        // what people want from the microphone
        private short[] samples;
        private double amplitude;
        private double samplingDuration;

        // double that holds the amplitude in the background thread
        double amp;

        #endregion

        #region Properties
        public IEnumerable<int> SupportedSampleRates { get; private set; }

        public bool IsMicrophoneActive
        {
            get
            {
                return (this.recorder != null
                    && this.recorder.RecordingState == RecordState.Recording);
            }
        }

        #endregion

        private void OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // clone the contents of the audio buffer
            this.samples = audioBuffer.ToArray<short>();
            this.samplingDuration = stopwatch.ElapsedMilliseconds;
            this.amplitude = amp;

            recordAudio.RunWorkerAsync();
        }

        private void OnDoWork(object sender, DoWorkEventArgs e)
        {
            Record();
        }

        private void GetRightSettings()
        {

            foreach(int rate in SamplingRate.GetValues(typeof(SamplingRate)))
            {
                foreach(Encoding encoding in new Encoding[] { Encoding.Pcm16bit, Encoding.Pcm8bit, Encoding.Ac3, Encoding.Default,
                    Encoding.Dts, Encoding.DtsHd, Encoding.EAc3, Encoding.Invalid, Encoding.PcmFloat })
                {
                    foreach (ChannelIn channel in new ChannelIn[]{ ChannelIn.Mono, ChannelIn.Stereo})
                    {
                        try
                        {
                            this.bufferSize = AudioRecord.GetMinBufferSize(samplingRate, channel, encoding);
                            this.recorder = new AudioRecord(source, samplingRate, channel, encoding, bufferSize);
                            this.audioBuffer = new short[this.bufferSize / 2];

                            // see if we can get away without reinitializing first
                            this.recorder = new AudioRecord(source, rate, channel, encoding, bufferSize);
                            if (recorder.State == State.Initialized)
                            {
                                Console.WriteLine(rate + " :: " + encoding + " :: " + channel + " -> TRULY ACCEPTED");
                                return;
                            }
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine(rate + " :: " + encoding + " :: " + channel + " -> not accepted");
                            Console.WriteLine(e.Message);
                        }
                    }
                }
            }
        }

        public void Start()
        {
            //this.bufferSize = AudioRecord.GetMinBufferSize(samplingRate, channel, encoding);
            //this.recorder = new AudioRecord(source, samplingRate, channel, encoding, bufferSize);
            //this.audioBuffer = new short[this.bufferSize / 2];

            //// see if we can get away without reinitializing first
            //this.recorder = new AudioRecord(source, samplingRate, channel, encoding, bufferSize);
            GetRightSettings();

            //if (recorder.State != State.Initialized)
            //{
            //    throw new Exception("ERROR RECORDING CANNOT INITIALIZE");
            //    // return;
            //}

            recorder.StartRecording();

            recordAudio.DoWork += OnDoWork;
            recordAudio.RunWorkerCompleted += OnRunWorkerCompleted;
            recordAudio.RunWorkerAsync();
        }

        public void Stop()
        {
            recordAudio.DoWork -= OnDoWork;
            recordAudio.RunWorkerCompleted -= OnRunWorkerCompleted;
            recordAudio.CancelAsync();

            this.recorder.Stop();
            this.recorder.Release();
        }

        private void Record()
        {

            stopwatch.Reset();
            stopwatch.Start();

            long shortsRead = 0;
            int numberOfShort = recorder.Read(audioBuffer, 0, audioBuffer.Length);
            shortsRead += numberOfShort;


            stopwatch.Stop();

            amp = Math.Abs(Array.ConvertAll(audioBuffer, s => (int)s).Sum() / audioBuffer.Length);


        }

        private void OnMicrophoneActivated(object sender, EventArgs e)
        {
            Start();
        }

        private void OnMicrophoneDeactivated(object sender, EventArgs e)
        {
            Stop();
        }

    }
}