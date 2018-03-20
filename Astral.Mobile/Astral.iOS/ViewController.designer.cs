// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace Astral.iOS
{
    [Register ("ViewController")]
    partial class ViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        Astral.iOS.ScreenshotView ScreenshotContainer { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (ScreenshotContainer != null) {
                ScreenshotContainer.Dispose ();
                ScreenshotContainer = null;
            }
        }
    }
}