using System;
using System.Collections.Generic;
using System.Text;
using Astral.Device;
using Astral.Net;

namespace Astral.Messaging
{
    public sealed class OrientationDataMessage : Message
    {
        #region Constant Class Members
        private const string MessageName = "AstralOri";

        private const string OrientationDataField = "ori";
        #endregion

        #region Constructors
        private OrientationDataMessage()
            : base(MessageName)
        { }
        #endregion

        #region Static Creation
        public static Message CreateInstance(OrientationData orientationData)
        {
            OrientationDataMessage msg = new OrientationDataMessage();
            msg.AddField(OrientationDataField, orientationData);

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
                && msg.ContainsField(OrientationDataField));
        }
        #endregion

        #region Conversion
        public static OrientationData ToOrientationData(Message msg)
        {
            OrientationData orientationData = (OrientationData)msg.GetField(
                OrientationDataField, typeof(OrientationData));
            return orientationData;
        }
        #endregion
    }
}
