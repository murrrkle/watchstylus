using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Astral.Content;
using Astral.Messaging;
using Astral.Net;
using Astral.Session;
using Astral.Session.Tasks;
using Astral.UI;



using Astral.Device;


namespace Astral
{
    #region Class 'AstralSession'
    public sealed class AstralSession
    {
        #region Class Members
        #region Control Class Members
        private AstralDevice m_device;

        private bool m_active;
        #endregion

        #region Task Class Members
        private ScreenCaptureTask m_captureTask;
        #endregion
        #endregion

        #region Events
        internal event AstralSessionEventHandler SessionInitialized;
        #endregion

        #region Constructors
        internal AstralSession(AstralDevice device)
        {
            m_device = device;
            m_active = false;

            // TODO: subscribe to MORE events
            m_device.DeviceInitialized += AstralDeviceInitialized;
        }
        #endregion

        #region Initialization
        private void InitializeTasks()
        {
            m_captureTask = new ScreenCaptureTask(this);
            m_captureTask.ProcessedScreenshotCaptured += ProcessedScreenshotCaptured;
        }
        #endregion

        #region Properties
        public AstralDevice Device
        {
            get { return m_device; }
        }

        public bool IsActive
        {
            get { return m_active; }
            private set { m_active = value; }
        }
        #endregion

        #region Region Handling
        public void SelectRegion()
        {
            m_captureTask.ShowSelectionWindow();
        }

        public void SetRegion(Rect newRegion)
        {

        }
        #endregion

        #region Shutdown
        public void Stop()
        {
            if (m_captureTask != null)
            {
                m_captureTask.ProcessedScreenshotCaptured -= ProcessedScreenshotCaptured;
                m_captureTask.Stop();
            }

            m_device.Stop();
        }
        #endregion

        #region Event Handler
        #region AstralDevice Event Handler
        private void AstralDeviceInitialized(object sender, AstralDevice device)
        {
            // remove the initialized event handler again (it's not needed)
            device.DeviceInitialized -= AstralDeviceInitialized;

            InitializeTasks();

            // now, we can actually inform the hosting application
            SessionInitialized?.Invoke(this, this);
        }
        #endregion

        #region Screenshot Event Handler
        private void ProcessedScreenshotCaptured(object sender, Screenshot screenshot)
        {
            if(m_device != null
                && m_device[ModuleType.Display] != null)
            {
                ((Display)m_device[ModuleType.Display]).UpdateContent(screenshot);
            }
        }
        #endregion
        #endregion
    }
    #endregion
}
