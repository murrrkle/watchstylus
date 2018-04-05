using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;

namespace TestingConcepts
{
    public class MouseSimulator
    {
        private int x = 0;
        private int y = 0;

        [DllImport("user32.dll")]
        private static extern void mouse_event
            (MouseEventType dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        [DllImport("user32")]
        private static extern int SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        private enum MouseEventType : int
        {
            LeftDown = 0x02,
            LeftUp = 0x04,
            RightDown = 0x08,
            RightUp = 0x10,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            Wheel = 0x800
        }

        public void Move(int x, int y)
        {
            SetCursorPos(x, y);
            this.x = x;
            this.y = y;
        }

        public void MoveVertical(int y)
        {
            SetCursorPos(this.x, y);
            this.y = y;
        }

        public void LeftClick()
        {
            mouse_event(MouseEventType.LeftDown, this.x, this.y, 0, 0);
            mouse_event(MouseEventType.LeftUp, this.x, this.y, 0, 0);
        }

        public void RightClick()
        {
            mouse_event(MouseEventType.RightDown, this.x, this.y, 0, 0);
            mouse_event(MouseEventType.RightUp, this.x, this.y, 0, 0);
        }

        public void LeftDown()
        {
            mouse_event(MouseEventType.LeftDown, this.x, this.y, 0, 0);
        }

        public void LeftDown(int x, int y)
        {
            mouse_event(MouseEventType.LeftDown, x, y, 0, 0);
        }

        public void LeftUp()
        {
            mouse_event(MouseEventType.LeftUp, this.x, this.y, 0, 0);
        }

        public void RightDown()
        {
            mouse_event(MouseEventType.RightDown, this.x, this.y, 0, 0);
        }

        public void RightUp()
        {
            mouse_event(MouseEventType.RightUp, this.x, this.y, 0, 0);
        }

        public void Wheel(int amount = 120)
        {
            mouse_event(MouseEventType.Wheel, this.x, this.y, amount, 0);
        }
    }
}
