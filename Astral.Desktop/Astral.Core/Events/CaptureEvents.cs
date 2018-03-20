using System.Drawing;

using Astral.Content;
using Astral.Session;
using Astral.UI;

namespace Astral
{
    #region Delegates
    internal delegate void CaptureStateEventHandler(object sender, CaptureTaskState oldState, CaptureTaskState newState);

    public delegate void CaptureEventHandler(object sender, Bitmap screenshot);

    internal delegate void ProcessedCaptureEventHandler(object sender, Screenshot screenshot);

    internal delegate void CaptureOrientationEventHandler(object sender, CaptureOrientation oldOrientation, CaptureOrientation newOrientation);
    #endregion
}
