using System;

namespace Astral.Net.Serialization
{
    #region Enumeration 'TransferType'
    [Flags]
    public enum TransferType
    {
        /// <summary>
        /// Data type is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// A boolean (1 bit, transferred as 1 byte)
        /// </summary>
        Bool = 1,

        /// <summary>
        /// A byte (1 byte)
        /// </summary>
        Byte = 2,

        /// <summary>
        /// A double (8 bytes)
        /// </summary>
        Double = 3,

        /// <summary>
        /// A float, also known as single (4 bytes)
        /// </summary>
        Float = 4,

        /// <summary>
        /// An integer (4 bytes).
        /// </summary>
        Int = 5,

        /// <summary>
        /// A long integer (8 bytes)
        /// </summary>
        Long = 6,

        /// <summary>
        /// A short integer (2 bytes)
        /// </summary>
        Short = 7,

        /// <summary>
        /// A string (arbitrary number of bytes)
        /// </summary>
        String = 8,

        /// <summary>
        /// A custom object, which implements the <see cref="Astral.Net.ITransferable"/> interface.
        /// </summary>
        Object = 9,

        /// <summary>
        /// An array of generic valus (arbitrary number of bytes)
        /// </summary>
        Array = 10,

        /// <summary>
        /// A list of generic valus (arbitrary number of bytes)
        /// </summary>
        List = 11,

        /// <summary>
        /// A dictionary of generic valus (arbitrary number of bytes)
        /// </summary>
        Dictionary = 12
    }
    #endregion
}
