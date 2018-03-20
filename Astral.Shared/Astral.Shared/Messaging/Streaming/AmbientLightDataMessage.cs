using System;
using System.Collections.Generic;
using System.Text;

using Astral.Device;
using Astral.Net;

namespace Astral.Messaging
{
    public sealed class AmbientLightDataMessage : Message
    {
        #region Constant Class Members
        private const string MessageName = "AstralAmbLight";

        private const string AmbientLightDataField = "amb";
        #endregion

        #region Constructors
        private AmbientLightDataMessage()
            : base(MessageName)
        { }
        #endregion

        #region Static Creation
        public static Message CreateInstance(AmbientLightData ambientLightData)
        {
            AmbientLightDataMessage msg = new AmbientLightDataMessage();
            msg.AddField(AmbientLightDataField, ambientLightData);

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
                && msg.ContainsField(AmbientLightDataField));
        }
        #endregion

        #region Conversion
        public static AmbientLightData ToAmbientLightData(Message msg)
        {
            AmbientLightData ambientLigthData = (AmbientLightData)msg.GetField(
                AmbientLightDataField, typeof(AmbientLightData));
            return ambientLigthData;
        }
        #endregion
    }
}
