using System;
using System.Collections.Generic;
using System.Text;

namespace Astral.Device
{
    #region Enumeration 'ModuleType'
    [Flags]
    public enum ModuleType
    {
        Display = 1,
        Accelerometer = 2,
        Gyroscope = 3,
        Compass = 4,
        Magnetometer = 5,
        Orientation = 6,
        AmbientLight = 7,
        Microphone = 8
        
        // TODO: MORE to come
    }
    #endregion

    #region Enumeration 'TouchCapabilities'
    [Flags]
    public enum TouchCapabilities
    {
        None = 1,
        Single = 2,
        Multi = 3
    }
    #endregion

    #region Enumeration 'TouchState'
    [Flags]
    public enum TouchState
    {
        None = 1,
        Began = 2,
        Moved = 3,
        Ended = 4
    }
    #endregion
}
