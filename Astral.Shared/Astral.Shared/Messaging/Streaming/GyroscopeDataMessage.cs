using System;
using System.Collections.Generic;
using System.Text;

using Astral.Device;
using Astral.Net;

namespace Astral.Messaging
{
    #region Sealed Class 'GyroscopeDataMessage'
    public sealed class GyroscopeDataMessage : Message
    {
        #region Constant Class Members
        private const string MessageName = "AstralGyro";

        private const string GyroscopeDataField = "gyro";
        #endregion

        #region Constructors
        private GyroscopeDataMessage()
            : base(MessageName)
        { }
        #endregion

        #region Static Creation
        public static Message CreateInstance(GyroscopeData gyroscopeData)
        {
            GyroscopeDataMessage msg = new GyroscopeDataMessage();
            msg.AddField(GyroscopeDataField, gyroscopeData);

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
                && msg.ContainsField(GyroscopeDataField));
        }
        #endregion

        #region Conversion
        public static GyroscopeData ToRotationData(Message msg)
        {
            GyroscopeData gyroscopeData = (GyroscopeData)msg.GetField(
                GyroscopeDataField, typeof(GyroscopeData));
            return gyroscopeData;
        }
        #endregion
    }
    #endregion
}
