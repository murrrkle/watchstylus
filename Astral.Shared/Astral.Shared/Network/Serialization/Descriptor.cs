
namespace Astral.Net.Serialization
{
    // TODO: add documentation

    #region Class 'Descriptor'
    internal class Descriptor
    {
        #region Class Members
        private string m_name;

        private TransferType m_type;

        private bool m_null;
        #endregion

        #region Constructors
        internal Descriptor(string name, TransferType type)
            : this(name, type, false)
        { }

        internal Descriptor(string name,
            TransferType type, bool isNull)
        {
            m_name = name;
            m_type = type;
            m_null = isNull;
        }
        #endregion

        #region Properties
        internal string Name
        {
            get { return m_name; }
        }

        internal TransferType Type
        {
            get { return m_type; }
        }

        internal bool IsNull
        {
            get { return m_null; }
        }
        #endregion

        #region Overrides (Object)
        public override int GetHashCode()
        {
            return 100;
        }

        public override bool Equals(object obj)
        {
            if (obj is Descriptor)
            {
                Descriptor descriptor = obj as Descriptor;
                if (descriptor.Type == TransferType.Unknown
                    || Type == TransferType.Unknown)
                {
                    return (descriptor.Name != null
                        && descriptor.Name.Equals(Name));
                }
                else
                {
                    return (descriptor.Name != null
                        && descriptor.Name.Equals(Name)
                        && descriptor.Type == Type);
                }
            }

            return false;
        }
        #endregion
    }
    #endregion
}
