using System;
using System.Collections.Generic;
using System.Text;

using Astral.Device;
using Astral.Net;

namespace Astral.Messaging
{
    #region Sealed Class 'TouchPointMessage'
    public sealed class TouchPointMessage : Message
    {
        #region Constant Class Members
        private const string MessageName = "AstralTouch";

        private const string TouchPointField = "touch";
        #endregion

        #region Constructors
        private TouchPointMessage()
            : base(MessageName)
        { }
        #endregion

        #region Static Creation
        public static Message CreateInstance(TouchPoint touchPoint)
        {
            TouchPointMessage msg = new TouchPointMessage();
            msg.AddField(TouchPointField, touchPoint);

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
                && msg.ContainsField(TouchPointField));
        }
        #endregion

        #region Conversion
        public static TouchPoint ToTouchPoint(Message msg)
        {
            TouchPoint touchPoint = (TouchPoint)msg.GetField(
                TouchPointField, typeof(TouchPoint));
            return touchPoint;
        }
        #endregion
    }
    #endregion
}
