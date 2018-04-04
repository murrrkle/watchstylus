using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Astral.Content;
using Astral.UI;

namespace Astral.Session.Tasks
{
    #region Class 'ScreenCaptureTask'
    internal class ScreenCaptureTask
    {
        #region Static Class Members
        internal static int KeyFrameCounter = 10000;
        #endregion

        #region Class Members
        #region Control Class Members
        private AstralSession m_session;

        private CaptureTaskState m_state;

        private double m_framesPerSecond = -1.0;

        private bool m_canSend = true;
        #endregion

        #region Thread Class Members
        private Thread m_captureThread;

        private bool m_running;

        private bool m_firstRun;

        private int m_frameCount;
        #endregion

        #region Imaging Class Members
        private Bitmap m_prevScreenshot;

        private Bitmap m_currScreenshot;

        private byte[] m_buffer;
        #endregion

        #region UI Class Members
        private CaptureSelectionWindow m_selectionWindow;
        #endregion
        #endregion

        #region Events
        internal event CaptureStateEventHandler CaptureStateChanged;

        internal event CaptureEventHandler ScreenshotCaptured;

        internal event ProcessedCaptureEventHandler ProcessedScreenshotCaptured;

        internal event SelectionWindowEventHandler SelectionWindowClosed
        {
            add
            {
                Application.Current.Dispatcher.Invoke(
                    new Action(
                        delegate ()
                        {
                            if (m_selectionWindow != null)
                            {
                                m_selectionWindow.SelectionWindowClosed += value;
                            }
                        }));
            }
            remove
            {
                Application.Current.Dispatcher.Invoke(
                    new Action(
                        delegate ()
                        {
                            if (m_selectionWindow != null)
                            {
                                m_selectionWindow.SelectionWindowClosed += value;
                            }
                        }));
            }
        }
        #endregion

        #region Constructors
        internal ScreenCaptureTask(AstralSession session)
        {
            m_session = session;
            m_running = false;
            m_state = CaptureTaskState.Idle;
            
            Rect initialRegion = InitializeRegion();

            Application.Current.Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        m_selectionWindow = new CaptureSelectionWindow(initialRegion, m_session.Device[Device.ModuleType.Display] as Device.Display);
                        m_selectionWindow.IsVisibleChanged += SelectionWindowVisibilityChanged;
                        m_selectionWindow.Loaded += SelectionWindowLoaded;
                    }));
        }
        #endregion

        #region Properties
        public bool IsRunning
        {
            get { return m_running; }
            private set { m_running = value; }
        }

        internal CaptureTaskState State
        {
            get { return m_state; }
            private set
            {
                CaptureTaskState oldState = m_state;
                m_state = value;
                
                if (oldState != m_state)
                {
                    CaptureStateChanged?.Invoke(this, oldState, m_state);
                }
            }
        }

        internal Rect CaptureRegion
        {
            get { return m_selectionWindow.CaptureRegion; }
        }

        internal Rect InitialCaptureRegion
        {
            get { return m_selectionWindow.InitialCaptureRegion; }
        }

        internal double FramesPerSecond
        {
            get { return m_framesPerSecond; }
        }

        internal bool CanSend
        {
            get { return m_canSend; }
            set
            {
                lock (this)
                {
                    m_canSend = value;
                }
            }
        }
        #endregion

        #region Initialization
        private Rect InitializeRegion()
        {
            double width = 0.0;
            double height = 0.0;

            if (m_session != null
                && m_session.Device != null)
            {
                Device.Display display = m_session.Device[Device.ModuleType.Display] as Device.Display;

                width = display.Width;
                height = display.Height;

                switch (display.Orientation)
                {
                    default:
                    case DeviceOrientation.Unknown:
                    case DeviceOrientation.Portrait:
                    case DeviceOrientation.PortraitUpsideDown:
                        // do nothing
                        break;
                    case DeviceOrientation.LandscapeLeft:
                    case DeviceOrientation.LandscapeRight:
                        // flip width and height
                        double tmp = width;
                        width = height;
                        height = tmp;
                        break;
                }
            }
            else
            {
                // this is an iPhone 8 in landscape for now
                width = 1334.0;
                height = 750.0;
            }

            // modify size
            width *= Constants.InitialRegionSizeFactor;
            height *= Constants.InitialRegionSizeFactor;

            return new Rect(0.0, 0.0, width, height);
        }
        #endregion

        #region Show / Hide
        internal void ShowSelectionWindow()
        {
            Application.Current.Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        m_selectionWindow.Show();
                    }));
        }

        internal void HideSelectionWindow()
        {
            Application.Current.Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        m_selectionWindow.Hide();
                    }));
        }
        #endregion

        #region Starting / Stopping
        internal void Start()
        {            
            if (!(IsRunning))
            {
                Reset(m_selectionWindow.CaptureRegion);

                IsRunning = true;
                m_captureThread = new Thread(new ThreadStart(Iterate))
                {
                    Name = "ScreenCaptureHandler#0",
                    IsBackground = true
                };
                m_captureThread.Start();
            }
        }

        internal void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                if (!(m_captureThread.Join(100)))
                {
                    m_captureThread.Abort();
                }
                m_captureThread = null;
            }

            if (m_selectionWindow != null)
            {
                Application.Current.Dispatcher.Invoke(
                    new Action(
                        delegate ()
                        {
                            m_selectionWindow.IsVisibleChanged -= SelectionWindowVisibilityChanged;
                            m_selectionWindow.Loaded -= SelectionWindowLoaded;
                            m_selectionWindow.Close();
                        }));

                m_selectionWindow = null;
            }
        }
        #endregion

        #region Manual Adjustments
        internal void SetCaptureWidth(int newWidth)
        {
            Application.Current.Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        m_selectionWindow.SetCaptureWidth(newWidth);
                    }));
        }

        internal void SetCaptureHeight(int newHeight)
        {
            Application.Current.Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        m_selectionWindow.SetCaptureHeight(newHeight);
                    }));
        }

        internal void SetCaptureLocation(System.Windows.Point newLocation)
        {
            Application.Current.Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        m_selectionWindow.SetCaptureLocation(newLocation);
                    }));
        }

        internal void SetCaptureX(int newX)
        {
            Application.Current.Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        m_selectionWindow.SetCaptureX(newX);
                    }));
        }

        internal void SetCaptureY(int newY)
        {
            Application.Current.Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        m_selectionWindow.SetCaptureY(newY);
                    }));
        }
        #endregion

        #region Update / Reset
        private void Reset(Rect captureRegion)
        {
            int width = (int)captureRegion.Width;
            int height = (int)captureRegion.Height;

            m_prevScreenshot = new Bitmap(
                width, height, PixelFormat.Format32bppArgb);
            m_currScreenshot = new Bitmap(
                width, height, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(m_prevScreenshot))
            {
                g.Clear(Color.Transparent);
            }

            m_buffer = new byte[width * height * 4];
            m_firstRun = true;
            m_frameCount = 0;
        }
        #endregion

        #region Capturing/Processing
        private void CaptureScreen(Rect captureRegion)
        {
            using (Graphics g = Graphics.FromImage(m_currScreenshot))
            {
                g.CopyFromScreen((int)Math.Round(captureRegion.X), (int)Math.Round(captureRegion.Y), 
                    0, 0, new System.Drawing.Size((int)captureRegion.Width, (int)captureRegion.Height), 
                    CopyPixelOperation.SourceCopy);
            }
        }

        private unsafe void ApplyXor(BitmapData prevData, BitmapData currData, ref bool hasData, ref Rectangle updateRect)
        {
            byte* prev = (byte*)prevData.Scan0.ToPointer();
            byte* curr = (byte*)currData.Scan0.ToPointer();

            int height = prevData.Height;
            int width = currData.Width;

            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            hasData = false;
            updateRect = Rectangle.Empty;

            fixed (byte* target = m_buffer)
            {
                uint* dst = (uint*)target;
                for (int y = 0; y < height; y++)
                {
                    uint* prevRow = (uint*)(prev + prevData.Stride * y);
                    uint* curRow = (uint*)(curr + currData.Stride * y);

                    for (int x = 0; x < width; x++)
                    {
                        uint tmp = curRow[x] ^ prevRow[x];
                        *(dst++) = tmp;

                        bool hasLocalData = (tmp != 0);
                        hasData |= hasLocalData;

                        /* if (hasLocalData)
                        {
                            minX = Math.Min(minX, x);
                            maxX = Math.Max(maxX, x);

                            minY = Math.Min(minY, y);
                            maxY = Math.Max(maxY, y);
                        } */
                    }
                }
            }

            /* if (hasData)
            {
                updateRect = new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
            } */
        }

        private unsafe byte[] ClipData(Rectangle updateRect, Rectangle fullRect)
        {
            byte[] clippedBuffer = new byte[updateRect.Width * updateRect.Height * 4];
            fixed (byte* source = m_buffer)
            {
                uint* src = (uint*)source;
                fixed (byte* target = clippedBuffer)
                {
                    uint* dst = (uint*)target;
                    for (int y = 0; y < updateRect.Height; y++)
                    {
                        uint* srcPos = src + fullRect.Width * (updateRect.Y + y) + updateRect.X;
                        uint* dstPos = dst + updateRect.Width * y;

                        for (int x = 0; x < updateRect.Width; x++)
                        {
                            dstPos[x] = srcPos[x];
                        }
                    }
                }
            }

            return clippedBuffer;
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }


        private bool CaptureAndSendScreenshot(Rect captureRegion)
        {
            bool useJpg = true;

            bool didSend = false;
            // int currStride = 0;

            CaptureScreen(captureRegion);

            // swap buffers
            /* Bitmap tmp = m_currScreenshot;
            m_currScreenshot = m_prevScreenshot;
            m_prevScreenshot = tmp; */

            MemoryStream strm = new MemoryStream();

            if (useJpg)
            {
                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;

                EncoderParameters myEncoderParameters = new EncoderParameters(1);

                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                m_currScreenshot.Save(strm, jpgEncoder, myEncoderParameters);
            }
            else
            {
                m_currScreenshot.Save(strm, ImageFormat.Png);
            }

            Screenshot screenshot = new Screenshot(strm.GetBuffer());
            ProcessedScreenshotCaptured?.Invoke(this, screenshot);

            strm.Dispose();

            didSend = true;

            /* int width = (int)captureRegion.Width;
            int height = (int)captureRegion.Height;

            BitmapData prevData = m_prevScreenshot.LockBits(
                new Rectangle(0, 0, m_prevScreenshot.Width, m_prevScreenshot.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData currData = m_currScreenshot.LockBits(
                new Rectangle(0, 0, m_currScreenshot.Width, m_currScreenshot.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            currStride = currData.Stride;

            bool hasData = false;
            Rectangle updateRect = Rectangle.Empty;

            try
            {
                ApplyXor(prevData, currData, ref hasData, ref updateRect);

                // swap buffers
                Bitmap tmp = m_currScreenshot;
                m_currScreenshot = m_prevScreenshot;
                m_prevScreenshot = tmp;

                System.IO.MemoryStream strm = new System.IO.MemoryStream();
                m_currScreenshot.Save(strm, ImageFormat.Png);
                Debug.WriteLine(strm.GetBuffer().Length);
            }
            finally
            {
                m_prevScreenshot.UnlockBits(prevData);
                m_currScreenshot.UnlockBits(currData);
            }

            if (hasData)
            {
                // now we need to clip the update
                // byte[] clipped = ClipData(updateRect, new Rectangle(0, 0, width, height));

                Screenshot screenshot = new Screenshot(m_buffer, updateRect,
                // Screenshot screenshot = new Screenshot(clipped, updateRect,
                    new System.Drawing.Size(width, height), currStride, m_firstRun);
                ProcessedScreenshotCaptured?.Invoke(this, screenshot);

                didSend = true;
            } */

            ScreenshotCaptured?.Invoke(this, (Bitmap)m_prevScreenshot.Clone());

            m_firstRun = false;
            m_frameCount++;

            if (m_frameCount == KeyFrameCounter)
            {
                Reset(captureRegion);
            }

            return didSend;
        }

        private bool ShouldResetBuffers(Rect newCaptureRegion)
        {
            if (m_prevScreenshot == null
                || m_currScreenshot == null
                || m_currScreenshot.Width != (int)newCaptureRegion.Width
                || m_currScreenshot.Height != (int)newCaptureRegion.Height)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region Threading
        private void Iterate()
        {
            Stopwatch stopWatch = Stopwatch.StartNew();
            bool shouldStream = (m_session.Device[Device.ModuleType.Display] as Device.Display).ConnectivityType == ConnectivityType.ContinuousStream;
            int numFrames = 0;

            while (IsRunning)
            {
                Thread.Sleep(2);

                if (!(shouldStream))
                {
                    lock (this)
                    {
                        if (!(m_canSend))
                        {
                            continue;
                        }
                        m_canSend = false;
                    }
                }

                Rect captureRegion = m_selectionWindow.CaptureRegion;
                bool shouldReset = ShouldResetBuffers(captureRegion);

                if (shouldReset)
                {
                    Reset(captureRegion);
                }

                bool didSend = CaptureAndSendScreenshot(captureRegion);

                if (!(shouldStream))
                {
                    lock (this)
                    {
                        if (!(didSend))
                        {
                            m_canSend = true;
                        }
                    }
                }

                numFrames++;
                m_framesPerSecond = numFrames / (stopWatch.ElapsedMilliseconds / 1000.0);
            }
        }
        #endregion

        #region Event Handler
        #region Window Event Handler
        private void SelectionWindowLoaded(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void SelectionWindowVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // if the window is shown: state -> CaptureTaskState.Selecting
            if (m_selectionWindow.IsVisible)
            {
                State = CaptureTaskState.Selecting;
            }
            else
            {
                State = CaptureTaskState.Running;
            }
        }
        #endregion
        #endregion
    }
    #endregion
}
