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
    internal partial class InputSelectionWindow : Window, ISelectionWindow
    {
        #region Static Class Members
        protected static readonly double HotCornerSize = 20.0;

        protected static readonly double MinimumSize = 40.0;
        #endregion

        #region Class Members
        private Rect m_screenBounds;

        private Rect m_inputRegion;

        private Rect m_lastInputRegion;

        private Rect[] m_hotCorners;

        private Rect[] m_hotEdges;

        private Display m_display;

        private MouseOperation m_currMouseOperation;

        private ScaleDirection m_scaleDirection;

        private Point m_prevPosition;

        private bool m_buttonPressed;

        private bool m_initialized = false;

        private object m_syncObj = new object();
        #endregion

        #region Events
        public event SelectionWindowEventHandler SelectionWindowClosed;
        #endregion

        #region Constructors
        internal InputSelectionWindow(Rect captureRegion, Display display)
        {
            m_inputRegion = captureRegion;
            m_lastInputRegion = m_inputRegion;

            m_display = display;

            m_buttonPressed = false;

            InitializeComponent();

            ResolutionLabel.Content = m_display.Width + " x " + m_display.Height;

            m_hotCorners = new Rect[4];
            m_hotEdges = new Rect[4];
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
                m_inputRegion.Location = new Point(
                    (m_screenBounds.Width - m_inputRegion.Width) / 2.0,
                    (m_screenBounds.Height - m_inputRegion.Height) / 2.0);
                m_lastInputRegion = m_inputRegion;
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

            m_inputRegion.X = inputRegion.X;
            m_inputRegion.Y = inputRegion.Y;
            m_inputRegion.Width = newWidth;
            m_inputRegion.Height = newHeight;

            // update it here right away, because it cannot be cancelled
            m_lastInputRegion = m_inputRegion;

            m_initialized = true;

            Update();
        }

        public void SetInputRegionSize(Size size)
        {
            double newWidth = Math.Max(size.Width, MinimumSize);
            double newHeight = Math.Max(size.Height, MinimumSize);

            m_inputRegion.Size = size;

            // update it here right away, because it cannot be cancelled
            m_lastInputRegion = m_inputRegion;
            m_initialized = true;

            Update();
        }

        public void SetInputRegionWidth(int width)
        {
            m_inputRegion.Width = Math.Max(width, MinimumSize);

            // update it here right away, because it cannot be cancelled
            m_lastInputRegion = m_inputRegion;
            m_initialized = true;

            Update();
        }

        public void SetInputRegionheight(int height)
        {
            m_inputRegion.Height = Math.Max(height, MinimumSize);

            // update it here right away, because it cannot be cancelled
            m_lastInputRegion = m_inputRegion;
            m_initialized = true;

            Update();
        }

        public void SetInputRegionLocation(Point location)
        {
            m_inputRegion.Location = location;

            // update it here right away, because it cannot be cancelled
            m_lastInputRegion = m_inputRegion;
            m_initialized = true;

            Update();
        }

        public void SetInputRegionX(int x)
        {
            m_inputRegion.X = x;

            // update it here right away, because it cannot be cancelled
            m_lastInputRegion = m_inputRegion;
            m_initialized = true;

            Update();
        }

        public void SetInputRegionY(int y)
        {
            m_inputRegion.Y = y;

            // update it here right away, because it cannot be cancelled
            m_lastInputRegion = m_inputRegion;
            m_initialized = true;

            Update();
        }
        #endregion

        #region Updates
        private void Update()
        {
            UpdateHotCornersAndEdges();
            UpdateOverlay();

            Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        ResolutionLabel.Content = ((int)Math.Round(m_inputRegion.Width))
                            + " x " + ((int)Math.Round(m_inputRegion.Height));
                    }));
        }

        private void UpdateHotCornersAndEdges()
        {
            // top-left
            m_hotCorners[0] = new Rect(m_inputRegion.TopLeft.X - HotCornerSize / 2.0,
                m_inputRegion.TopLeft.Y - HotCornerSize / 2.0, HotCornerSize, HotCornerSize);
            // top-right
            m_hotCorners[1] = new Rect(m_inputRegion.TopRight.X - HotCornerSize / 2.0,
                m_inputRegion.TopRight.Y - HotCornerSize / 2.0, HotCornerSize, HotCornerSize);
            // bottom-right
            m_hotCorners[2] = new Rect(m_inputRegion.BottomRight.X - HotCornerSize / 2.0,
                m_inputRegion.BottomRight.Y - HotCornerSize / 2.0, HotCornerSize, HotCornerSize);
            // bottom-left
            m_hotCorners[3] = new Rect(m_inputRegion.BottomLeft.X - HotCornerSize / 2.0,
                m_inputRegion.BottomLeft.Y - HotCornerSize / 2.0, HotCornerSize, HotCornerSize);

            // top
            m_hotEdges[0] = new Rect(m_inputRegion.TopLeft.X + HotCornerSize / 2.0,
                m_inputRegion.TopLeft.Y - HotCornerSize / 2.0, 
                m_inputRegion.Width - HotCornerSize, HotCornerSize);
            // right
            m_hotEdges[1] = new Rect(m_inputRegion.TopRight.X - HotCornerSize / 2.0,
                m_inputRegion.TopRight.Y + HotCornerSize / 2.0,
                HotCornerSize, m_inputRegion.Height - HotCornerSize);
            // bottom
            m_hotEdges[2] = new Rect(m_inputRegion.BottomLeft.X + HotCornerSize / 2.0,
                m_inputRegion.BottomLeft.Y - HotCornerSize / 2.0,
                m_inputRegion.Width - HotCornerSize, HotCornerSize);
            // left
            m_hotEdges[3] = new Rect(m_inputRegion.TopLeft.X - HotCornerSize / 2.0,
                m_inputRegion.TopLeft.Y + HotCornerSize / 2.0,
                HotCornerSize, m_inputRegion.Height - HotCornerSize);
        }

        private void UpdateOverlay()
        {
            UpdateRegions();
            UpdateButtons();
        }

        private void UpdateRegions()
        {
            Geometry region = new RectangleGeometry(m_inputRegion);
            Geometry invert = new CombinedGeometry(GeometryCombineMode.Exclude,
                new RectangleGeometry(m_screenBounds), region);

            InvertedOverlay.Clip = invert;

            double increase = 1.5;

            Canvas.SetLeft(MouseOverlay, m_inputRegion.Left - increase);
            Canvas.SetTop(MouseOverlay, m_inputRegion.Top - increase);
            MouseOverlay.Width = m_inputRegion.Width + 2.0 * increase;
            MouseOverlay.Height = m_inputRegion.Height + 2.0 * increase;

            GC.Collect();
        }

        private void UpdateButtons()
        {
            ControlPanel.RenderTransform = new RotateTransform(0.0);
            Canvas.SetLeft(ControlPanel, m_inputRegion.Left - 1.5);
            Canvas.SetTop(ControlPanel, m_inputRegion.Top - (ControlPanel.ActualHeight + 5.5));
        }
        #endregion

        #region Manipulation
        internal void Move(Vector offset)
        {
            m_inputRegion.Location += offset;
        }

        internal void Scale(Vector offset, ScaleDirection direction)
        {
            double newWidth = m_inputRegion.Width;
            double newHeight = m_inputRegion.Height;

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
                case ScaleDirection.N:
                    newHeight -= offset.Y;
                    break;
                case ScaleDirection.W:
                    newWidth -= offset.X;
                    break;
                case ScaleDirection.S:
                    newHeight += offset.Y;
                    break;
                case ScaleDirection.E:
                    newWidth += offset.X;
                    break;
            }

            newWidth = Math.Max(MinimumSize, newWidth);
            newHeight = Math.Max(MinimumSize, newHeight);

            // adjust the width
            double widthChange = newWidth - m_inputRegion.Width;
            double heightChange = newHeight - m_inputRegion.Height;

            m_inputRegion.Width = newWidth;
            m_inputRegion.Height = newHeight;

            // do we need to adjust the location as well?
            switch (direction)
            {
                default:
                case ScaleDirection.None:
                    // don't do anything
                    break;
                case ScaleDirection.NW:
                    m_inputRegion.Location -= (new Vector(widthChange, heightChange));
                    break;
                case ScaleDirection.NE:
                    m_inputRegion.Location -= (new Vector(0.0, heightChange));
                    break;
                case ScaleDirection.SE:
                    // this will not do anything
                    m_inputRegion.Location -= (new Vector(0.0, 0.0));
                    break;
                case ScaleDirection.SW:
                    m_inputRegion.Location -= (new Vector(widthChange, 0.0));
                    break;
                case ScaleDirection.N:
                    m_inputRegion.Location -= (new Vector(0.0, heightChange));
                    break;
                case ScaleDirection.W:
                    m_inputRegion.Location -= (new Vector(widthChange, 0.0));
                    break;
                case ScaleDirection.S:
                    // this will not do anything
                    m_inputRegion.Location -= (new Vector(0.0, 0.0));
                    break;
                case ScaleDirection.E:
                    // this will not do anything
                    m_inputRegion.Location -= (new Vector(0.0, 0.0));
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
                // check whether we hit an edge
                int hotEdgeIndex = -1;
                for (int i = 0; i < m_hotEdges.Length; i++)
                {
                    if (m_hotEdges[i].Contains(position))
                    {
                        hotEdgeIndex = i;
                        break;
                    }
                }

                if (hotEdgeIndex != -1)
                {
                    operation = MouseOperation.Scale;
                    direction = (ScaleDirection)Enum.ToObject(
                        typeof(ScaleDirection), hotEdgeIndex + (4 + 1));
                }
                else
                {
                    operation = m_inputRegion.Contains(position)
                        ? MouseOperation.Move : MouseOperation.None;
                    direction = ScaleDirection.None;
                }
            }

            return new Tuple<MouseOperation, ScaleDirection>(
                operation, direction);
        }
        #endregion

        #region Closing
        private void AcceptSelection()
        {
            // update the last region here, because the user accepted it
            m_lastInputRegion = m_inputRegion;

            Hide();

            // fire the event
            SelectionWindowClosed?.Invoke(this, new SelectionWindowEventArgs(ClosingReason.OK));
        }

        private void CancelSelection()
        {
            // revert back to the previous region, as the user has cancelled it
            m_inputRegion = m_lastInputRegion;
            Update();

            // enforce update
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, 
                new DispatcherOperationCallback(
                    delegate (object parameter)
                    {
                        frame.Continue = false;
                        return null;
            }), null);
            Dispatcher.PushFrame(frame);

            Hide();

            // fire the event
            SelectionWindowClosed?.Invoke(this, new SelectionWindowEventArgs(ClosingReason.Cancel));
        }
        #endregion

        #region Event Handler
        #region Window Event Handler
        private void CaptureSelectionWindowLoaded(object sender, RoutedEventArgs e)
        {
            ControlPanel.Visibility = Visibility.Visible;
            InitializeOverlay();
        }
        #endregion

        #region UI Event Handler
        private void DoneButtonClicked(object sender, RoutedEventArgs e)
        {
            AcceptSelection();
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            CancelSelection();
        }
        #endregion

        #region Key Event Handler
        private void WindowKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CancelSelection();
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
                            case ScaleDirection.N:
                            case ScaleDirection.S:
                                Cursor = Cursors.SizeNS;
                                break;
                            case ScaleDirection.W:
                            case ScaleDirection.E:
                                Cursor = Cursors.SizeWE;
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
