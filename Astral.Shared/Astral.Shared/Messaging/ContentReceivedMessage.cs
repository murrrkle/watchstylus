using System;
using System.Collections.Generic;
using System.Text;

using Astral.Net;

namespace Astral.Messaging
{
    #region Enumerations
    [Flags]
    public enum ContentType
    {
        Screenshot = 1
    }
    #endregion

    #region Sealed Class 'ContentReceivedMessage'
    public sealed class ContentReceivedMessage : Message
    {
        #region Constant Class Members
        private const string MessageName = "AstralAck";

        private const string ContentDataField = "con";
        #endregion

        #region Constructors
        private ContentReceivedMessage()
            : base(MessageName)
        { }
        #endregion

        #region Static Creation
        public static Message CreateInstance(ContentType type)
        {
            ContentReceivedMessage msg = new ContentReceivedMessage();
            msg.AddField(ContentDataField, (int)type);

            return msg;
        }
        #endregion

        #region Checking
        public static bool IsKindOf(Message msg)
        {
            if (msg == null
                || msg.Name != MessageName)
            {
                return false;
            }

            return (msg.Name == MessageName
                && msg.ContainsField(ContentDataField));
        }
        #endregion

        #region Conversion
        public static ContentType ToContentType(Message msg)
        {
            ContentType type = (ContentType)Enum.ToObject(
                typeof(ContentType), msg.GetIntField(ContentDataField));
            return type;
        }
        #endregion
    }
    #endregion
}
