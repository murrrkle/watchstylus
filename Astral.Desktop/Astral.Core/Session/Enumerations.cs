using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astral.Session
{
    #region Enumeration 'CaptureTaskState'
    [Flags]
    internal enum CaptureTaskState
    {
        Idle = 1,
        Selecting = 2,
        Running = 3
    }
    #endregion
}
