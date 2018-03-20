using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Astral.Net.Serialization
{
    #region Class 'ObjectConverter'
    /// <summary>
    ///     <para>Handles all serialization and deserialization.</para>
    /// </summary>
    public class ObjectConverter
    {
        #region Type Conversions
        /// <summary>
        ///     <para>Retrieves the <see cref="Astral.Net.TransferType"/>  of a given object.</para>
        /// </summary>
        /// <returns>The <see cref="Astral.Net.TransferType"/>.</returns>
        /// <param name="value">The object of which the <see cref="Astral.Net.TransferType"/> should be received.</param>
        /// <remarks>Returns <see cref="Astral.Net.TransferType.Unknown"/> if the object <see cref="Astral.Net.TransferType"/> cannot be determined.</remarks>
        internal static TransferType GetTransferType(object value)
        {
            if (value != null)
            {
                return GetTransferType(value.GetType());
            }
            return TransferType.Unknown;
        }

        /// <summary>
        ///     <para>Retrieves the <see cref="Astral.Net.TransferType"/>  of a given object <see cref="System.Type"/>.</para>
        /// </summary>
        /// <returns>The <see cref="Astral.Net.TransferType"/>.</returns>
        /// <param name="type">The object <see cref="System.Type"/> of which the <see cref="Astral.Net.TransferType"/> should be received.</param>
        /// <remarks>Returns <see cref="Astral.Net.TransferType.Unknown"/> if the object <see cref="Astral.Net.TransferType"/> cannot be determined.</remarks>
        internal static TransferType GetTransferType(Type type)
        {
            if (type.Equals(typeof(bool)))
            {
                return TransferType.Bool;
            }
            else if (type.Equals(typeof(byte)))
            {
                return TransferType.Byte;
            }
            else if (type.Equals(typeof(double)))
            {
                return TransferType.Double;
            }
            else if (type.Equals(typeof(float)))
            {
                return TransferType.Float;
            }
            else if (type.Equals(typeof(int)))
            {
                return TransferType.Int;
            }
            else if (type.Equals(typeof(long)))
            {
                return TransferType.Long;
            }
            else if (type.Equals(typeof(short)))
            {
                return TransferType.Short;
            }
            else if (type.Equals(typeof(string)))
            {
                return TransferType.String;
            }
            else if (type.GetInterfaces().Contains(typeof(ITransferable)))
            {
                return TransferType.Object;
            }
            else if (type.IsArray)
            {
                return TransferType.Array;
            }
            else if (type.IsGenericType)
            {
                Type[] args = type.GetGenericArguments();
                if (args != null
                    && args.Length > 0)
                {
                    if (args.Length == 1)
                    {
                        // this is a list
                        return TransferType.List;
                    }
                    else if (args.Length == 2)
                    {
                        // this is a dictionary
                        return TransferType.Dictionary;
                    }
                }
            }
            return TransferType.Unknown;
        }

        private static int GetTypeSize(TransferType type)
        {
            if (!(IsGenericType(type)))
            {
                return 0;
            }

            switch (type)
            {
                default:
                case TransferType.Unknown:
                case TransferType.String:
                case TransferType.Object:
                case TransferType.Array:
                case TransferType.List:
                case TransferType.Dictionary:
                    return 0;
                case TransferType.Bool:
                case TransferType.Byte:
                    return (sizeof(byte));
                case TransferType.Double:
                    return (sizeof(double));
                case TransferType.Float:
                    return (sizeof(float));
                case TransferType.Int:
                    return (sizeof(int));
                case TransferType.Long:
                    return (sizeof(long));
                case TransferType.Short:
                    return (sizeof(short));
            }
        }
        #endregion

        #region Type Checking
        private static bool IsGenericType(TransferType type)
        {
            return (!(type == TransferType.Unknown
                || type == TransferType.String
                || type == TransferType.Object
                || type == TransferType.Array
                || type == TransferType.Dictionary
                || type == TransferType.List));
        }
        #endregion

        #region Serialization
        /// <summary>
        ///     <para>Encode an integer value in a given number of bytes.</para>
        /// </summary>
        /// <returns>The byte array containing the integer.</returns>
        /// <param name="value">The integer value to encode.</param>
        /// <param name="length">The length (number of bytes) of the resulting byte array.</param>
        internal static byte[] Encode(int value, int length)
        {
            byte[] bytes = new byte[length];
            for (int i = 0; i < length - 1; i++)
            {
                bytes[i] = (byte)(value >> ((length - (i + 1)) * 8));
            }
            bytes[length - 1] = (byte)(value & 0xff);

            return bytes;
        }

        /// <summary>
        ///    <para>Convert an object with a given <see cref="Astral.Net.TransferType"/> to a byte stream.</para>.
        /// </summary>
        /// <returns>The byte array containing the object.</returns>
        /// <param name="value">The object to convert.</param>
        /// <param name="type">The <see cref="Astral.Net.TransferType"/> of the object.</param>
        internal static byte[] ToBytes(object value, TransferType type)
        {
            List<byte> bytes = new List<byte>();
            switch (type)
            {
                case TransferType.Bool:
                    {
                        bytes.AddRange(BitConverter.GetBytes((bool)value));
                    }
                    break;
                case TransferType.Byte:
                    {
                        bytes.Add((byte)value);
                    }
                    break;
                case TransferType.Double:
                    {
                        bytes.AddRange(BitConverter.GetBytes((double)value));
                    }
                    break;
                case TransferType.Float:
                    {
                        bytes.AddRange(BitConverter.GetBytes((float)value));
                    }
                    break;
                case TransferType.Int:
                    {
                        bytes.AddRange(BitConverter.GetBytes((int)value));
                    }
                    break;
                case TransferType.Long:
                    {
                        bytes.AddRange(BitConverter.GetBytes((long)value));
                    }
                    break;
                case TransferType.Short:
                    {
                        bytes.AddRange(BitConverter.GetBytes((short)value));
                    }
                    break;
                case TransferType.String:
                    {
                        bytes.AddRange(Encoding.UTF8.GetBytes((string)value));
                    }
                    break;
                case TransferType.Object:
                    {
                        NetworkStreamInfo info = new NetworkStreamInfo();
                        ((ITransferable)value).GetStreamData(info);
                        bytes.AddRange(info.Serialize());
                    }
                    break;
                case TransferType.Array:
                    {
                        int dimensions = ((Array)value).Rank;
                        TransferType elementType = GetTransferType(value.GetType().GetElementType());

                        // add the dimensions in the beginning
                        byte[] dimensionBytes = Encode(dimensions, 4);
                        bytes.AddRange(dimensionBytes);

                        // now add the length of each dimension
                        for (int i = 0; i < dimensions; i++)
                        {
                            int length = ((Array)value).GetLength(i);

                            byte[] lengthBytes = Encode(length, 4);
                            bytes.AddRange(lengthBytes);
                        }

                        // now add the array elements (only use individual length when required)
                        bool requiresIndividualLength = !(IsGenericType(elementType));

                        IEnumerator elementEnumerator = ((Array)value).GetEnumerator();
                        while (elementEnumerator.MoveNext())
                        {
                            byte[] elementBytes = ToBytes(elementEnumerator.Current, elementType);

                            if (requiresIndividualLength)
                            {
                                byte[] objectLengthBytes = Encode(elementBytes.Length, 4);
                                bytes.AddRange(objectLengthBytes);
                            }

                            bytes.AddRange(elementBytes);
                        }
                    }
                    break;
                case TransferType.List:
                    {
                        TransferType elementType = GetTransferType(value.GetType().GetGenericArguments()[0]);

                        // we can add the elements right away (only use individual length when required)
                        bool requiresIndividualLength = !(IsGenericType(elementType));

                        IEnumerator elementEnumerator = ((IEnumerable)value).GetEnumerator();
                        while (elementEnumerator.MoveNext())
                        {
                            byte[] elementBytes = ToBytes(elementEnumerator.Current, elementType);

                            if (requiresIndividualLength)
                            {
                                byte[] objectLengthBytes = Encode(elementBytes.Length, 4);
                                bytes.AddRange(objectLengthBytes);
                            }
                            
                            bytes.AddRange(elementBytes);
                        }
                    }
                    break;
                case TransferType.Dictionary:
                    {
                        TransferType keyType = GetTransferType(value.GetType().GetGenericArguments()[0]);
                        TransferType valueType = GetTransferType(value.GetType().GetGenericArguments()[1]);

                        // we can add the elements right away (only use individual length when required)
                        bool keyRequiresIndividualLength = !(IsGenericType(keyType));
                        bool valueRequiresIndividualLength = !(IsGenericType(valueType));

                        IEnumerator keyValueEnumerator = ((IDictionary)value).Keys.GetEnumerator();
                        while (keyValueEnumerator.MoveNext())
                        {
                            object key = keyValueEnumerator.Current;

                            byte[] keyBytes = ToBytes(key, keyType);
                            byte[] valueBytes = ToBytes(((IDictionary)value)[key], valueType);

                            if (keyRequiresIndividualLength)
                            {
                                byte[] keyLengthBytes = Encode(keyBytes.Length, 4);
                                bytes.AddRange(keyLengthBytes);
                            }

                            if (valueRequiresIndividualLength)
                            {
                                byte[] valueLengthBytes = Encode(valueBytes.Length, 4);
                                bytes.AddRange(valueLengthBytes);
                            }

                            bytes.AddRange(keyBytes);
                            bytes.AddRange(valueBytes);
                        }
                    }
                    break;
                default:
                    break;
            }

            if (bytes == null)
            {
                throw new EncodingException("There was an error encoding "
                    + "the object of type '" + type + "'");
            }

            return bytes.ToArray();
        }
        #endregion

        #region Deserialization
        /// <summary>
        ///     <para>Decodes a byte out of a byte array (i.e., it takes the first byte).</para>
        /// </summary>
        /// <returns>The decoded byte.</returns>
        /// <param name="bytes">A byte array with a minimum of one byte.</param>
        internal static byte DecodeByte(byte[] bytes)
        {
            byte value = bytes[0];
            return value;
        }

        /// <summary>
        ///     <para>Decodes a byte out of a byte array (i.e., it takes the first two bytes).</para>
        /// </summary>
        /// <returns>The decoded short.</returns>
        /// <param name="bytes">A byte array with a minimum of two bytes.</param>
        internal static short DecodeShort(byte[] bytes)
        {
            int value = bytes[0] << 8;
            value |= bytes[1];

            return (short)value;
        }

        /// <summary>
        ///     <para>Decodes an integer out of a byte array (i.e., it takes the first four bytes).</para>
        /// </summary>
        /// <returns>The decoded short.</returns>
        /// <param name="bytes">A byte array with a minimum of four bytes.</param>
        internal static int DecodeInt(byte[] bytes)
        {
            int value = bytes[0] << 24;
            value |= (bytes[1] << 16);
            value |= (bytes[2] << 8);
            value |= bytes[3];

            return value;
        }

        /// <summary>
        ///     <para>Copies a sub array out of the given array.</para>
        /// </summary>
        /// <returns>A sub array (as deep copy) of the given length.</returns>
        /// <param name="source">The source array of bytes.</param>
        /// <param name="offset">The index of where the first element to be copied is in the source.</param>
        /// <param name="length">Describes how many elements shoulc be copied.</param>
        internal static byte[] GetBytes(byte[] source, int offset, int length)
        {
            byte[] bytes = new byte[length];
            Array.Copy(source, offset, bytes, 0, length);

            return bytes;
        }

        /// <summary>
        ///     <para>Rebuilds the object of a given <see cref="System.Type"/> from a number of bytes</para>
        /// </summary>
        /// <returns>The reconstructed object.</returns>
        /// <param name="bytes">The bytes containing the serialized object.</param>
        /// <param name="type">The <see cref="System.Type"/> of the object to be reconstructed.</param>
        internal static object FromBytes(byte[] bytes, Type type)
        {
            TransferType transferType = GetTransferType(type);
            object value = null;

            switch (transferType)
            {
                case TransferType.Bool:
                    {
                        value = BitConverter.ToBoolean(bytes, 0);
                    }
                    break;
                case TransferType.Byte:
                    {
                        value = bytes[0];
                    }
                    break;
                case TransferType.Double:
                    {
                        value = BitConverter.ToDouble(bytes, 0);
                    }
                    break;
                case TransferType.Float:
                    {
                        value = BitConverter.ToSingle(bytes, 0);
                    }
                    break;
                case TransferType.Int:
                    {
                        value = BitConverter.ToInt32(bytes, 0);
                    }
                    break;
                case TransferType.Long:
                    {
                        value = BitConverter.ToInt64(bytes, 0);
                    }
                    break;
                case TransferType.Short:
                    {
                        value = BitConverter.ToInt16(bytes, 0);
                    }
                    break;
                case TransferType.String:
                    {
                        value = Encoding.UTF8.GetString(bytes);
                    }
                    break;
                case TransferType.Object:
                    {
                        NetworkStreamInfo info = new NetworkStreamInfo();
                        info.Deserialize(bytes);

                        ConstructorInfo constructor = type.GetConstructor(
                            new Type[] { typeof(NetworkStreamInfo) });
                        value = constructor.Invoke(new object[] { info });
                    }
                    break;
                case TransferType.Array:
                    {
                        int arrayCounter = 0;

                        // first, check whether we need to read the length out as well
                        TransferType elementType = GetTransferType(type.GetElementType());
                        bool hasIndividualLength = !(IsGenericType(elementType));
                        int elementSize = GetTypeSize(elementType);

                        // get the dimensions
                        byte[] dimensionBytes = new byte[4];
                        Array.Copy(bytes, arrayCounter, dimensionBytes, 0, 4);
                        arrayCounter += 4;
                        int dimensions = DecodeInt(dimensionBytes);

                        // get the length of each dimension
                        object[] subLengths = new object[dimensions];
                        int[] currPositions = new int[dimensions];

                        for (int i = 0; i < dimensions; i++)
                        {
                            byte[] subLengthBytes = GetBytes(bytes, arrayCounter, 4);
                            arrayCounter += 4;

                            subLengths[i] = DecodeInt(subLengthBytes);
                            currPositions[i] = 0;
                        }

                        Type arrayType = null;
                        if (dimensions == 1)
                        {
                            arrayType = type.GetElementType().MakeArrayType();
                        }
                        else
                        {
                            arrayType = type.GetElementType().MakeArrayType(dimensions);
                        }
                        value = Activator.CreateInstance(arrayType, subLengths);

                        while (arrayCounter < bytes.Length)
                        {
                            int contentLength = elementSize;

                            if (hasIndividualLength)
                            {
                                byte[] contentLengthBytes = GetBytes(bytes, arrayCounter, 4);
                                arrayCounter += 4;

                                contentLength = DecodeInt(contentLengthBytes);
                            }

                            byte[] contentBytes = GetBytes(bytes, arrayCounter, contentLength);
                            arrayCounter += contentLength;

                            object element = FromBytes(contentBytes, type.GetElementType());
                            ((Array)value).SetValue(Convert.ChangeType(element, type.GetElementType()), currPositions);

                            for (int j = dimensions - 1; j >= 0; j--)
                            {
                                currPositions[j]++;
                                if (currPositions[j] == (int)subLengths[j])
                                {
                                    for (int k = j; k < dimensions; k++)
                                    {
                                        currPositions[k] = 0;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case TransferType.List:
                    {
                        value = type.GetConstructor(new Type[] { }).Invoke(new object[] { });

                        // first, check whether we need to read the length out as well
                        TransferType elementType = GetTransferType(type.GetGenericArguments()[0]);
                        bool hasIndividualLength = !(IsGenericType(elementType));
                        int elementSize = GetTypeSize(elementType);

                        int listCounter = 0;
                        while (listCounter < bytes.Length)
                        {
                            int contentLength = elementSize;

                            if (hasIndividualLength)
                            {
                                byte[] contentLengthBytes = GetBytes(bytes, listCounter, 4);
                                listCounter += 4;

                                contentLength = DecodeInt(contentLengthBytes);
                            }

                            byte[] contentBytes = GetBytes(bytes, listCounter, contentLength);
                            listCounter += contentLength;

                            object element = FromBytes(contentBytes, type.GetGenericArguments()[0]);
                            ((IList)value).Add(element);
                        }
                    }
                    break;
                case TransferType.Dictionary:
                    {
                        value = type.GetConstructor(new Type[] { }).Invoke(new object[] { });

                        // first, check whether we need to read the length out as well
                        TransferType keyType = GetTransferType(type.GetGenericArguments()[0]);
                        TransferType valueType = GetTransferType(type.GetGenericArguments()[1]);

                        bool hasIndividualKeyLength = !(IsGenericType(keyType));
                        bool hasIndividualValueLength = !(IsGenericType(valueType));
                        int keySize = GetTypeSize(keyType);
                        int valueSize = GetTypeSize(valueType);

                        int dictionaryCounter = 0;
                        while (dictionaryCounter < bytes.Length)
                        {
                            int keyLength = keySize;
                            if (hasIndividualKeyLength)
                            {
                                byte[] keyLengthBytes = GetBytes(bytes, dictionaryCounter, 4);
                                dictionaryCounter += 4;

                                keyLength = DecodeInt(keyLengthBytes);
                            }

                            int valueLength = valueSize;
                            if (hasIndividualValueLength)
                            {
                                byte[] valueLengthBytes = GetBytes(bytes, dictionaryCounter, 4);
                                dictionaryCounter += 4;

                                valueLength = DecodeInt(valueLengthBytes);
                            }

                            byte[] keyBytes = GetBytes(bytes, dictionaryCounter, keyLength);
                            dictionaryCounter += keyLength;

                            byte[] valueBytes = GetBytes(bytes, dictionaryCounter, valueLength);
                            dictionaryCounter += valueLength;

                            object key = FromBytes(keyBytes, type.GetGenericArguments()[0]);
                            object element = FromBytes(valueBytes, type.GetGenericArguments()[1]);

                            ((IDictionary)value).Add(key, element);
                        }
                    }
                    break;
                default:
                    break;
            }

            return value;
        }
        #endregion
    }
    #endregion
}
