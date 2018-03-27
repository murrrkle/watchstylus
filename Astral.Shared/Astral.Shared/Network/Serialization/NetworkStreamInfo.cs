using System;
using System.Collections.Generic;
using System.Text;

namespace Astral.Net.Serialization
{
    // TODO: add documentation

    #region Class 'NetworkStreamInfo'
    public class NetworkStreamInfo
    {
        #region Class Members
        private Dictionary<Descriptor, byte[]> m_data;
        #endregion

        #region Constructors
        public NetworkStreamInfo()
        {
            m_data = new Dictionary<Descriptor, byte[]>();
        }
        #endregion

        #region Properties
        internal List<Descriptor> Descriptors
        {
            get
            {
                List<Descriptor> descriptors = new List<Descriptor>();
                foreach (Descriptor descriptor in m_data.Keys)
                {
                    descriptors.Add(descriptor);
                }
                return descriptors;
            }
        }
        #endregion

        #region Checks
        public bool ContainsKey(string name, TransferType type)
        {
            lock (this)
            {
                return (ContainsKey(new Descriptor(name, type)));
            }
        }

        public bool ContainsKey(string name)
        {
            lock (this)
            {
                return (ContainsKey(new Descriptor(name, TransferType.Unknown)));
            }
        }

        internal bool ContainsKey(Descriptor descriptor)
        {
            if (m_data != null
                && m_data.ContainsKey(descriptor))
            {
                return true;
            }
            return false;
        }
        #endregion

        #region Nullify Methods
        private bool IsNullable(TransferType type)
        {
            return (type == TransferType.String
                || type == TransferType.Object
                || type == TransferType.Array
                || type == TransferType.List
                || type == TransferType.Dictionary);
        }
        #endregion

        #region Adding and Retrieving
        #region Adding Objects
        public void AddValue(string name, bool value)
        {
            AddValue(name, (object)value, TransferType.Bool);
        }

        public void AddValue(string name, byte value)
        {
            AddValue(name, (object)value, TransferType.Byte);
        }

        public void AddValue(string name, double value)
        {
            AddValue(name, (object)value, TransferType.Double);
        }

        public void AddValue(string name, float value)
        {
            AddValue(name, (object)value, TransferType.Float);
        }

        public void AddValue(string name, int value)
        {
            AddValue(name, (object)value, TransferType.Int);
        }

        public void AddValue(string name, long value)
        {
            AddValue(name, (object)value, TransferType.Long);
        }

        public void AddValue(string name, short value)
        {
            AddValue(name, (object)value, TransferType.Short);
        }

        public void AddValue(string name, string value)
        {
            AddValue(name, (object)value, TransferType.String);
        }
        
        public void AddValue(string name, object value)
        {
            // figure out, whether this is an object or array/list/dictioary
            TransferType transferType = TransferType.Object;
            if (value != null)
            {
                transferType = ObjectConverter.GetTransferType(value);
            }

            AddValue(name, value, transferType);
        }

        private void AddValue(string name, object value,
            TransferType internalType)
        {
            bool shouldReplaceValue = false;
            Descriptor existingDescriptor = null;

            bool isNull = false;
            byte[] bytes = null;

            if (name == null
                || name.Equals(""))
            {
                throw new EncodingException("The given name '" + name
                    + "' is invalid. Names cannot be 'null' and "
                    + "must contain at least one character!");
            }
            else if (ContainsKey(name))
            {
                existingDescriptor = GetDescriptor(name);
                if (existingDescriptor.Type == internalType)
                {
                    // override it
                    shouldReplaceValue = true;
                }
                else
                {
                    throw new EncodingException("The given name '" + name
                        + "' already exists in this context with a different transfer type. "
                        + "You can only override values with the same transfer type!");
                }
            }

            if (value == null)
            {
                if (IsNullable(internalType))
                {
                    bytes = new byte[] { };
                    isNull = true;
                }
                else
                {
                    throw new EncodingException("There was an error encoding the object "
                        + "[internal type '" + internalType + "' is not nullable]");
                }
            }
            else
            {
                if (internalType != TransferType.Unknown)
                {
                    bytes = ObjectConverter.ToBytes(value, internalType);
                    isNull = false;
                }
                else
                {
                    throw new EncodingException("There was an error encoding the object "
                        + "[internal type is 'Unknown', name = '" + name + "']");
                }
            }

            if (bytes != null)
            {
                if (shouldReplaceValue
                    && existingDescriptor != null)
                {
                    m_data.Remove(existingDescriptor);
                    m_data.Add(new Descriptor(
                        name, internalType, isNull), bytes);
                }
                else
                {
                    m_data.Add(new Descriptor(
                        name, internalType, isNull), bytes);
                }
            }
        }
        #endregion

        #region Retrieving Objects
        private Descriptor GetDescriptor(string name)
        {
            return (GetDescriptor(name, TransferType.Unknown));
        }

        private Descriptor GetDescriptor(string name, TransferType type)
        {
            IEnumerator<Descriptor> keys = m_data.Keys.GetEnumerator();
            while (keys.MoveNext())
            {
                Descriptor key = keys.Current;
                if (key.Name != null
                    && key.Name.Equals(name)
                    && (key.Type == type
                        || type == TransferType.Unknown
                        || key.Type == TransferType.Unknown))
                {
                    return key;
                }
            }
            return null;
        }

        private byte[] GetData(Descriptor descriptor)
        {
            if (m_data.ContainsKey(descriptor))
            {
                return m_data[descriptor];
            }
            return null;
        }

        public bool GetBool(string name)
        {
            return (bool)GetValue(name, typeof(bool));
        }

        public byte GetByte(string name)
        {
            return (byte)GetValue(name, typeof(byte));
        }

        public double GetDouble(string name)
        {
            return (double)GetValue(name, typeof(double));
        }

        public float GetFloat(string name)
        {
            return (float)GetValue(name, typeof(float));
        }

        public int GetInt(string name)
        {
            return (int)GetValue(name, typeof(int));
        }

        public long GetLong(string name)
        {
            return (long)GetValue(name, typeof(long));
        }

        public short GetShort(string name)
        {
            return (short)GetValue(name, typeof(short));
        }

        public string GetString(string name)
        {
            return (string)GetValue(name, typeof(string));
        }

        public object GetValue(string name, Type type)
        {
            if (name == null
                || name.Equals(""))
            {
                throw new DecodingException("The given name '" + name
                    + "' is invalid. Names cannot be 'null' and "
                    + "must contain at least one character!");
            }

            TransferType internalType = ObjectConverter.GetTransferType(type);

            Descriptor descriptor = GetDescriptor(name, internalType);
            if (descriptor == null)
            {
                throw new DecodingException("The given name '" + name + "' with the type '"
                    + internalType + "' does not exist in this context.");
            }

            byte[] data = GetData(descriptor);
            if (data == null)
            {
                throw new DecodingException("The given name '" + name + "' does not exist in this context.");
            }

            internalType = descriptor.Type;
            object value = null;

            if (data != null
                && internalType != TransferType.Unknown)
            {
                if (descriptor.IsNull)
                {
                    value = null;
                }
                else
                {
                    value = ObjectConverter.FromBytes(data, type);
                }
            }
            else
            {
                throw new DecodingException("There was an error decoding this object "
                    + "[internal type is 'Unknown', name = '" + name + "']");
            }

            return value;
        }
        #endregion
        #endregion

        #region Serialization / Deserialization
        #region Serializating Objects
        private byte[] CreateHeader(string name, TransferType type,
            int contentLength, bool isNull)
        {
            List<byte> header = new List<byte>();

            byte[] encodedName = Encoding.UTF8.GetBytes(name);

            // start with the name length (SHORT = 2 bytes)
            header.AddRange(ObjectConverter.Encode(encodedName.Length, 2));

            // continue with the name (STRING)
            header.AddRange(encodedName);

            // continue with the content type byte (BYTE = 1 byte)
            header.AddRange(ObjectConverter.Encode((int)type, 1));

            // continue with the null byte (BYTE = 1 byte)
            header.AddRange(ObjectConverter.Encode((int)(isNull ? 1 : 0), 1));

            // finalize with the content length bytes (INT = 4 bytes)
            header.AddRange(ObjectConverter.Encode(contentLength, 4));

            return header.ToArray();
        }

        internal byte[] Serialize()
        {
            int totalLength = 0;
            List<byte> bytes = new List<byte>();

            IEnumerator<Descriptor> keys = m_data.Keys.GetEnumerator();
            while (keys.MoveNext())
            {
                Descriptor key = keys.Current;
                byte[] content = m_data[key];

                byte[] header = CreateHeader(key.Name, key.Type,
                    content.Length, key.IsNull);

                bytes.AddRange(header);
                bytes.AddRange(content);

                totalLength += (header.Length + content.Length);
            }

            bytes.InsertRange(0, ObjectConverter.Encode(totalLength, 4));

            return bytes.ToArray();
        }
        #endregion

        #region Deserializating Objects
        internal int Deserialize(byte[] rawData)
        {
            int counter = 0;

            // read length of NetworkStreamInfo
            int totalLength = rawData[counter] << 24;
            totalLength |= rawData[counter + 1] << 16;
            totalLength |= rawData[counter + 2] << 8;
            totalLength |= rawData[counter + 3];

            counter += 4;

            while (counter - 4 < totalLength)
            {
                int nameLength = rawData[counter] << 8;
                nameLength |= rawData[counter + 1];
                counter += 2;

                byte[] nameBytes = new byte[nameLength];
                Array.Copy(rawData, counter, nameBytes, 0, nameLength);
                counter += nameLength;

                Descriptor descriptor = new Descriptor(
                    Encoding.UTF8.GetString(nameBytes),
                    (TransferType)Enum.ToObject(typeof(TransferType),
                        (int)rawData[counter]), (int)rawData[counter + 1] == 1);
                counter += 2;

                int contentLength = rawData[counter] << 24;
                contentLength |= rawData[counter + 1] << 16;
                contentLength |= rawData[counter + 2] << 8;
                contentLength |= rawData[counter + 3];
                counter += 4;

                // content
                byte[] contentBytes = new byte[contentLength];
                Array.Copy(rawData, counter, contentBytes, 0, contentLength);

                m_data.Add(descriptor, contentBytes);

                counter += contentLength;
            }

            return totalLength;
        }
        #endregion
        #endregion
    }
    #endregion
}
