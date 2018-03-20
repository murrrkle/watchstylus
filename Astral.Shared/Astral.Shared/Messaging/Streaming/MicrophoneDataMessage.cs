using System;
using System.Collections.Generic;
using System.Text;

using Astral.Device;
using Astral.Net;

namespace Astral.Messaging
{
    #region Sealed Class 'MicrophoneDataMessage'
    public sealed class MicrophoneDataMessage : Message
    {
        #region Constant Class Members
        private const string MessageName = "AstralMic";

        private const string MicrophoneDataField = "mic";
        #endregion

        #region Constructors
        private MicrophoneDataMessage()
            : base(MessageName)
        { }
        #endregion

        #region Static Creation
        public static Message CreateInstance(MicrophoneData microphoneData)
        {
            MicrophoneDataMessage msg = new MicrophoneDataMessage();
            msg.AddField(MicrophoneDataField, microphoneData);

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
                && msg.ContainsField(MicrophoneDataField));
        }
        #endregion

        #region Conversion
        public static MicrophoneData ToMicrophoneData(Message msg)
        {
            MicrophoneData microphoneData = (MicrophoneData)msg.GetField(
                MicrophoneDataField, typeof(MicrophoneData));
            return microphoneData;
        }
        #endregion
    }
    #endregion
}
