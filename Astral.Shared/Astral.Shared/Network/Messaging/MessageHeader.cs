using System;

namespace Astral.Net
{
    // TODO: finish documentation

    #region Class 'MessageHeader'
    internal class MessageHeader
    {
        #region Class Members
        private int m_contentLength;

        private bool m_internal;

        private string m_name;
        #endregion

        #region Constructors
        internal MessageHeader(int contentLength, bool isInternal, string name)
        {
            m_contentLength = contentLength;
            m_internal = isInternal;
            m_name = name;
        }
        #endregion

        #region Properties
        internal int ContentLength
        {
            get { return m_contentLength; }
        }

        internal bool IsInternal
        {
            get { return m_internal; }
        }

        internal string Name
        {
            get { return m_name; }
        }

        internal bool IsValid
        {
            get { return m_contentLength >= 0; }
        }
        #endregion
    }
    #endregion
}
