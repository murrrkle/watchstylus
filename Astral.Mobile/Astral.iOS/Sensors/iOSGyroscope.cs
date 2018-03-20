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
    #region Sealed Class 'GyroscopeSensor'
    public sealed class iOSGyroscope : Gyroscope
    {
        #region Class Members
        private CMMotionManager m_motionManager;

        private bool m_running = false;
        #endregion

        #region Constructors
        public iOSGyroscope()
        {
            m_motionManager = new CMMotionManager();

            Activated += OnActivated;
            Deactivated += OnDeactivated;
        }
        #endregion

        #region Start/Stop
        private void OnActivated(object sender, EventArgs e)
        {
            if (!(m_running))
            {
                m_running = true;

                UIApplication.SharedApplication.InvokeOnMainThread(
                    delegate ()
                    {
                        m_motionManager.StartGyroUpdates(NSOperationQueue.CurrentQueue, (data, error) =>
                        {
                            double x = data.RotationRate.x;
                            double y = data.RotationRate.y;
                            double z = data.RotationRate.z;

                            GyroscopeData gyroData = new GyroscopeData(x, y, z);
                            UpdateGyroscopeData(gyroData);
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
                        m_motionManager.StopGyroUpdates();
                    });
            }
        }
        #endregion
    }
    #endregion
}