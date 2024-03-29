﻿using System;
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
                Console.WriteLine("ENTERED " + this.inputAction.InputEvent + " :: " + this.inputAction.Argument);
                RaiseSelectionEntered(new SelectionCrossedEventArgs(current, previous, this.sourceRect, this.destinationRect));
            }

            //Console.WriteLine("current {0:00} \t top {1:00} \t bottom {2:00} \t previous {3:00} \t {4} {5}", current.Y, sourceRect.Top, sourceRect.Bottom, previous.Y, 
                //current.IsInsideRect(sourceRect), previous.IsInsideRect(sourceRect));

            else if (previous.IsInsideRect(this.sourceRect) && !current.IsInsideRect(this.sourceRect))
            {
                Console.WriteLine("LEFT");
                RaiseSelectionExited(new SelectionCrossedEventArgs(current, previous, this.sourceRect, this.destinationRect));
            }


            this.previousPoint = new Point(current.X, current.Y);
        }

        bool firstTime = true;

        public override bool ExecuteRule(Point point)
        {
            if(this.child != null && executedOnce)
            {
                Console.WriteLine("returning child");
                return this.child.ExecuteRule(point);
            }

            if (this.previousPoint.X == Double.NaN && this.previousPoint.Y == Double.NaN)
            {
                // for the first exectution
                previousPoint = point;
            }

            if (this.checksBounds)
            {
                CheckEntered(point, this.previousPoint);
            }


            if (point.IsInsideRect(sourceRect))
            {



                this.executedOnce = true;
                if(this.child != null)
                {
                    RaiseParentExecuted(new EventArgs());
                }

                if(IsMedleyRule)
                {
                    RaiseMedleyRule(new EventArgs());
                }

                if(this.destinationRect != null && inputAction != null && inputAction.InputEvent == PCInputEventType.MouseDown)
                {
                    this.inputAction.Argument = new Point(this.destinationRect.X, this.destinationRect.Y);
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
