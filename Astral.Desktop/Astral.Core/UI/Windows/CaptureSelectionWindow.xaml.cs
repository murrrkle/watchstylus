using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using Astral.Device;

namespace Astral.UI
{
    #region Partial Class 'CaptureSelectionWindow'
    internal partial class CaptureSelectionWindow : Window
    {
        #region Static Class Members
        private static readonly double HotCornerSize = 20.0;

        private static readonly double MinimumSize = 100.0;
        #endregion

        #region Class Members
        private Rect m_screenBounds;

        private Rect m_captureRegion;

        private Rect m_backgroundUpdatedRect;

        private Rect[] m_hotCorners;

        private double m_aspectRatio; // width / height

        private Display m_display;

        private DpiScale m_displayScale;

        private MouseOperation m_currMouseOperation;

        private ScaleDirection m_scaleDirection;

        private Point m_prevPosition;

        private bool m_buttonPressed;

        private object m_syncObj = new object();
        #endregion

        #region Constructors
        internal CaptureSelectionWindow(Rect captureRegion, Display display)
        {
            m_backgroundUpdatedRect = captureRegion;
            m_captureRegion = captureRegion;
            m_display = display;

            m_buttonPressed = false;

            InitializeComponent();

            ResolutionLabel.Content = m_display.Width + " x " + m_display.Height;

            m_aspectRatio = captureRegion.Width / captureRegion.Height;
            m_hotCorners = new Rect[4];
        }
        #endregion

        #region Initialization
        private void InitializeOverlay()
        {
            InvertedOverlay.Width = ActualWidth;
            InvertedOverlay.Height = ActualHeight;

            m_screenBounds = new Rect(0.0, 0.0, ActualWidth, ActualHeight);
            m_backgroundUpdatedRect.Location = new Point(
                (m_screenBounds.Width - m_backgroundUpdatedRect.Width) / 2.0,
                (m_screenBounds.Height - m_backgroundUpdatedRect.Height) / 2.0);

            Update();
        }
        #endregion

        #region Properties
        internal Rect CaptureRegion
        {
            get
            {
                lock (m_syncObj)
                {
                    Rect correctRegion = new Rect(
                        m_captureRegion.X * m_displayScale.DpiScaleX,
                        m_captureRegion.Y * m_displayScale.DpiScaleY,
                        m_captureRegion.Width * m_displayScale.DpiScaleX,
                        m_captureRegion.Height * m_displayScale.DpiScaleY);

                    return correctRegion;
                }
            }
        }

        internal DpiScale DisplayScale
        {
            get { return m_displayScale; }
        }
        #endregion

        #region Updates
        private void Update()
        {
            UpdateHotCorners();
            UpdateOverlay();
        }

        private void UpdateHotCorners()
        {
            m_hotCorners[0] = new Rect(m_backgroundUpdatedRect.TopLeft.X - HotCornerSize / 2.0,
                m_backgroundUpdatedRect.TopLeft.Y - HotCornerSize / 2.0, HotCornerSize, HotCornerSize);
            m_hotCorners[1] = new Rect(m_backgroundUpdatedRect.TopRight.X - HotCornerSize / 2.0,
                m_backgroundUpdatedRect.TopRight.Y - HotCornerSize / 2.0, HotCornerSize, HotCornerSize);
            m_hotCorners[2] = new Rect(m_backgroundUpdatedRect.BottomRight.X - HotCornerSize / 2.0,
                m_backgroundUpdatedRect.BottomRight.Y - HotCornerSize / 2.0, HotCornerSize, HotCornerSize);
            m_hotCorners[3] = new Rect(m_backgroundUpdatedRect.BottomLeft.X - HotCornerSize / 2.0,
                m_backgroundUpdatedRect.BottomLeft.Y - HotCornerSize / 2.0, HotCornerSize, HotCornerSize);
        }
        
        private void UpdateOverlay()
        {
            UpdateRegions();
            UpdateButtons();
        }

        private void UpdateRegions()
        {
            Geometry region = new RectangleGeometry(m_backgroundUpdatedRect);
            Geometry invert = new CombinedGeometry(GeometryCombineMode.Exclude,
                new RectangleGeometry(m_screenBounds), region);

            InvertedOverlay.Clip = invert;

            // TODO: fix the offset in m_cropRegion!
            double increase = 1.5;

            Canvas.SetLeft(MouseOverlay, m_backgroundUpdatedRect.Left - increase);
            Canvas.SetTop(MouseOverlay, m_backgroundUpdatedRect.Top - increase);
            MouseOverlay.Width = m_backgroundUpdatedRect.Width + 2.0 * increase;
            MouseOverlay.Height = m_backgroundUpdatedRect.Height + 2.0 * increase;

            GC.Collect();
        }

        private void UpdateButtons()
        {
            if (m_display != null)
            {
                switch (m_display.Orientation)
                {
                    default:
                    case DeviceOrientation.Unknown:
                        break;
                    case DeviceOrientation.Portrait:
                        ControlPanel.RenderTransform = new RotateTransform(0.0);
                        Canvas.SetLeft(ControlPanel, m_backgroundUpdatedRect.Left - 1.5);
                        Canvas.SetTop(ControlPanel, m_backgroundUpdatedRect.Top - (ControlPanel.ActualHeight + 5.5));
                        break;
                    case DeviceOrientation.PortraitUpsideDown:
                        ControlPanel.RenderTransform = new RotateTransform(0.0);
                        Canvas.SetLeft(ControlPanel, m_backgroundUpdatedRect.Right - (ControlPanel.ActualWidth - 1.5));
                        Canvas.SetTop(ControlPanel, m_backgroundUpdatedRect.Bottom + 5.5);
                        break;
                    case DeviceOrientation.LandscapeLeft:
                        ControlPanel.RenderTransform = new RotateTransform(90.0);
                        Canvas.SetLeft(ControlPanel, m_backgroundUpdatedRect.Right + (ControlPanel.ActualHeight + 5.5));
                        Canvas.SetTop(ControlPanel, m_backgroundUpdatedRect.Top - 1.5);
                        break;
                    case DeviceOrientation.LandscapeRight:
                        ControlPanel.RenderTransform = new RotateTransform(-90.0);
                        Canvas.SetLeft(ControlPanel, m_backgroundUpdatedRect.Left - (ControlPanel.ActualHeight + 5.5));
                        Canvas.SetTop(ControlPanel, m_backgroundUpdatedRect.Bottom + 1.5);
                        break;
                }
            }
        }
        #endregion
        
        #region Manipulation
        internal void Move(Vector offset)
        {
            m_backgroundUpdatedRect.Location += offset;
        }

        internal void Scale(Vector offset, ScaleDirection direction)
        {
            double minWidth = (m_aspectRatio < 1.0 ? MinimumSize : m_aspectRatio * MinimumSize);

            double newWidth = m_backgroundUpdatedRect.Width;
            switch (direction)
            {
                default:
                case ScaleDirection.None:
                    // don't do anything
                    break;
                case ScaleDirection.SW:
                case ScaleDirection.NW:
                    newWidth -= offset.X;
                    break;
                case ScaleDirection.NE:
                case ScaleDirection.SE:
                    newWidth += offset.X;
                    break;
            }

            newWidth = Math.Max(minWidth, newWidth);
            double newHeight = newWidth / m_aspectRatio;

            // adjust the width
            double widthChange = newWidth - m_backgroundUpdatedRect.Width;
            double heightChange = newHeight - m_backgroundUpdatedRect.Height;

            m_backgroundUpdatedRect.Width = newWidth;
            m_backgroundUpdatedRect.Height = newHeight;

            // do we need to adjust the location as well?
            switch (direction)
            {
                default:
                case ScaleDirection.None:
                    // don't do anything
                    break;
                case ScaleDirection.NW:
                    m_backgroundUpdatedRect.Location -= (new Vector(widthChange, heightChange));
                    break;
                case ScaleDirection.NE:
                    m_backgroundUpdatedRect.Location -= (new Vector(0.0, heightChange));
                    break;
                case ScaleDirection.SE:
                    // this will not do anything
                    m_backgroundUpdatedRect.Location -= (new Vector(0.0, 0.0));
                    break;
                case ScaleDirection.SW:
                    m_backgroundUpdatedRect.Location -= (new Vector(widthChange, 0.0));
                    break;
            }
        }
        #endregion

        #region Interaction
        internal Tuple<MouseOperation, ScaleDirection> GetMouseOperation(Point position)
        {
            MouseOperation operation = MouseOperation.None;
            ScaleDirection direction = ScaleDirection.None;

            int hotCornerIndex = -1;
            for (int i = 0; i < m_hotCorners.Length; i++)
            {
                if (m_hotCorners[i].Contains(position))
                {
                    hotCornerIndex = i;
                    break;
                }
            }

            if (hotCornerIndex != -1)
            {
                operation = MouseOperation.Scale;
                direction = (ScaleDirection)Enum.ToObject(
                    typeof(ScaleDirection), hotCornerIndex + 1);
            }
            else
            {
                operation = m_backgroundUpdatedRect.Contains(position)
                    ? MouseOperation.Move : MouseOperation.None;
                direction = ScaleDirection.None;
            }

            return new Tuple<MouseOperation, ScaleDirection>(
                operation, direction);
        }
        #endregion

        #region Event Handler
        #region Window Event Handler
        private void CaptureSelectionWindowLoaded(object sender, RoutedEventArgs e)
        {
            // get the display scale
            m_displayScale = VisualTreeHelper.GetDpi(this);
            
            ControlPanel.Visibility = Visibility.Visible;

            MouseOverlay.LayoutUpdated += OnMouseOverlayLayoutUpdated;
            InitializeOverlay();
        }
        #endregion

        #region UI Event Handler
        private void OnMouseOverlayLayoutUpdated(object sender, EventArgs e)
        {
            lock (m_syncObj)
            {
                double increase = 1.5;

                m_captureRegion.Width = MouseOverlay.RenderSize.Width - 2.0 * increase;
                m_captureRegion.Height = MouseOverlay.RenderSize.Height - 2.0 * increase;
                m_captureRegion.X = Canvas.GetLeft(MouseOverlay) + increase;
                m_captureRegion.Y = Canvas.GetTop(MouseOverlay) + increase;
            }
        }

        private void OrientationButtonClicked(object sender, RoutedEventArgs e)
        {
            int buttonPressed = Convert.ToInt32(((Button)sender).Tag);
            bool shouldUpdate = false;

            if (buttonPressed == 0)
            {
                // rotate left
                if (m_display != null)
                {
                    m_display.RotateLeft();
                    shouldUpdate = true;
                }
            }
            else if (buttonPressed == 1)
            {
                // rotate right
                if (m_display != null)
                {
                    m_display.RotateRight();
                    shouldUpdate = true;
                }
            }

            if (shouldUpdate)
            {
                // now, flip the width and height and update the top-left corner
                double newWidth = m_backgroundUpdatedRect.Height;
                double newHeight = m_backgroundUpdatedRect.Width;
                double newX = m_backgroundUpdatedRect.X
                    - (newWidth - m_backgroundUpdatedRect.Width) / 2.0;
                double newY = m_backgroundUpdatedRect.Y
                    - (newHeight - m_backgroundUpdatedRect.Height) / 2.0;

                m_backgroundUpdatedRect.X = newX;
                m_backgroundUpdatedRect.Y = newY;
                m_backgroundUpdatedRect.Width = newWidth;
                m_backgroundUpdatedRect.Height = newHeight;

                Update();
            }
        }

        private void DoneButtonClicked(object sender, RoutedEventArgs e)
        {
            Hide();
        }
        #endregion

        #region Key Event Handler
        private void WindowKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Hide();
            }
        }
        #endregion

        #region Mouse Event Handler
        private void MousePressed(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left
                && e.ButtonState == MouseButtonState.Pressed
                && !(m_buttonPressed))
            {
                m_buttonPressed = true;
                m_prevPosition = e.GetPosition(this);
            }
        }

        private void MouseMoved(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(this);

            if (!(m_buttonPressed))
            {
                // this should only occur when we're not dragging

                // first, get the new operation
                Tuple<MouseOperation, ScaleDirection> mouseOperation = GetMouseOperation(position);
                m_currMouseOperation = mouseOperation.Item1;
                m_scaleDirection = mouseOperation.Item2;

                switch (m_currMouseOperation)
                {
                    default:
                    case MouseOperation.None:
                        Cursor = Cursors.Arrow;
                        break;
                    case MouseOperation.Move:
                        Cursor = Cursors.SizeAll;
                        break;
                    case MouseOperation.Scale:
                        switch (m_scaleDirection)
                        {
                            default:
                            case ScaleDirection.None:
                                Cursor = Cursors.Arrow;
                                break;
                            case ScaleDirection.NW:
                            case ScaleDirection.SE:
                                Cursor = Cursors.SizeNWSE;
                                break;
                            case ScaleDirection.NE:
                            case ScaleDirection.SW:
                                Cursor = Cursors.SizeNESW;
                                break;
                        }
                        break;
                }
            }
            else
            {
                // we apply these transforms to the background rectangle
                Vector offset = position - m_prevPosition;
                switch (m_currMouseOperation)
                {
                    default:
                    case MouseOperation.None:
                        break;
                    case MouseOperation.Move:
                        Move(offset);
                        break;
                    case MouseOperation.Scale:
                        Scale(offset, m_scaleDirection);
                        break;
                }
                
                Update();
                m_prevPosition = position;
            }
        }

        private void WindowReleased(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left
                && e.ButtonState == MouseButtonState.Released
                && m_buttonPressed)
            {
                m_buttonPressed = false;
            }
        }
        #endregion
        #endregion
    }
    #endregion
}
