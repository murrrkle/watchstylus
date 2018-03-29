using System;
using System.Diagnostics;
using System.Drawing;

using Astral.Net.Serialization;

using LZ4;

namespace Astral.Content
{
    #region Class 'Screenshot'
    public sealed class Screenshot : ITransferable
    {
        #region Static Class Members
        internal static readonly int PixelSize = 4;
        #endregion

        #region Class Members
        private byte[] m_rawData;

        private byte[] m_compressedData;

        private Rectangle m_updateRect;

        private Size m_size;

        private int m_stride;

        private bool m_fullFrame;
        #endregion

        #region Constructors
        public Screenshot(byte[] rawData, Rectangle updateRect, Size size, int stride, bool isFullFrame)
        {
            // raw data is XOR or full
            m_rawData = rawData;
            m_updateRect = updateRect;
            m_size = size;
            m_stride = stride;
            m_fullFrame = isFullFrame;

            m_compressedData = LZ4Codec.Wrap(m_rawData);
        }

        public Screenshot(NetworkStreamInfo info)
        {
            m_updateRect = new Rectangle(
                info.GetInt("uX"), info.GetInt("uY"),
                info.GetInt("uW"), info.GetInt("uH"));

            m_size = new Size(info.GetInt("w"), info.GetInt("h"));

            m_stride = info.GetInt("str");
            m_fullFrame = info.GetBool("full");
            m_compressedData = (byte[])info.GetValue("data", typeof(byte[]));

            m_rawData = LZ4Codec.Unwrap(m_compressedData);
        }
        #endregion

        #region Overrides (ITransferrable)
        public void GetStreamData(NetworkStreamInfo info)
        {
            // update rect
            info.AddValue("uX", m_updateRect.X);
            info.AddValue("uY", m_updateRect.Y);
            info.AddValue("uW", m_updateRect.Width);
            info.AddValue("uH", m_updateRect.Height);

            // image size
            info.AddValue("w", m_size.Width);
            info.AddValue("h", m_size.Height);

            info.AddValue("str", m_stride);
            info.AddValue("full", m_fullFrame);
            info.AddValue("data", m_compressedData);
        }
        #endregion

        #region Properties
        public byte[] Data
        {
            get { return m_rawData; }
        }

        public Rectangle UpdateArea
        {
            get { return m_updateRect; }
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
}
