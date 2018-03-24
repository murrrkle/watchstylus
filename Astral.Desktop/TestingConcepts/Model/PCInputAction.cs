using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TestingConcepts
{
    public enum PCInputEventType
    {
        None,
        MouseDown,
        MouseUp,
        MouseClick,
        MouseMove,
        MouseScroll,
        KeyDown,
        KeyUp,
        KeyPress
    }

    public class PCInputAction
    {
        protected PCInputEventType inputEvent;
        protected object argument;

        public virtual PCInputEventType InputEvent
        {
            get
            {
                return this.inputEvent;
            }
            set
            {
                this.inputEvent = value;
            }
        }        

        public virtual object Argument
        {
            get
            {
                return this.argument;
            }
            set
            {
                this.argument = value;
            }
        }
    }

    public class DiscreteInputAction : PCInputAction
    {
        public override PCInputEventType InputEvent
        {
            get
            {
                return this.inputEvent;
            }
            set
            {
                if(this.inputEvent == PCInputEventType.MouseScroll || this.inputEvent == PCInputEventType.MouseMove)
                {
                    throw new InvalidOperationException("Tried Adding a Continuous Event to a Discrete Event");
                }
                this.inputEvent = value;
            }
        }
    }

    public class ContinuousInputAction : PCInputAction
    {
        public override PCInputEventType InputEvent
        {
            get
            {
                return this.inputEvent;
            }
            set
            {
                if (this.inputEvent == PCInputEventType.MouseScroll || this.inputEvent == PCInputEventType.MouseMove || this.inputEvent == PCInputEventType.None)
                {
                    this.inputEvent = value;
                }
                else
                {
                    throw new InvalidOperationException("Tried Adding a Discrete Event to a Continuous Event " + this.inputEvent.ToString());
                }
            }
        }
    }


}
