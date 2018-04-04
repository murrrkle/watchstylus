using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

using CoreGraphics;
using Foundation;
using UIKit;

using Astral.Device;

namespace Astral.iOS
{
    [Register("ScreenshotView")]
    public class ScreenshotView : UIImageView
    {
        #region Class Members
        #region Core Class Members
        private Display m_display;
        #endregion

        #region Imaging Class Members
        private UIImage m_currImg;
        #endregion

        #region Touch Class Members
        private Dictionary<IntPtr, TouchPoint> m_touchPts;
        #endregion
        #endregion

        #region Constructors
        public ScreenshotView()
        {
            Initialize();
        }

        public ScreenshotView(RectangleF bounds)
            : base(bounds)
        {
            Initialize();
        }

        public ScreenshotView(IntPtr p)
            : base(p)
        {
            Initialize();
        }
        #endregion

        #region Initialization
        private void Initialize()
        {
            m_touchPts = new Dictionary<IntPtr, TouchPoint>();
            m_currImg = null;
        }
        #endregion

        #region Properties
        public Display Display
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

        #region Screenshot Handling
        private UIImage RotateImage(UIImage source)
        {
            CGSize newSize = source.Size;
            switch (m_display.Orientation)
            {
                default:
                case DeviceOrientation.Unknown:
                case DeviceOrientation.Portrait:
                case DeviceOrientation.PortraitUpsideDown:
                    // don't do anything
                    break;
                case DeviceOrientation.LandscapeLeft:
                case DeviceOrientation.LandscapeRight:
                    // flip size
                    newSize = new CGSize(source.Size.Height, source.Size.Width);
                    break;
            }

            UIGraphics.BeginImageContext(newSize);
            CGContext context = UIGraphics.GetCurrentContext();

            switch (m_display.Orientation)
            {
                default:
                case DeviceOrientation.Unknown:
                case DeviceOrientation.Portrait:
                    // don't do anything
                    break;
                case DeviceOrientation.PortraitUpsideDown:
                    // rotate 180 deg
                    context.RotateCTM((float)Math.PI);
                    context.TranslateCTM(-source.Size.Width, -source.Size.Height);
                    break;
                case DeviceOrientation.LandscapeRight:
                    // rotate +90 deg
                    context.RotateCTM((float)(Math.PI / 2.0));
                    context.TranslateCTM(0.0f, -source.Size.Height);
                    break;
                case DeviceOrientation.LandscapeLeft:
                    // rotate -90 deg
                    context.RotateCTM((float)(-Math.PI / 2.0));
                    context.TranslateCTM(-source.Size.Width, 0.0f);
                    break;
            }

            source.Draw(new CGPoint(0.0, 0.0));

            UIImage image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            return image;
        }
        
        private void UpdateContent(AstralContentEventArgs e)
        {
            if (e.IsFullFrame)
            {
                m_currImg = RotateImage(new UIImage(NSData.FromArray(e.BitmapData)));
                Image = m_currImg;
            }
            else
            {
                // setup image data
                int width = e.Size.Width;
                int height = e.Size.Height;

                int bytesPerPixel = 4;
                int bytesPerRow = bytesPerPixel * width;
                int numBytes = bytesPerRow * height;

                CGImage image = null;
                IntPtr pixels = IntPtr.Zero;

                try
                {
                    pixels = Marshal.AllocHGlobal(numBytes);

                    using (CGColorSpace colorSpace = CGColorSpace.CreateDeviceRGB())
                    {
                        using (CGBitmapContext context = new CGBitmapContext(
                            pixels, width, height, 8, bytesPerRow,
                            colorSpace, CGBitmapFlags.PremultipliedFirst | CGBitmapFlags.ByteOrder32Little))
                        {
                            unsafe
                            {
                                byte* currentPixel = (byte*)pixels.ToPointer();
                                fixed (byte* newPixels = e.BitmapData)
                                {
                                    byte* screenshotPixels = newPixels;
                                    for (int i = 0; i < height; i++)
                                    {
                                        for (int j = 0; j < width; j++)
                                        {
                                            *currentPixel = *screenshotPixels;
                                            *(currentPixel + 1) = *(screenshotPixels + 1);
                                            *(currentPixel + 2) = *(screenshotPixels + 2);
                                            *(currentPixel + 3) = *(screenshotPixels + 3);

                                            screenshotPixels += bytesPerPixel;
                                            currentPixel += bytesPerPixel;
                                        }
                                    }
                                }
                            }

                            image = context.ToImage();
                        }

                        m_currImg = RotateImage(new UIImage(image));
                        Image = m_currImg;
                    }
                }
                finally
                {
                    if (pixels != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(pixels);
                    }
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

        #region Event Handler
        #region Screenshot Handler
        private void OnDisplayContentUpdated(object sender, AstralContentEventArgs content)
        {
            InvokeOnMainThread(() =>
            {
                UpdateContent(content);
            });
        }
        #endregion

        #region Touch Event Handler
        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            float scale = (float)UIScreen.MainScreen.Scale;

            foreach (UITouch touch in touches.Cast<UITouch>())
            {
                if (touch.Phase == UITouchPhase.Began
                    && !(m_touchPts.ContainsKey(touch.Handle)))
                {
                    CGPoint location = touch.LocationInView(this);
                    TouchState state = TouchState.Began;
                    int id = GetNextTouchId();

                    TouchPoint touchPoint = new TouchPoint(id, state, 
                        location.X * scale, location.Y * scale);
                    m_touchPts.Add(touch.Handle, touchPoint);

                    if (m_display != null)
                    {
                        m_display.UpdateTouchPoint(touchPoint);
                    }
                }
            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);
            float scale = (float)UIScreen.MainScreen.Scale;

            foreach (UITouch touch in touches.Cast<UITouch>())
            {
                if (touch.Phase == UITouchPhase.Moved
                    && m_touchPts.ContainsKey(touch.Handle))
                {
                    CGPoint location = touch.LocationInView(this);
                    TouchState state = TouchState.Moved;

                    TouchPoint touchPoint = m_touchPts[touch.Handle];
                    touchPoint.State = state;
                    touchPoint.X = location.X * scale;
                    touchPoint.Y = location.Y * scale;

                    if (m_display != null)
                    {
                        m_display.UpdateTouchPoint(touchPoint);
                    }
                }
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            float scale = (float)UIScreen.MainScreen.Scale;

            foreach (UITouch touch in touches.Cast<UITouch>())
            {
                if ((touch.Phase == UITouchPhase.Ended
                    || touch.Phase == UITouchPhase.Cancelled)
                    && m_touchPts.ContainsKey(touch.Handle))
                {
                    CGPoint location = touch.LocationInView(this);
                    TouchState state = TouchState.Ended;

                    TouchPoint touchPoint = m_touchPts[touch.Handle];
                    touchPoint.State = state;
                    touchPoint.X = location.X * scale;
                    touchPoint.Y = location.Y * scale;

                    if (m_display != null)
                    {
                        m_display.UpdateTouchPoint(touchPoint);
                    }

                    m_touchPts.Remove(touch.Handle);
                }
            }
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);

            TouchesEnded(touches, evt);
        }
        #endregion
        #endregion
    }
}