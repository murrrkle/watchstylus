using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;


namespace TestingConcepts
{
    public class PCInputHandler
    {
        private KeyboardHook keyboardHook = new KeyboardHook();
        private MouseSimulator mouseSimulator = new MouseSimulator();
        private KeyboardSimulator keyboardSimulator = new KeyboardSimulator();

        public event EventHandler<EventArgs> EscapeCaptured;

        private void RaiseEscapeCaptured(EventArgs e)
        {
            EscapeCaptured?.Invoke(this, e);
        }

        public PCInputHandler()
        {
            this.keyboardHook.HookKeyboard();
            this.keyboardHook.OnKeyPressed += OnGlobalKeyPress;
        }

        /// <summary>
        /// Checks for the Escape key being pressed outside the application to
        /// kill input rerouting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGlobalKeyPress(object sender, KeyPressedArgs e)
        {
            if(e.KeyPressed == Key.Escape)
            {
                RaiseEscapeCaptured(new EventArgs());
            }
        }

        public void MoveMouse(int x, int y)
        {
            this.mouseSimulator.Move(x, y);
        }

        public void Scroll(int amount)
        {
            this.mouseSimulator.Wheel(amount);
        }

        public void MouseLeftDown()
        {
            this.mouseSimulator.LeftDown();
        }

        public void MouseLeftUp()
        {
            this.mouseSimulator.LeftUp();
        }

        public void MouseLeftClick()
        {
            this.mouseSimulator.LeftClick();
        }

        public void ExecuteInputAction(PCInputAction action)
        {
            switch (action.InputEvent)
            {
                case PCInputEventType.MouseDown:
                    this.mouseSimulator.LeftDown();
                    break;
                case PCInputEventType.MouseUp:
                    this.mouseSimulator.LeftUp();
                    break;
                case PCInputEventType.MouseClick:
                    if(action.Argument != null)
                    {
                        int x = (int)((action.Argument as Point?).GetValueOrDefault().X);
                        int y = (int)((action.Argument as Point?).GetValueOrDefault().Y);
                        this.mouseSimulator.Move(x, y);
                    }
                    this.mouseSimulator.LeftClick();
                    break;
                case PCInputEventType.MouseMove:
                    if (action.Argument != null)
                    {
                        
                        int x = (int)((action.Argument as Point?).GetValueOrDefault().X);
                        int y = (int)((action.Argument as Point?).GetValueOrDefault().Y);
                        this.mouseSimulator.Move(x, y);
                    }
                    break;
                case PCInputEventType.MouseScroll:
                    if (action.Argument != null)
                    {
                        int value = Convert.ToInt32(action.Argument);
                        this.mouseSimulator.Wheel(value);
                    }
                    break;
                case PCInputEventType.KeyDown:
                    Key argumentKey = (Key)Enum.Parse(typeof(Key), action.Argument.ToString());
                    break;
                case PCInputEventType.KeyUp:
                    break;
                case PCInputEventType.KeyPress:
                    break;
                default:
                    break;
            }
        }


    }
}
