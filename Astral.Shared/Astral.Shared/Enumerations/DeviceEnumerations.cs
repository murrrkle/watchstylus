using System;
using System.Collections.Generic;
using System.Text;

namespace Astral
{
    #region Enumeration 'DeviceOrientation'
    [Flags]
    public enum DeviceOrientation
    {
        Unknown = 0,
        Portrait = 1,
        PortraitUpsideDown = 2,
        LandscapeLeft = 3,
        LandscapeRight = 4
    }
    #endregion
}
