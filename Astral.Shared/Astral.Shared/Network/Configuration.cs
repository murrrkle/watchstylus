using System;
using System.Collections.Generic;
using System.Text;

namespace Astral
{
    #region Static Partial Class 'Configuration'
    public static partial class Configuration
    {
        #region Nested Static Class 'Network'
        public static class Network
        {
            #region Adjustable Members
            public static bool IsMultiThreaded = false;

            public static int Timeout = 1000;
            #endregion

            #region Fixed Members
            public const string AstralServerName = "Astral";

            internal const int StartPort = 10001;

            internal const int PortStepping = 2;
            #endregion
        }
        #endregion
    }
    #endregion
}
