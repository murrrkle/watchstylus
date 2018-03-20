using System;
using System.Collections.Generic;
using System.Text;

using Astral.Net;

namespace Astral.Messaging
{
    #region Sealed Class 'ModuleStatusMessage'
    public sealed class ModuleStatusMessage : Message
    {
        #region Constant Class Members
        private const string MessageName = "AstralSet";

        private const string StatusDataField = "state";
        #endregion

        #region Constructors
        private ModuleStatusMessage()
            : base(MessageName)
        { }
        #endregion

        #region Static Creation
        public static Message CreateInstance(bool enableModule)
        {
            ModuleStatusMessage msg = new ModuleStatusMessage();
            msg.AddField(StatusDataField, enableModule);

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
                && msg.ContainsField(StatusDataField));
        }
        #endregion

        #region Conversion
        public static bool ToStatus(Message msg)
        {
            bool enableModule = msg.GetBoolField(StatusDataField);
            return enableModule;
        }
        #endregion
    }
    #endregion
}
