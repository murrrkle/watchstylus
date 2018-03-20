using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using CoreLocation;

using Astral.Device;

namespace Astral.iOS.Sensors
{
    #region Sealed Class 'iOSCompass'
    public sealed class iOSCompass : Compass
    {
        #region Class Members
        private CLLocationManager m_locationManager;

        private bool m_running = false;
        #endregion

        #region Constructors
        public iOSCompass()
        {
            m_locationManager = new CLLocationManager();

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
                        m_locationManager.UpdatedHeading += OnLocationManagerUpdatedHeading;
                        m_locationManager.StartUpdatingHeading();
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
                        m_locationManager.UpdatedHeading -= OnLocationManagerUpdatedHeading;
                        m_locationManager.StopUpdatingHeading();
                    });
            }
        }
        #endregion

        #region Event Handler
        private void OnLocationManagerUpdatedHeading(object sender, CLHeadingUpdatedEventArgs e)
        {
            double hdg = e.NewHeading.MagneticHeading;

            CompassData compassData = new CompassData(hdg);
            UpdateCompassData(compassData);
        }
        #endregion
    }
    #endregion
}