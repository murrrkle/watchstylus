using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;

using Astral.Content;
using Astral.Messaging;
using Astral.Net;
using Astral.Net.Serialization;

namespace Astral.Device
{
    #region Delegates
    public delegate void AstralTouchEventHandler(object sender, AstralTouchEventArgs e);
    
    public delegate void AstralContentEventHandler(object sender, AstralContentEventArgs content);
    #endregion

    #region Class 'AstralTouchEventArgs'
    public class AstralTouchEventArgs : EventArgs
    {
        #region Class Members
        private TouchPoint m_touchPt;
        #endregion

        #region Constructors
        internal AstralTouchEventArgs(TouchPoint touchPt)
        {
            m_touchPt = touchPt;
        }
        #endregion
        
        #region Properties
        public TouchPoint TouchPoint
        {
            get { return m_touchPt; }
        }
        #endregion
    }
    #endregion

    #region Class 'AstralContentEventArgs'
    public class AstralContentEventArgs : EventArgs
    {
        #region Class Members
        private byte[] m_bitmapData;

        private Size m_size;

        private int m_stride;

        private bool m_fullFrame;
        #endregion

        #region Constructors
        internal AstralContentEventArgs(byte[] bitmapData, Size size, int stride)
        {
            m_bitmapData = bitmapData;
            m_size = size;
            m_stride = stride;
            m_fullFrame = false;
        }

        internal AstralContentEventArgs(byte[] bitmapData)
        {
            m_fullFrame = true;
            m_bitmapData = bitmapData;
        }
        #endregion

        #region Properties
        public byte[] BitmapData
        {
            get { return m_bitmapData; }
        }

        public Size Size
        {
            get { return m_size; }
        }

        public int Stride
        {
            get { return m_stride; }
        }

        public bool IsFullFrame
        {
            get { return m_fullFrame; }
        }
        #endregion
    }
    #endregion

    #region Sealed Class 'Display'
    public sealed class Display : IDeviceModule
    {
        #region Constant Class Members
        private const string ResolutionWidthField = "resW";

        private const string ResolutionHeightField = "resH";

        private const string OrientationField = "ori";

        private const string TouchCapabilitiesField = "touC";

        private const string ConnectivityTypeField = "conT";
        #endregion

        #region Class Members
        private Size m_resolution;

        private ConnectivityType m_connectivityType = ConnectivityType.ContinuousStream;

        private DeviceOrientation m_orientation;

        private TouchCapabilities m_touchCaps;

        private byte[] m_currBuffer = null;
        #endregion

        #region Events
        public event AstralTouchEventHandler TouchDown;

        public event AstralTouchEventHandler TouchMove;

        public event AstralTouchEventHandler TouchUp;

        public event AstralContentEventHandler ContentUpdated;

        public event EventHandler DisplayReady;
        #endregion

        #region Constructors
        public Display(Size resolution, DeviceOrientation orientation, 
            TouchCapabilities touchCapabilities, ConnectivityType connectivityType)
            : base("Display")
        {
            m_resolution = resolution;
            m_orientation = orientation;
            m_touchCaps = touchCapabilities;

            m_connectivityType = connectivityType;
        }

        public Display(NetworkStreamInfo info)
            : base(info)
        {
            int width = info.GetInt(ResolutionWidthField);
            int height = info.GetInt(ResolutionHeightField);

            m_resolution = new Size(width, height);
            m_orientation = (DeviceOrientation)Enum.ToObject(
                typeof(DeviceOrientation), info.GetInt(OrientationField));
            m_touchCaps = (TouchCapabilities)Enum.ToObject(
                typeof(TouchCapabilities), info.GetInt(TouchCapabilitiesField));

            m_connectivityType = (ConnectivityType)Enum.ToObject(
                typeof(ConnectivityType), info.GetInt(ConnectivityTypeField));
        }
        #endregion

        #region Properties
        public int Width
        {
            get { return m_resolution.Width; }
        }

        public int Height
        {
            get { return m_resolution.Height; }
        }

        public DeviceOrientation Orientation
        {
            get { return m_orientation; }
            set
            {
                bool changed = (m_orientation != value);
                m_orientation = value;

                if (changed)
                {
                    Message msg = DeviceOrientationMessage.CreateInstance(m_orientation);
                    SendMessage(msg);
                }
            }
        }

        public TouchCapabilities TouchCapabilities
        {
            get { return m_touchCaps; }
        }

        public ConnectivityType ConnectivityType
        {
            get { return m_connectivityType; }
        }
        #endregion

        #region Orientation Handling
        public void RotateLeft()
        {
            switch (m_orientation)
            {
                default:
                case DeviceOrientation.Unknown:
                    // don't do anything
                    break;
                case DeviceOrientation.Portrait:
                    Orientation = DeviceOrientation.LandscapeRight;
                    break;
                case DeviceOrientation.LandscapeRight:
                    Orientation = DeviceOrientation.PortraitUpsideDown;
                    break;
                case DeviceOrientation.PortraitUpsideDown:
                    Orientation = DeviceOrientation.LandscapeLeft;
                    break;
                case DeviceOrientation.LandscapeLeft:
                    Orientation = DeviceOrientation.Portrait;
                    break;
            }
        }

        public void RotateRight()
        {
            switch (m_orientation)
            {
                default:
                case DeviceOrientation.Unknown:
                    // don't do anything
                    break;
                case DeviceOrientation.Portrait:
                    Orientation = DeviceOrientation.LandscapeLeft;
                    break;
                case DeviceOrientation.LandscapeLeft:
                    Orientation = DeviceOrientation.PortraitUpsideDown;
                    break;
                case DeviceOrientation.PortraitUpsideDown:
                    Orientation = DeviceOrientation.LandscapeRight;
                    break;
                case DeviceOrientation.LandscapeRight:
                    Orientation = DeviceOrientation.Portrait;
                    break;
            }
        }
        #endregion

        #region Visual Handling
        public void UpdateContent(Screenshot screenshot)
        {
            Message screenshotMsg = ScreenshotMessage.CreateInstance(screenshot);
            SendMessage(screenshotMsg);
        }
        
        internal AstralContentEventArgs ProcessScreenshot(Screenshot screenshot)
        {
            // prev = m_currBuffer
            // curr = screenshot.Data

            // TODO: change this to be transferrable!
            int pixelSize = 4;

            int width = screenshot.Size.Width;
            int height = screenshot.Size.Height;
            int stride = screenshot.Stride;

            Rectangle updateArea = screenshot.UpdateArea;
            int offsetX = updateArea.X;
            int offsetY = updateArea.Y;
            int updateWidth = updateArea.Width;
            int updateHeight = updateArea.Height;

            byte[] result = new byte[width * height * pixelSize];

            if (screenshot.IsFullFrame
                || m_currBuffer == null)
            {
                // re-initialize the whole buffer
                m_currBuffer = new byte[width * height * pixelSize];
            }

            unsafe
            {
                // create temporary copy
                Array.Copy(m_currBuffer, result, m_currBuffer.Length);

                fixed (byte* tmpPrev = m_currBuffer)
                {
                    uint* prev = (uint*)tmpPrev;
                    fixed (byte* tmpCurr = screenshot.Data)
                    {
                        uint* curr = (uint*)tmpCurr;
                        fixed (byte* target = result)
                        {
                            uint* dst = (uint*)target;
                            for (int y = 0; y < updateHeight; y++)
                            {
                                int offset = width * (offsetY + y);

                                uint* targetPos = dst + offset;
                                uint* prevRow = prev + offset;
                                uint* curRow = curr + updateWidth * y;

                                for (int x = 0; x < updateWidth; x++)
                                {
                                    targetPos[x + offsetX] = curRow[x] ^ prevRow[x + offsetX];
                                }
                            }
                        }
                    }
                }

                // copy it back
                Array.Copy(result, m_currBuffer, result.Length);
            }

            return (new AstralContentEventArgs(result, screenshot.Size, screenshot.Stride));
        }
        #endregion

        #region Touch Handling
        public void UpdateTouchPoint(TouchPoint touchPt)
        {
            Message touchPtMsg = TouchPointMessage.CreateInstance(touchPt);
            SendMessage(touchPtMsg);
        }
        #endregion

        #region Overrides (IDeviceModule)
        public override ModuleType Type => ModuleType.Display;

        internal override void GetStreamDataInternal(NetworkStreamInfo info)
        {
            info.AddValue(ResolutionWidthField, m_resolution.Width);
            info.AddValue(ResolutionHeightField, m_resolution.Height);
            info.AddValue(OrientationField, (int)m_orientation);
            info.AddValue(TouchCapabilitiesField, (int)m_touchCaps);
            info.AddValue(ConnectivityTypeField, (int)m_connectivityType);
        }

        protected override void ProcessModuleMessage(Message msg)
        {
            // check what type of message this is
            if (TouchPointMessage.IsKindOf(msg))
            {
                TouchPoint touchPt = TouchPointMessage.ToTouchPoint(msg);
                if (touchPt != null)
                {
                    switch (touchPt.State)
                    {
                        default:
                        case TouchState.None:
                            // do nothing
                            break;
                        case TouchState.Began:
                            TouchDown?.Invoke(this, new AstralTouchEventArgs(touchPt));
                            break;
                        case TouchState.Moved:
                            TouchMove?.Invoke(this, new AstralTouchEventArgs(touchPt));
                            break;
                        case TouchState.Ended:
                            TouchUp?.Invoke(this, new AstralTouchEventArgs(touchPt));
                            break;
                    }
                }
            }
            else if (ScreenshotMessage.IsKindOf(msg))
            {
                Screenshot screenshot = ScreenshotMessage.ToScreenshot(msg);

                if (ConnectivityType == ConnectivityType.RequestResponse)
                {
                    // send response
                    Message recvMsg = ContentReceivedMessage.CreateInstance(ContentType.Screenshot);
                    SendMessage(recvMsg);
                }

                if (screenshot.IsFullFrame)
                {
                    AstralContentEventArgs rawData = new AstralContentEventArgs(screenshot.Data);
                    ContentUpdated?.Invoke(this, rawData);
                }
                else
                {
                    AstralContentEventArgs rawData = ProcessScreenshot(screenshot);
                    ContentUpdated?.Invoke(this, rawData);
                }
            }
            else if (DeviceOrientationMessage.IsKindOf(msg))
            {
                m_orientation = DeviceOrientationMessage.ToOrientation(msg);
            }
            else if (ContentReceivedMessage.IsKindOf(msg))
            {
                DisplayReady?.Invoke(this, EventArgs.Empty);
            }
        }
        #endregion
    }
    #endregion
}
