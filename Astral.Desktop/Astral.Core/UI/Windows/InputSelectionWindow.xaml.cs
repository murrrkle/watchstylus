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
    #region Partial Class 'InputSelectionWindow'
    internal partial class InputSelectionWindow : Window
    {
        #region Static Class Members
        private static readonly double HotCornerSize = 20.0;

        private static readonly double MinimumSize = 40.0;
        #endregion

        #region Class Members
        private Rect m_screenBounds;

        private Rect m_inputRegion;

        private Rect m_backgroundUpdatedRect;

        private Rect[] m_hotCorners;

        private Display m_display;

        private MouseOperation m_currMouseOperation;

        private ScaleDirection m_scaleDirection;

        private Point m_prevPosition;

        private bool m_buttonPressed;

        private bool m_initialized = false;

        private object m_syncObj = new object();
        #endregion

        #region Constructors
        internal InputSelectionWindow(Rect captureRegion, Display display)
        {
            m_backgroundUpdatedRect = captureRegion;
            m_inputRegion = captureRegion;
            m_display = display;

            m_buttonPressed = false;

            InitializeComponent();

            ResolutionLabel.Content = m_display.Width + " x " + m_display.Height;
            m_hotCorners = new Rect[4];
        }
        #endregion

        #region Initialization
        private void InitializeOverlay()
        {
            InvertedOverlay.Width = ActualWidth;
            InvertedOverlay.Height = ActualHeight;

            m_screenBounds = new Rect(0.0, 0.0, ActualWidth, ActualHeight);
            if (!(m_initialized))
            {
                m_backgroundUpdatedRect.Location = new Point(
                    (m_screenBounds.Width - m_backgroundUpdatedRect.Width) / 2.0,
                    (m_screenBounds.Height - m_backgroundUpdatedRect.Height) / 2.0);
            }

            Update();
        }
        #endregion

        #region Properties
        internal Rect InputRegion
        {
            get
            {
                lock (m_syncObj)
                {
                    Rect correctRegion = new Rect(
                        m_inputRegion.X * AstralService.DisplayScale.DpiScaleX,
                        m_inputRegion.Y * AstralService.DisplayScale.DpiScaleY,
                        m_inputRegion.Width * AstralService.DisplayScale.DpiScaleX,
                        m_inputRegion.Height * AstralService.DisplayScale.DpiScaleY);

                    return correctRegion;
                }
            }
        }
        #endregion

        #region Manual Adjustments
        internal void SetInputRegion(Rect inputRegion)
        {
            double newWidth = Math.Max(inputRegion.Width, MinimumSize);
            double newHeight = Math.Max(inputRegion.Height, MinimumSize);

            m_backgroundUpdatedRect.X = inputRegion.X;
            m_backgroundUpdatedRect.Y = inputRegion.Y;
            m_backgroundUpdatedRect.Width = newWidth;
            m_backgroundUpdatedRect.Height = newHeight;

            m_initialized = true;

            Update();
        }

        public void SetInputRegionSize(Size size)
        {
            double newWidth = Math.Max(size.Width, MinimumSize);
            double newHeight = Math.Max(size.Height, MinimumSize);

            m_backgroundUpdatedRect.Size = size;
            m_initialized = true;

            Update();
        }

        public void SetInputRegionWidth(int width)
        {
            m_backgroundUpdatedRect.Width = Math.Max(width, MinimumSize);
            m_initialized = true;

            Update();
        }

        public void SetInputRegionheight(int height)
        {
            m_backgroundUpdatedRect.Height = Math.Max(height, MinimumSize);
            m_initialized = true;

            Update();
        }

        public void SetInputRegionLocation(Point location)
        {
            m_backgroundUpdatedRect.Location = location;
            m_initialized = true;

            Update();
        }

        public void SetInputRegionX(int x)
        {
            m_backgroundUpdatedRect.X = x;
            m_initialized = true;

            Update();
        }

        public void SetInputRegionY(int y)
        {
            m_backgroundUpdatedRect.Y = y;
            m_initialized = true;

            Update();
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
            ControlPanel.RenderTransform = new RotateTransform(0.0);
            Canvas.SetLeft(ControlPanel, m_backgroundUpdatedRect.Left - 1.5);
            Canvas.SetTop(ControlPanel, m_backgroundUpdatedRect.Top - (ControlPanel.ActualHeight + 5.5));
        }
        #endregion
        
        #region Manipulation
        internal void Move(Vector offset)
        {
            m_backgroundUpdatedRect.Location += offset;
        }

        internal void Scale(Vector offset, ScaleDirection direction)
        {
            double newWidth = m_backgroundUpdatedRect.Width;
            double newHeight = m_backgroundUpdatedRect.Height;

            switch (direction)
            {
                default:
                case ScaleDirection.None:
                    // don't do anything
                    break;
                case ScaleDirection.SW:
                    newWidth -= offset.X;
                    newHeight += offset.Y;
                    break;
                case ScaleDirection.NW:
                    newWidth -= offset.X;
                    newHeight -= offset.Y;
                    break;
                case ScaleDirection.NE:
                    newWidth += offset.X;
                    newHeight -= offset.Y;
                    break;
                case ScaleDirection.SE:
                    newWidth += offset.X;
                    newHeight += offset.Y;
                    break;
            }

            newWidth = Math.Max(MinimumSize, newWidth);
            newHeight = Math.Max(MinimumSize, newHeight);

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

                m_inputRegion.Width = MouseOverlay.RenderSize.Width - 2.0 * increase;
                m_inputRegion.Height = MouseOverlay.RenderSize.Height - 2.0 * increase;
                m_inputRegion.X = Canvas.GetLeft(MouseOverlay) + increase;
                m_inputRegion.Y = Canvas.GetTop(MouseOverlay) + increase;

                // let's update the text
                ResolutionLabel.Content = Math.Round(m_inputRegion.Width) 
                    + " x " + Math.Round(m_inputRegion.Height);
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
