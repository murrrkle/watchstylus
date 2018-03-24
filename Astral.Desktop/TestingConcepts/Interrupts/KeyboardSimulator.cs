using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace TestingConcepts
{
    public class KeyboardSimulator
    {
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, KeyPress dwFlags, int dwExtraInfo);

        private enum KeyPress : int
        {
            Pressed = 0,
            Released = 2
        }

        public void PressKey(Key key)
        {
            byte virtualKey = Convert.ToByte(KeyInterop.VirtualKeyFromKey(key));
            keybd_event(virtualKey, 0, KeyPress.Pressed, 0);
            keybd_event(virtualKey, 0, KeyPress.Released, 0);

        }

        public void KeyDown(Key key)
        {
            byte virtualKey = Convert.ToByte(KeyInterop.VirtualKeyFromKey(key));
            keybd_event(virtualKey, 0, KeyPress.Pressed, 0);
        }

        public void KeyUp(Key key)
        {
            byte virtualKey = Convert.ToByte(KeyInterop.VirtualKeyFromKey(key));
            keybd_event(virtualKey, 0, KeyPress.Released, 0);
        }
    }
}
