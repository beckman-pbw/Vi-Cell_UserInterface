using ScoutUI.Common.Controls;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Documents;

namespace ScoutUI.Common.Helper
{
   
    public class AdornerHelper : Adorner
    {
       
        private FrameworkElement child;
        
        private AdornerPlacement horizontalAdornerPlacement = AdornerPlacement.Inside;

        private AdornerPlacement verticalAdornerPlacement = AdornerPlacement.Inside;
        
        private double _offsetX;
        
        private double _offsetY;
        
        private double _positionX = Double.NaN;

        private double _positionY = Double.NaN;

        public AdornerHelper(FrameworkElement adornerChildElement, FrameworkElement adornedElement)
            : base(adornedElement)
        {
            child = adornerChildElement;
            AddLogicalChild(adornerChildElement);
            AddVisualChild(adornerChildElement);
        }

        public AdornerHelper(FrameworkElement adornerChildElement, FrameworkElement adornedElement,
            AdornerPlacement horizontalAdornerPlacement, AdornerPlacement verticalAdornerPlacement,
            double offsetX, double offsetY)
            : base(adornedElement)
        {
            child = adornerChildElement;
            this.horizontalAdornerPlacement = horizontalAdornerPlacement;
            this.verticalAdornerPlacement = verticalAdornerPlacement;
            _offsetX = offsetX;
            _offsetY = offsetY;
            adornedElement.SizeChanged += adornedElement_SizeChanged;
            AddLogicalChild(adornerChildElement);
            AddVisualChild(adornerChildElement);
        }

        private void adornedElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InvalidateMeasure();
        }

        public double PositionX
        {
            get { return _positionX; }
            set { _positionX = value; }
        }

        public double PositionY
        {
            get { return _positionY; }
            set { _positionY = value; }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            child.Measure(constraint);
            return child.DesiredSize;
        }

        private double DetermineX()
        {
            switch (child.HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                {
                    if (horizontalAdornerPlacement == AdornerPlacement.Outside)
                    {
                        return -child.DesiredSize.Width + _offsetX;
                    }

                    return _offsetX;
                }
                case HorizontalAlignment.Right:
                {
                    if (horizontalAdornerPlacement == AdornerPlacement.Outside)
                    {
                        double adornedWidth = AdornedElement.ActualWidth;
                        return adornedWidth + _offsetX;
                    }
                    else
                    {
                        double adornerWidth = child.DesiredSize.Width;
                        double adornedWidth = AdornedElement.ActualWidth;
                        double x = adornedWidth - adornerWidth;
                        return x + _offsetX;
                    }
                }
                case HorizontalAlignment.Center:
                {
                    double adornerWidth = child.DesiredSize.Width;
                    double adornedWidth = AdornedElement.ActualWidth;
                    double x = (adornedWidth / 2) - (adornerWidth / 2);
                    return x + _offsetX;
                }
                case HorizontalAlignment.Stretch:
                {
                    return 0.0;
                }
            }

            return 0.0;
        }

        private double DetermineY()
        {
            switch (child.VerticalAlignment)
            {
                case VerticalAlignment.Top:
                {
                    if (verticalAdornerPlacement == AdornerPlacement.Outside)
                    {
                        return -child.DesiredSize.Height + _offsetY;
                    }

                    return _offsetY;
                }
                case VerticalAlignment.Bottom:
                {
                    if (verticalAdornerPlacement == AdornerPlacement.Outside)
                    {
                        double adornedHeight = AdornedElement.ActualHeight;
                        return adornedHeight + _offsetY;
                    }
                    else
                    {
                        double adornerHeight = child.DesiredSize.Height;
                        double adornedHeight = AdornedElement.ActualHeight;
                        double x = adornedHeight - adornerHeight;
                        return x + _offsetY;
                    }
                }
                case VerticalAlignment.Center:
                {
                    double adornerHeight = child.DesiredSize.Height;
                    double adornedHeight = AdornedElement.ActualHeight;
                    double x = (adornedHeight / 2) - (adornerHeight / 2);
                    return x + _offsetY;
                }
                case VerticalAlignment.Stretch:
                {
                    return 0.0;
                }
            }

            return 0.0;
        }

        private double DetermineWidth()
        {
            if (!Double.IsNaN(PositionX))
            {
                return child.DesiredSize.Width;
            }

            switch (child.HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                {
                    return child.DesiredSize.Width;
                }
                case HorizontalAlignment.Right:
                {
                    return child.DesiredSize.Width;
                }
                case HorizontalAlignment.Center:
                {
                    return child.DesiredSize.Width;
                }
                case HorizontalAlignment.Stretch:
                {
                    return AdornedElement.ActualWidth;
                }
            }

            return 0.0;
        }

        private double DetermineHeight()
        {
            if (!Double.IsNaN(PositionY))
            {
                return child.DesiredSize.Height;
            }

            switch (child.VerticalAlignment)
            {
                case VerticalAlignment.Top:
                {
                    return child.DesiredSize.Height;
                }
                case VerticalAlignment.Bottom:
                {
                    return child.DesiredSize.Height;
                }
                case VerticalAlignment.Center:
                {
                    return child.DesiredSize.Height;
                }
                case VerticalAlignment.Stretch:
                {
                    return AdornedElement.ActualHeight;
                }
            }

            return 0.0;
        }

      
        protected override Size ArrangeOverride(Size finalSize)
        {
            double x = PositionX;
            if (Double.IsNaN(x))
            {
                x = DetermineX();
            }

            double y = PositionY;
            if (Double.IsNaN(y))
            {
                y = DetermineY();
            }

            double adornerWidth = DetermineWidth();
            double adornerHeight = DetermineHeight();
            child.Arrange(new Rect(x, y, adornerWidth, adornerHeight));
            return finalSize;
        }

     
        protected override Int32 VisualChildrenCount => 1;
        
        protected override System.Windows.Media.Visual GetVisualChild(Int32 index)
        {
            return child;
        }

        protected override IEnumerator LogicalChildren
        {
            get
            {
                ArrayList list = new ArrayList();
                list.Add(child);
                return (IEnumerator) list.GetEnumerator();
            }
        }

     
        public void DisconnectChild()
        {
            RemoveLogicalChild(child);
            RemoveVisualChild(child);
        }

      
        public new FrameworkElement AdornedElement => (FrameworkElement) base.AdornedElement;
    }
}