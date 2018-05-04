using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TestingConcepts
{
    public enum AstralContinuousRuleMapping
    {
        Move,
        Scroll
    }

    public class ContinuousRule : Rule
    {
        private EasingType easing = EasingType.Linear;

    //    private AstralContinuousRuleMapping destinationMapping;

        // This holds the result from Execute
        private Point latestConvertedPoint;

        private bool invert = false;

        public EasingType Easing { get => easing; set => easing = value; }
    //    public AstralContinuousRuleMapping DestinationMouseMapping { get => destinationMapping; set => destinationMapping = value; }

        public override PCInputAction InputAction
        {
            get
            {
                return this.inputAction as ContinuousInputAction;
            }
        }

        public Point LatestConvertedPoint
        {
            get
            {
                return this.latestConvertedPoint;
            }
        }

        public bool Invert
        {
            get
            {
                return this.invert;
            }
            set
            {
                this.invert = value;
            }
        }

        public ContinuousRule(MobileEventType eventType)
            : base(eventType)
        {
            this.inputAction = new ContinuousInputAction();
        }
        


        private void CheckEntered(Point current, Point previous)
        {
            if(current.IsInsideRect(this.sourceRect) && !previous.IsInsideRect(this.sourceRect))
            {
                RaiseSelectionEntered(new SelectionCrossedEventArgs(current, previous, this.sourceRect, this.destinationRect));
            }

            if(previous.IsInsideRect(this.sourceRect) && !current.IsInsideRect(this.sourceRect))
            {
                RaiseSelectionExited(new SelectionCrossedEventArgs(current, previous, this.sourceRect, this.destinationRect));
            }
        }
        
        public override bool ExecuteRule(Point point)
        {
            if(this.parent != null)
            {
                if (!parent.ExecutedOnce)
                {
                    Console.WriteLine("parent hasn't executed");
                    return false;
                }
            }
            
            if(this.previousPoint.X == Double.NaN && this.previousPoint.Y == Double.NaN)
            {
                // for the first exectution
                previousPoint = point;
            }

            if(!point.IsInsideRect(sourceRect))
            {
                return false;
            }
            else
            {
                double x = point.X;
                double y = point.Y;

                if ((this.dimension & MappingDimension.X) == MappingDimension.X)
                {
                    // map x coordinates
                    x = Utils.Map(x, sourceRect.TopLeft.X, sourceRect.TopRight.X, destinationRect.TopLeft.X, destinationRect.TopRight.X);
                    //if (this.name.Contains("Slide")) x = Utils.Map(x, sourceRect.TopLeft.X, sourceRect.TopRight.X, destinationRect.TopLeft.Y, destinationRect.TopRight.Y);
                    // turn into value from 0 to 1 to apply easing
                    double xAsPercent = Utils.Map(x, destinationRect.TopLeft.X, destinationRect.TopRight.X, 0, 1);
                        // apply easing
                        xAsPercent = AstralEasings.Interpolate(xAsPercent, this.easing);

                        if (this.invert)
                        {
                            xAsPercent = 1 - xAsPercent;
                        }

                        // convert back
                        x = Utils.Map(xAsPercent, 0, 1, destinationRect.TopLeft.X, destinationRect.TopRight.X);
                    
                }

                if((this.dimension & MappingDimension.Y) == MappingDimension.Y)
                {
                    // map y coordinates
                    //if (!this.name.Contains("Slide"))
                        y = Utils.Map(y, sourceRect.TopLeft.Y, sourceRect.BottomLeft.Y, destinationRect.TopLeft.Y, destinationRect.BottomLeft.Y);
                    // HAX
                    //if (this.name.Contains("Slide"))
                    //{
                    //    y = Utils.Map(y, sourceRect.TopLeft.Y, sourceRect.BottomLeft.Y, destinationRect.TopLeft.X, destinationRect.TopRight.X);
                    //    Console.WriteLine(destinationRect.Width + " :: " + y);
                    //}
                    // apply easing on y
               //     if (this.easing != EasingType.Linear)
                    {
                        // turn into value from 0 to 1 to apply easing
                        double yAsPercent = Utils.Map(y, destinationRect.TopLeft.Y, destinationRect.BottomLeft.Y, 0, 1);
                        // apply easing
                        yAsPercent = AstralEasings.Interpolate(yAsPercent, this.easing);
                        if(this.invert)
                        {
                            yAsPercent = 1 - yAsPercent;
                        }
                        // convert back
                        y = Utils.Map(yAsPercent, 0, 1, destinationRect.TopLeft.Y, destinationRect.BottomLeft.Y);
                    }
                }

                //this.latestConvertedPoint = new Point(x, y);

                if (this.Dimension == MappingDimension.XY)
                {
                    this.InputAction.Argument = new Point(x, y);
                    if((this.name.Contains("Slide"))) this.InputAction.Argument = new Point(y, x);
                }
                else
                {
                    if(this.dimension == MappingDimension.X)
                    {
                        this.InputAction.Argument = x;
                    }
                    else
                    {
                        this.InputAction.Argument = y;
                    }
                }
                if (this.checksBounds)
                {
                    CheckEntered(point, this.previousPoint);
                }

                this.previousPoint = new Point(point.X, point.Y);
                return true;
            }
        }
    }
}
