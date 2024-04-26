using System;
using System.Windows;
using System.Windows.Controls;

namespace ScoutUI.Common.Controls
{
    public class CirclePanel : Panel
    {
        #region Dependency Properties

        public int OuterRadius
        {
            get { return (int)GetValue(OuterRadiusProperty); }
            set { SetValue(OuterRadiusProperty, value); }
        }

        public static readonly DependencyProperty OuterRadiusProperty =
            DependencyProperty.Register(nameof(OuterRadius), typeof(int), typeof(CirclePanel), new PropertyMetadata(0));

        public int InnerRadius
        {
            get { return (int)GetValue(InnerRadiusProperty); }
            set { SetValue(InnerRadiusProperty, value); }
        }

        public static readonly DependencyProperty InnerRadiusProperty =
            DependencyProperty.Register(nameof(InnerRadius), typeof(int), typeof(CirclePanel), new PropertyMetadata(0));

        #endregion

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement u in Children)
            {
                u.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            }

            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count == 0)
                return finalSize;

            double angle = 30;

            // Degrees converted to Radian by multiplying with PI/180
            double incrementalAngularSpace = (360.0 / Children.Count) * (Math.PI / 180);
            
            // An approximate radii based on the available size , obviously a better approach is needed here.
            double radiusX = finalSize.Width / 2.4;
            double radiusY = finalSize.Height / 2.4;
            foreach (UIElement elem in Children)
            {
                // Calculate the point on the circle for the element
                var childPoint = new Point(Math.Cos(angle) * radiusX, -Math.Sin(angle) * radiusY);
                
                // Offsetting the point to the available rectangular area which is FinalSize.
                var actualChildPoint = new Point(
                    finalSize.Width  / 2 + childPoint.X - elem.DesiredSize.Width  / 2,
                    finalSize.Height / 2 + childPoint.Y - elem.DesiredSize.Height / 2);
                
                // Call Arrange method on the child element by giving the calculated point as the placementPoint.
                elem.Arrange(new Rect(actualChildPoint.X, actualChildPoint.Y, elem.DesiredSize.Width, elem.DesiredSize.Height));
                
                // Calculate the new _angle for the next element
                angle += incrementalAngularSpace;
            }

            return finalSize;
        }
    }
}
