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
    #region Sealed Class 'AstralSession'
    public sealed class AstralSession
    {
        #region Class Members
        #region Control Class Members
        private AstralDevice m_device;

        private bool m_active;
        #endregion

        #region Task Class Members
        private ScreenCaptureTask m_captureTask;

        private InputSelectionWindow m_inputSelectionWindow;
        #endregion
        #endregion

        #region Events
        internal event AstralSessionEventHandler SessionInitialized;

        public event SelectionWindowEventHandler CaptureSelectionWindowClosed;

        public event SelectionWindowEventHandler InputSelectionWindowClosed;
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
            m_captureTask.SelectionWindowClosed += OnCaptureSelectionWindowClosed;

            Application.Current.Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        m_inputSelectionWindow = new InputSelectionWindow(
                            m_captureTask.InitialCaptureRegion, m_device[ModuleType.Display] as Display);
                        m_inputSelectionWindow.SelectionWindowClosed += OnInputSelectionWindowClosed;
                    }));
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

        public Rect CaptureRegion
        {
            get
            {
                Rect captureRegion = Rect.Empty;
                if (m_captureTask != null)
                {
                    Application.Current.Dispatcher.Invoke(
                       new Action(
                           delegate ()
                           {
                               captureRegion = m_captureTask.CaptureRegion;
                           }));
                }

                return captureRegion;
            }
        }

        public Rect InputRegion
        {
            get
            {
                Rect inputRegion = Rect.Empty;
                if (m_inputSelectionWindow != null)
                {
                    Application.Current.Dispatcher.Invoke(
                       new Action(
                           delegate ()
                           {
                               inputRegion = m_inputSelectionWindow.InputRegion;
                           }));
                }

                return inputRegion;
            }
        }

        public double FramesPerSecond
        {
            get
            {
                if (m_captureTask != null)
                {
                    return m_captureTask.FramesPerSecond;
                }
                return -1.0;
            }
        }
        #endregion

        #region Region Handling
        #region Capture Region Handling
        public void ShowCaptureSelectionWindow()
        {
            m_captureTask.ShowSelectionWindow();
        }

        public void SetCaptureRegionWidth(int newWidth)
        {
            m_captureTask.SetCaptureWidth(newWidth);
        }

        public void SetCaptureRegionHeight(int newHeight)
        {
            m_captureTask.SetCaptureHeight(newHeight);
        }

        public void SetCaptureRegionLocation(Point newLocation)
        {
            m_captureTask.SetCaptureLocation(newLocation);
        }

        public void SetCaptureRegionX(int newX)
        {
            m_captureTask.SetCaptureX(newX);
        }

        public void SetCaptureRegionY(int newY)
        {
            m_captureTask.SetCaptureY(newY);
        }
        #endregion

        #region Input Region Handling
        public void ShowInputSelectionWindow()
        {
            Application.Current.Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        m_inputSelectionWindow.Show();
                    }));
        }

        public void SetInputRegion(Rect newInputRegion)
        {
            Application.Current.Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        m_inputSelectionWindow.SetInputRegion(newInputRegion);
                    }));
        }

        public void SetInputRegionLocation(Size newSize)
        {
            Application.Current.Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        m_inputSelectionWindow.SetInputRegionSize(newSize);
                    }));
        }

        public void SetInputRegionWidth(int newWidth)
        {
            Application.Current.Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        m_inputSelectionWindow.SetInputRegionWidth(newWidth);
                    }));
        }

        public void SetInputRegionheight(int newHeight)
        {
            Application.Current.Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        m_inputSelectionWindow.SetInputRegionheight(newHeight);
                    }));
        }

        public void SetInputRegionLocation(Point newLocation)
        {
            Application.Current.Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        m_inputSelectionWindow.SetInputRegionLocation(newLocation);
                    }));
        }

        public void SetInputRegionX(int newX)
        {
            Application.Current.Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        m_inputSelectionWindow.SetInputRegionX(newX);
                    }));
        }

        public void SetInputRegionY(int newY)
        {
            Application.Current.Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        m_inputSelectionWindow.SetInputRegionY(newY);
                    }));
        }
        #endregion
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
        #region ISelectionWindow Event Handler
        private void OnCaptureSelectionWindowClosed(object sender, SelectionWindowEventArgs e)
        {
            CaptureSelectionWindowClosed?.Invoke(this, e);
        }

        private void OnInputSelectionWindowClosed(object sender, SelectionWindowEventArgs e)
        {
            InputSelectionWindowClosed?.Invoke(this, e);
        }
        #endregion

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
