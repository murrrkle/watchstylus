using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Hardware;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Astral.Device;

namespace Astral.Droid.Media
{
    #region Class 'AndroidMicrophone'
    public class AndroidMicrophone : Microphone
    {
        #region Class Members
        private AudioRecord m_recorder;

        private int m_bufferSize;

        private Thread m_recordThread;
        #endregion

        #region Constructors
        public AndroidMicrophone(SamplingRate samplingRate = SamplingRate.High)
            : base(samplingRate)
        {
            Activated += OnMicrophoneActivated;
            Deactivated += OnMicrophoneDeactivated;

            SupportedSampleRates = (new[] { 8000, 11025, 16000, 22050, 44100 }).Where(
                rate => AudioRecord.GetMinBufferSize(rate, ChannelIn.Mono, Encoding.Pcm16bit) > 0).ToList();
        }
        #endregion

        #region Properties
        public IEnumerable<int> SupportedSampleRates { get; private set; }

        public bool IsMicrophoneActive
        {
            get
            {
                return (m_recorder != null 
                    && m_recorder.RecordingState == RecordState.Recording);
            }
        }
        #endregion

        #region Start / Stop
        internal void Start()
        {
            // setup buffers
            m_bufferSize = AudioRecord.GetMinBufferSize(
                (int)SamplingRate, ChannelIn.Mono,
                Encoding.Pcm16bit);

            if (m_bufferSize < 0)
            {
                m_bufferSize = (int)SamplingRate * 2;
            }

            m_recorder = new AudioRecord(
                AudioSource.Default, (int)SamplingRate,
                ChannelIn.Mono, Encoding.Pcm16bit, m_bufferSize);

            Console.WriteLine(m_bufferSize);

            if (m_recorder.State != State.Initialized)
            {
                Console.WriteLine("Audio Record cannot initialize: " + m_recorder.State);
                return;
            }

            m_recordThread = new Thread(new ThreadStart(RecordAudio));
            m_recordThread.IsBackground = true;
            m_recordThread.Name = "AndroidMicrophone#RecordAudio";
            m_recordThread.Start();
        }

        internal void Stop()
        {
            if (m_recordThread != null
                && m_recordThread.IsAlive
                && !(m_recordThread.Join(100)))
            {
                m_recordThread.Abort();
            }

            m_recordThread = null;
            GC.Collect();
        }
        #endregion

        #region Recording
        private void RecordAudio()
        {
            Process.SetThreadPriority(Android.OS.ThreadPriority.Audio);
            short[] audioBuffer = new short[m_bufferSize / 2];

            m_recorder.StartRecording();
            while (IsActive)
            {
                m_recorder.Read(audioBuffer, 0, audioBuffer.Length);

                MicrophoneData micData = new MicrophoneData(audioBuffer);
                UpdateMicrophoneData(micData);
            }

            m_recorder.Stop();
            m_recorder.Release();
        }
        #endregion

        #region Event Handling
        private void OnMicrophoneActivated(object sender, EventArgs e)
        {
            Start();
        }

        private void OnMicrophoneDeactivated(object sender, EventArgs e)
        {
            Stop();
        }
        #endregion
    }
    #endregion
}