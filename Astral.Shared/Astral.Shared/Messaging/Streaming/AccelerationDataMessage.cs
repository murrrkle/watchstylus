using System;
using System.Collections.Generic;
using System.Text;

using Astral.Device;
using Astral.Net;

namespace Astral.Messaging
{
    #region Sealed Class 'AccelerationDataMessage'
    public sealed class AccelerationDataMessage : Message
    {
        #region Constant Class Members
        private const string MessageName = "AstralAcc";

        private const string AccelerationDataField = "acc";
        #endregion

        #region Constructors
        private AccelerationDataMessage()
            : base(MessageName)
        { }
        #endregion

        #region Static Creation
        public static Message CreateInstance(AccelerationData accelerationData)
        {
            AccelerationDataMessage msg = new AccelerationDataMessage();
            msg.AddField(AccelerationDataField, accelerationData);

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
                && msg.ContainsField(AccelerationDataField));
        }
        #endregion

        #region Conversion
        public static AccelerationData ToAccelerationData(Message msg)
        {
            AccelerationData accelerationData = (AccelerationData)msg.GetField(
                AccelerationDataField, typeof(AccelerationData));
            return accelerationData;
        }
        #endregion
    }
    #endregion
}
