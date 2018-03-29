using System;
using System.Collections.Generic;
using System.Text;

namespace Astral
{
    #region Enumeration 'ConnectivityType'
    [Flags]
    public enum ConnectivityType
    {
        ContinuousStream = 1,
        RequestResponse = 2
    }
    #endregion
}
