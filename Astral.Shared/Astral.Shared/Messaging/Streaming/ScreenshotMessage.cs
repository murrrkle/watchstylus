using System;
using System.Collections.Generic;
using System.Text;

using Astral.Content;
using Astral.Net;

namespace Astral.Messaging
{
    #region Sealed Class 'ScreenshotMessage'
    public sealed class ScreenshotMessage : Message
    {
        #region Constant Class Members
        private const string MessageName = "Scr";

        private const string ScreenshotField = "scr";
        #endregion

        #region Constructors
        private ScreenshotMessage()
            : base(MessageName)
        { }
        #endregion

        #region Static Creation
        public static Message CreateInstance(Screenshot screenshot)
        {
            ScreenshotMessage msg = new ScreenshotMessage();
            msg.AddField(ScreenshotField, screenshot);
            
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

            // TODO: add capabilities here!
            return (msg.ContainsField(ScreenshotField));
        }
        #endregion

        #region Conversion
        public static Screenshot ToScreenshot(Message msg)
        {
            return (Screenshot)msg.GetField(ScreenshotField, typeof(Screenshot));
        }
        #endregion
    }
    #endregion
}
