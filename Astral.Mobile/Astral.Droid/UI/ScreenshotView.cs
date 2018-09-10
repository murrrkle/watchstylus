using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Java.Nio;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using Astral.Device;
using Android.Graphics;

using System.ComponentModel;

namespace Astral.Droid.UI
{
    [Register("Astral.Droid.UI.ScreenshotView")]
    public class ScreenshotView : ImageView
    {
        #region Class Members

        #region Core Class Members
        private Device.Display m_display;
        
        #endregion

        #region Imaging Class Members
        private Android.Graphics.Bitmap m_currImg;
        #endregion

        #region Touch Class Members
        private Dictionary<int, TouchPoint> m_touchPts;
        #endregion
        #endregion

        #region Core Android Setup
        public ScreenshotView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public ScreenshotView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }
        #endregion

        #region Initialization
        private void Initialize()
        {
            m_touchPts = new Dictionary<int, TouchPoint>();
            m_currImg = null;
        }
        #endregion

        #region Properties
        public Device.Display Screen
        {
            get { return m_display; }
            set
            {
                if (value == null
                    && m_display != null)
                {
                    m_display.ContentUpdated -= OnDisplayContentUpdated;
                }

                m_display = value;

                if (m_display != null)
                {
                    m_display.ContentUpdated += OnDisplayContentUpdated;
                }
            }
        }
        #endregion

        #region Touch Methods
        private int GetNextTouchId()
        {
            int lowestId = 0;

            lock (m_touchPts)
            {
                foreach (TouchPoint touchPt in m_touchPts.Values)
                {
                    if (touchPt.Id >= lowestId)
                    {
                        lowestId = touchPt.Id + 1;
                    }
                }
            }

            return lowestId;
        }
        #endregion

        #region Touch Handling
        private void TouchDown(MotionEvent e)
        {
            int pointerIndex = e.ActionIndex;
            int pointerId = e.GetPointerId(pointerIndex);

            System.Drawing.PointF location = new System.Drawing.PointF(e.GetX(pointerIndex), e.GetY(pointerIndex));
            TouchState state = TouchState.Began;
            int id = GetNextTouchId();

            TouchPoint touchPoint = new TouchPoint(id, state, location.X, location.Y);
            m_touchPts.Add(pointerId, touchPoint);

            if (m_display != null)
            {
                m_display.UpdateTouchPoint(touchPoint);
            }
        }

        private void TouchMove(MotionEvent e)
        {
            for (int pointerIndex = 0; pointerIndex < e.PointerCount; pointerIndex++)
            {
                int pointerId = e.GetPointerId(pointerIndex);

                System.Drawing.PointF location = new System.Drawing.PointF(e.GetX(pointerIndex), e.GetY(pointerIndex));
                TouchState state = TouchState.Moved;

                TouchPoint touchPoint = m_touchPts[pointerId];
                touchPoint.State = state;
                touchPoint.X = location.X;
                touchPoint.Y = location.Y;

                if (m_display != null)
                {
                    m_display.UpdateTouchPoint(touchPoint);
                }
            }
        }

        private void TouchUp(MotionEvent e)
        {
            int pointerIndex = e.ActionIndex;
            int pointerId = e.GetPointerId(pointerIndex);

            System.Drawing.PointF location = new System.Drawing.PointF(e.GetX(pointerIndex), e.GetY(pointerIndex));
            TouchState state = TouchState.Ended;

            TouchPoint touchPoint = m_touchPts[pointerId];
            touchPoint.State = state;
            touchPoint.X = location.X;
            touchPoint.Y = location.Y;

            if (m_display != null)
            {
                m_display.UpdateTouchPoint(touchPoint);
            }

            m_touchPts.Remove(pointerId);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            switch (e.ActionMasked)
            {
                default:
                    // don't do anything here
                    break;
                case MotionEventActions.Down:
                case MotionEventActions.PointerDown:
                    TouchDown(e);
                    break;
                case MotionEventActions.Move:
                    TouchMove(e);
                    break;
                case MotionEventActions.Cancel:
                case MotionEventActions.PointerUp:
                case MotionEventActions.Up:
                    TouchUp(e);
                    break;
            }

            return true;
        }
        #endregion

        #region Screenshot Handling
        private Bitmap RotateBitmap(Bitmap image)
        {
            Matrix matrix = new Matrix();
            switch (m_display.Orientation)
            {
                default:
                case DeviceOrientation.Unknown:
                case DeviceOrientation.Portrait:
                    // don't do anything
                    break;
                case DeviceOrientation.PortraitUpsideDown:
                    // rotate 180 deg
                    matrix.PostRotate(180.0f);
                    matrix.PostTranslate(-image.Width, -image.Height);
                    break;
                case DeviceOrientation.LandscapeRight:
                    // rotate +90 deg
                    matrix.PostRotate(90.0f);
                    matrix.PostTranslate(0.0f, -image.Height);
                    break;
                case DeviceOrientation.LandscapeLeft:
                    // rotate -90 deg
                    matrix.PostRotate(-90.0f);
                    matrix.PostTranslate(-image.Width, 0.0f);
                    break;
            }

            return (Bitmap.CreateBitmap(image, 0, 0, image.Width, image.Height, matrix, true));
        }

        private uint ABGRtoARGB(uint argbColor)
        {
            uint r = (argbColor >> 16) & 0xFF;
            uint b = argbColor & 0xFF;

            uint result = (argbColor & 0xFF00FF00) | (b << 16) | r;
            return result;
        }

        private void UpdateContent(AstralContentEventArgs e)
        {
            if (e.IsFullFrame)
            {
                m_currImg = BitmapFactory.DecodeByteArray(e.BitmapData, 0, e.BitmapData.Length);
            }
            else
            {
                int width = e.Size.Width;
                int height = e.Size.Height;

                int bytesPerPixel = 4;
                int bytesPerRow = bytesPerPixel * width;
                int numBytes = bytesPerRow * height;

                if (m_currImg == null
                    || m_currImg.Width != e.Size.Width
                    || m_currImg.Height != e.Size.Height)
                {
                    m_currImg = Bitmap.CreateBitmap(width, height,
                        Bitmap.Config.Argb8888);
                }

                // we need to adjust the bytes here (flip R and B)
                unsafe
                {
                    fixed (byte* bytes = e.BitmapData)
                    {
                        uint* pixels = (uint*)bytes;
                        int length = e.BitmapData.Length / bytesPerPixel;

                        for (int i = 0; i < length; i++)
                        {
                            *pixels = ABGRtoARGB(*pixels);
                            pixels++;
                        }
                    }
                }

                ByteBuffer buffer = ByteBuffer.Wrap(e.BitmapData);
                buffer.Order(ByteOrder.BigEndian);
                m_currImg.CopyPixelsFromBuffer(buffer);
            }
             
            ((Activity)Context).RunOnUiThread(() =>
            {
                SetImageBitmap(RotateBitmap(m_currImg));
                // SetImageBitmap(m_currImg);
            });
        }

        private void OnDisplayContentUpdated(object sender, AstralContentEventArgs content)
        {
            UpdateContent(content);
        }
        #endregion
    }
}