using System;
using System.Collections.Generic;
using System.Linq;
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
        private int m_bufferSize;

        private AudioRecord m_recorder;
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

        #region Start/Stop
        internal Task<bool> Start()
        {
            return Task.Run(
                () =>
                    {
                        if (!(SupportedSampleRates.Contains((int)SamplingRate)))
                        {
                            return false;
                        }

                        m_bufferSize = AudioRecord.GetMinBufferSize(
                            (int)SamplingRate, ChannelIn.Mono, Encoding.Pcm16bit);

                        m_recorder = new AudioRecord(AudioSource.Mic, (int)SamplingRate, 
                            ChannelIn.Mono, Encoding.Pcm16bit, m_bufferSize);

                        StartRecording();

                        return true;
                    });
        }

        internal Task Stop()
        {
            return Task.Run(
                () =>
                    {
                        m_recorder.Stop();
                        m_recorder = null;
                    });
        }
        #endregion

        #region Recording
        private void StartRecording()
        {
            m_recorder.StartRecording();
            Task.Run(
                async () =>
                {
                    do
                    {
                        await Record();
                    }
                    while (IsMicrophoneActive);
                });
        }

        private async Task Record()
        {
            var buffer = new short[m_bufferSize / 2];
            var readCount = await m_recorder.ReadAsync(buffer, 0, m_bufferSize / 2);

            // send it right away
            MicrophoneData micData = new MicrophoneData(buffer);
            UpdateMicrophoneData(micData);
            Console.WriteLine("RECORDED" + buffer.Length);
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