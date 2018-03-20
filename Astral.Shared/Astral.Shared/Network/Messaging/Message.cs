using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Astral.Net.Serialization;

namespace Astral.Net
{
    // TODO: finish documentation

    #region Class 'Message'
    public class Message
    {
        #region Class Members
        private NetworkStreamInfo m_content;

        private string m_name;

        private bool m_internal = false;
        #endregion

        #region Constructors
        public Message(string messageName)
            : this(messageName, false)
        { }

        internal Message(string messageName, bool isInternal)
        {
            m_content = new NetworkStreamInfo();
            m_name = messageName;
            m_internal = isInternal;
        }

        private Message(MessageHeader header)
        {
            m_internal = header.IsInternal;
            m_name = header.Name;
        }
        #endregion

        #region Properties
        internal bool IsInternal
        {
            get { return m_internal; }
            set { m_internal = value; }
        }

        internal NetworkStreamInfo Content
        {
            get { return m_content; }
            set { m_content = value; }
        }

        internal List<Descriptor> Descriptors
        {
            get { return m_content.Descriptors; }
        }

        public string Name
        {
            get { return m_name; }
        }
        #endregion

        #region Check Methods
        public bool ContainsField(string name, TransferType type)
        {
            lock (this)
            {
                return (ContainsField(new Descriptor(name, type)));
            }
        }

        public bool ContainsField(string name)
        {
            lock (this)
            {
                return (ContainsField(new Descriptor(name, TransferType.Unknown)));
            }
        }

        internal bool ContainsField(Descriptor descriptor)
        {
            lock (this)
            {
                return (m_content.ContainsKey(descriptor));
            }
        }
        #endregion

        #region Add/Get Methods
        #region Add Methods
        public void AddField(string name, bool value)
        {
            m_content.AddValue(name, value);
        }

        public void AddField(string name, byte value)
        {
            m_content.AddValue(name, value);
        }

        public void AddField(string name, double value)
        {
            m_content.AddValue(name, value);
        }

        public void AddField(string name, float value)
        {
            m_content.AddValue(name, value);
        }

        public void AddField(string name, int value)
        {
            m_content.AddValue(name, value);
        }

        public void AddField(string name, long value)
        {
            m_content.AddValue(name, value);
        }

        public void AddField(string name, short value)
        {
            m_content.AddValue(name, value);
        }

        public void AddField(string name, string value)
        {
            m_content.AddValue(name, value);
        }

        public void AddField(string name, object value)
        {
            m_content.AddValue(name, value);
        }
        #endregion

        #region Get Methods
        public bool GetBoolField(string name)
        {
            return (bool)GetField(name, typeof(bool));
        }

        public byte GetByteField(string name)
        {
            return (byte)GetField(name, typeof(byte));
        }

        public double GetDoubleField(string name)
        {
            return (double)GetField(name, typeof(double));
        }

        public float GetFloatField(string name)
        {
            return (float)GetField(name, typeof(float));
        }

        public int GetIntField(string name)
        {
            return (int)GetField(name, typeof(int));
        }

        public long GetLongField(string name)
        {
            return (long)GetField(name, typeof(long));
        }

        public short GetShortField(string name)
        {
            return (short)GetField(name, typeof(short));
        }

        public string GetStringField(string name)
        {
            return (string)GetField(name, typeof(string));
        }

        public object GetField(string name, Type type)
        {
            return m_content.GetValue(name, type);
        }
        #endregion
        #endregion

        #region Encoding / Decoding Methods
        #region Ecndoing Methods
        private byte[] CreateHeader(int messageLength)
        {
            List<byte> header = new List<byte>();
            byte[] encodedName = Encoding.UTF8.GetBytes(Name);

            // start with the message length (INT = 4 bytes)
            header.AddRange(ObjectConverter.Encode(messageLength, 4));

            // continue with the control flag (BYTE = 1 byte)
            header.AddRange(ObjectConverter.Encode((IsInternal ? 1 : 0), 1));

            // continue with the name length (SHORT = 2 bytes)
            header.AddRange(ObjectConverter.Encode(encodedName.Length, 2));

            // continue with the name (STRING)
            header.AddRange(encodedName);

            return header.ToArray();
        }

        internal byte[] ToByteArray()
        {
            // ok, create the message
            byte[] content = m_content.Serialize();
            int totalLength = content.Length;

            byte[] header = CreateHeader(totalLength);

            List<byte> rawData = new List<byte>();
            rawData.AddRange(header);
            rawData.AddRange(content);

            return (rawData.ToArray());
        }
        #endregion

        #region Decoding Methods
        internal static Message FromStream(MessageHeader header, byte[] rawData)
        {
            Message message = new Message(header);

            NetworkStreamInfo info = new NetworkStreamInfo();
            info.Deserialize(rawData);

            message.Content = info;

            return message;
        }
        #endregion
        #endregion
    }
    #endregion
}
