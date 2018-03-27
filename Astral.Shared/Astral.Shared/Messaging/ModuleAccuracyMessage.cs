using System;
using System.Collections.Generic;
using System.Text;

using Astral.Net;

namespace Astral.Messaging
{
    #region Sealed Class 'ModuleAccuracyMessage'
    public sealed class ModuleAccuracyMessage : Message
    {
        #region Constant Class Members
        private const string MessageName = "AstralFid";

        private const string AccuracyDataField = "fid";
        #endregion

        #region Constructors
        private ModuleAccuracyMessage()
            : base(MessageName)
        { }
        #endregion

        #region Static Creation
        public static Message CreateInstance(double accuracy)
        {
            ModuleAccuracyMessage msg = new ModuleAccuracyMessage();
            msg.AddField(AccuracyDataField, accuracy);

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
                && msg.ContainsField(AccuracyDataField));
        }
        #endregion

        #region Conversion
        public static double ToAccuracy(Message msg)
        {
            double accuracy = msg.GetDoubleField(AccuracyDataField);
            return accuracy;
        }
        #endregion
    }
    #endregion
}
