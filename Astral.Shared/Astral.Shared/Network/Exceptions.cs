using System;

namespace Astral.Net
{
    // TODO: add more info to this exception.

    #region Class 'ConfigurationException'
    /// <summary>
    ///     <para>A class handeling exceptions related to server configurations.</para>
    /// </summary>
    public class ConfigurationException : Exception
    {
        /// <summary>
        ///     <para>Initializes a new instance of the <see cref="T:Astral.Net.ConfigurationException"/> class.</para>
        /// </summary>
        internal ConfigurationException()
            : base()
        { }

        /// <summary>
        ///     <para>Initializes a new instance of the <see cref="T:Astral.Net.ConfigurationException"/> class.</para>
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        internal ConfigurationException(string message)
            : base(message)
        { }
    }
    #endregion

    #region Class 'DecodingException'
    /// <summary>
    ///     <para>A class handeling exceptions related to decodings.</para>
    /// </summary>
    public class DecodingException : Exception
    {
        /// <summary>
        ///     <para>Initializes a new instance of the <see cref="T:Astral.Net.DecodingException"/> class.</para>
        /// </summary>
        internal DecodingException()
            : base()
        { }

        /// <summary>
        ///     <para>Initializes a new instance of the <see cref="T:Astral.Net.DecodingException"/> class.</para>
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        internal DecodingException(string message)
            : base(message)
        { }
    }
    #endregion

    #region Class 'EncodingException'
    /// <summary>
    ///     <para>A class handeling exceptions related to encodings.</para>
    /// </summary>
    public class EncodingException : Exception
    {
        /// <summary>
        ///     <para>Initializes a new instance of the <see cref="T:Astral.Net.EncodingException"/> class.</para>
        /// </summary>
        internal EncodingException()
            : base()
        { }

        /// <summary>
        ///     <para>Initializes a new instance of the <see cref="T:Astral.Net.EncodingException"/> class.</para>
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        internal EncodingException(string message)
            : base(message)
        { }
    }
    #endregion
}
