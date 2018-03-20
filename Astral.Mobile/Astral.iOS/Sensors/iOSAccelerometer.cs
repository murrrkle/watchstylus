using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using CoreMotion;

using Astral.Device;

namespace Astral.iOS.Sensors
{
    #region Sealed Class 'iOSAccelerometer'
    public sealed class iOSAccelerometer : Accelerometer
    {
        #region Class Members
        private CMMotionManager m_motionManager;

        private bool m_running = false;
        #endregion

        #region Constructors
        public iOSAccelerometer()
        {
            m_motionManager = new CMMotionManager();

            Activated += OnActivated;
            Deactivated += OnDeactivated;
        }
        #endregion

        #region Activate/Deactivate
        private void OnActivated(object sender, EventArgs e)
        {
            if (!(m_running))
            {
                m_running = true;

                UIApplication.SharedApplication.InvokeOnMainThread(
                    delegate()
                    {
                        m_motionManager.StartAccelerometerUpdates(NSOperationQueue.CurrentQueue, (data, error) =>
                        {
                            double x = data.Acceleration.X;
                            double y = data.Acceleration.Y;
                            double z = data.Acceleration.Z;

                            AccelerationData accData = new AccelerationData(x, y, z);
                            UpdateAccelerationData(accData);
                        });
                    });
            }
        }

        private void OnDeactivated(object sender, EventArgs e)
        {
            if (m_running)
            {
                m_running = false;

                UIApplication.SharedApplication.InvokeOnMainThread(
                    delegate ()
                    {
                        m_motionManager.StopAccelerometerUpdates();
                    });
            }
        }
        #endregion
    }
    #endregion
}