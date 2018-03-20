using System;
using System.Collections.Generic;
using System.Text;

using Astral.Device;
using Astral.Net;

namespace Astral.Messaging
{
    public sealed class MagnetometerDataMessage : Message
    {
        #region Constant Class Members
        private const string MessageName = "AstralMag";

        private const string MagnetometerDataField = "mag";
        #endregion

        #region Constructors
        private MagnetometerDataMessage()
            : base(MessageName)
        { }
        #endregion

        #region Static Creation
        public static Message CreateInstance(MagnetometerData magnetometerData)
        {
            MagnetometerDataMessage msg = new MagnetometerDataMessage();
            msg.AddField(MagnetometerDataField, magnetometerData);

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
                && msg.ContainsField(MagnetometerDataField));
        }
        #endregion

        #region Conversion
        public static MagnetometerData ToMagnetometerData(Message msg)
        {
            MagnetometerData magnetometerData = (MagnetometerData)msg.GetField(
                MagnetometerDataField, typeof(MagnetometerData));
            return magnetometerData;
        }
        #endregion
    }
}
