using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TestingConcepts
{
    public class DiscreteRule : Rule
    {

        public override PCInputAction InputAction
        {
            get
            {
                return this.inputAction as DiscreteInputAction;
            }
        }

        public DiscreteRule(MobileEventType eventType)
            : base(eventType)
        {
            this.checksBounds = true;
            this.inputAction = new DiscreteInputAction();
            
        }

        public DiscreteRule(MobileEventType eventType, Rule ruleToCopy)
            : base(eventType, ruleToCopy)
        {
            this.checksBounds = true;
            this.inputAction = new DiscreteInputAction();
        }

        private void CheckEntered(Point current, Point previous)
        {
            if (current.IsInsideRect(this.sourceRect) && !previous.IsInsideRect(this.sourceRect))
            {
                Console.WriteLine("ENTERED");
                RaiseSelectionEntered(new SelectionCrossedEventArgs(current, previous, this.sourceRect, this.destinationRect));
            }

            if (previous.IsInsideRect(this.sourceRect) && !current.IsInsideRect(this.sourceRect))
            {
                Console.WriteLine("LEFT");
                RaiseSelectionExited(new SelectionCrossedEventArgs(current, previous, this.sourceRect, this.destinationRect));
            }
        }

        public override bool ExecuteRule(Point point)
        {
            if (this.previousPoint.X == Double.NaN && this.previousPoint.Y == Double.NaN)
            {
                // for the first exectution
                previousPoint = point;
            }

            if (point.IsInsideRect(sourceRect))
            {

                if (this.checksBounds)
                {
                    CheckEntered(point, this.previousPoint);
                }

                this.previousPoint = new Point(point.X, point.Y);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
