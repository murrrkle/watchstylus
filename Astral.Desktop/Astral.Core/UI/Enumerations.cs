using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astral.UI
{
    #region Enumeration 'MouseOperation'
    [Flags]
    internal enum MouseOperation
    {
        None = 1,
        Move = 2,
        Scale = 4
    }
    #endregion

    #region Enumeration 'ScaleDirection'
    [Flags]
    internal enum ScaleDirection
    {
        None = 0,
        NW = 1,
        NE = 2,
        SE = 3,
        SW = 4
    }
    #endregion

    #region Enumeration 'CaptureOrientation'
    [Flags]
    internal enum CaptureOrientation
    {
        Landscape = 0,
        Portrait = 1
    }
    #endregion

    #region Enumeration 'ClosingReason'
    [Flags]
    public enum ClosingReason
    {
        OK = 1,
        Cancel = 2
    }
    #endregion
}
