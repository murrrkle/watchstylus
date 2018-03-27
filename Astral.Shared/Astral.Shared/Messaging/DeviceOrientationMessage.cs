using System;
using System.Collections.Generic;
using System.Text;

using Astral.Net;

namespace Astral.Messaging
{
    #region Sealed Class 'DeviceOrientationMessage'
    public sealed class DeviceOrientationMessage : Message
    {
        #region Constant Class Members
        private const string MessageName = "AstralOri";

        private const string OrientationField = "ori";
        #endregion

        #region Constructors
        private DeviceOrientationMessage()
            : base(MessageName)
        { }
        #endregion

        #region Static Creation
        public static Message CreateInstance(DeviceOrientation orientation)
        {
            DeviceOrientationMessage msg = new DeviceOrientationMessage();
            msg.AddField(OrientationField, (int)orientation);

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
                && msg.ContainsField(OrientationField));
        }
        #endregion

        #region Conversion
        public static DeviceOrientation ToOrientation(Message msg)
        {
            DeviceOrientation orientation = (DeviceOrientation)Enum.ToObject(
                typeof(DeviceOrientation), msg.GetIntField(OrientationField));
            return orientation;
        }
        #endregion
    }
    #endregion
}
