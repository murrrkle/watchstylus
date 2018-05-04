using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TestingConcepts
{
    public enum MappingDimension
    {
        X = 1,
        Y = 2,
        XY = X | Y
    }

    public enum RuleArgument
    {
        None,
        X,
        Y,
        Z,
        Magnitude,
        Angle,
        Amplitude,
        Pitch
    }

    public class SelectionCrossedEventArgs : EventArgs
    {
        public Rect SourceRect { get; set; }
        public Rect DestinationRect { get; set; }
        public Point CurrentPoint { get; set; }
        public Point PreviousPoint { get; set; }

        public SelectionCrossedEventArgs(Point current, Point previous, Rect source, Rect destination)
        {
            this.SourceRect = source;
            this.DestinationRect = destination;
            this.CurrentPoint = current;
            this.PreviousPoint = previous;
        }
    }

    public abstract class Rule
    {
        protected string name = "x";
        protected MappingDimension dimension;
        protected MobileEventType eventType;
        protected bool checksBounds = false;
        protected Rect sourceRect;
        protected Rect destinationRect;
        protected Point previousPoint = new Point(Double.NaN, Double.NaN);
        protected bool executedOnce = false;

        protected Rect captureRegion;

        protected Rule parent;
        protected Rule child;

        protected PCInputAction inputAction;

        public Rect CaptureRegion
        {
            get
            {
                return this.captureRegion;
            }
            set
            {
                this.captureRegion = value;
            }
        }

        public bool IsMedleyRule { get; set; }

        public virtual PCInputAction InputAction { get; }
        
        public virtual RuleArgument ArgumentInfo { get; set; }

        public event EventHandler<SelectionCrossedEventArgs> SelectionEntered;
        public event EventHandler<SelectionCrossedEventArgs> SelectionExited;

        public event EventHandler<EventArgs> MedleyRuleExecuted;
        public event EventHandler<EventArgs> ParentExecuted;

        protected void RaiseMedleyRule(EventArgs e)
        {
            MedleyRuleExecuted?.Invoke(this, e);
        }

        protected void RaiseParentExecuted(EventArgs e)
        {
            ParentExecuted?.Invoke(this, e);
        }

        public override string ToString()
        {
            string s = "RULE: " + this.name + " (" + (this is ContinuousRule ? "Continuous" : "Discrete") + ") \n" +
                " Sensor Event " + this.eventType + "\n" +
                " PC Input Action: " + this.inputAction.InputEvent + " Arg (" + this.inputAction.Argument + ")\n Source " + this.sourceRect + "\n Destination ";
            return s;
        }

        protected void RaiseSelectionEntered(SelectionCrossedEventArgs e)
        {
            SelectionEntered?.Invoke(this, e);
        }

        protected void RaiseSelectionExited(SelectionCrossedEventArgs e)
        {
            SelectionExited?.Invoke(this, e);
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public bool ChecksBounds
        {
            get
            {
                return this.checksBounds;
            }
            set
            {
                this.checksBounds = value;
            }
        }

        public MappingDimension Dimension
        {
            get
            {
                return this.dimension;
            }
            set
            {
                this.dimension = value;
            }
        }

        public MobileEventType EventType
        {
            get
            {
                return this.eventType;
            }
            set
            {
                this.eventType = value;
            }
        }

        public Rule Child
        {
            get
            {
                return this.child;
            }
            set
            {
                this.child = value;
                this.child.Parent = this;
            }
        }

        public Rule Parent

        {
            get
            {
                return this.parent;
            }
            set
            {
                this.parent = value;
            }
        }

        internal bool ExecutedOnce
        {
            get
            {
                return this.executedOnce;
            }
            set
            {
                this.executedOnce = value;
            }
        }

        public Rect SourceRect { get => sourceRect; set => sourceRect = value; }
        public Rect DestinationRect { get => destinationRect; set => destinationRect = value; }
        public Point PreviousPoint { get => this.previousPoint; }
        public string FilterInfo { get; set; }

        public Rule(MobileEventType eventType)
        {
            this.IsMedleyRule = false;
            this.eventType = eventType;
        }

        public Rule(MobileEventType eventType, Rule ruleToCopy)
        {
            this.IsMedleyRule = false;
            this.eventType = eventType;
            this.sourceRect = ruleToCopy.sourceRect;
            this.destinationRect = ruleToCopy.destinationRect;
        }

        public abstract bool ExecuteRule(Point value);

            
    }
}
