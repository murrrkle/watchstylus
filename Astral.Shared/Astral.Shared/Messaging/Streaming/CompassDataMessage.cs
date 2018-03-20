using System;
using System.Collections.Generic;
using System.Text;

using Astral.Device;
using Astral.Net;

namespace Astral.Messaging
{
    #region Sealed Class 'CompassDataMessage'
    public sealed class CompassDataMessage : Message
    {
        #region Constant Class Members
        private const string MessageName = "AstralCom";

        private const string CompassDataField = "com";
        #endregion

        #region Constructors
        private CompassDataMessage()
            : base(MessageName)
        { }
        #endregion

        #region Static Creation
        public static Message CreateInstance(CompassData compassData)
        {
            CompassDataMessage msg = new CompassDataMessage();
            msg.AddField(CompassDataField, compassData);

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
                && msg.ContainsField(CompassDataField));
        }
        #endregion

        #region Conversion
        public static CompassData ToCompassData(Message msg)
        {
            CompassData compassData = (CompassData)msg.GetField(
                CompassDataField, typeof(CompassData));
            return compassData;
        }
        #endregion
    }
    #endregion
}
