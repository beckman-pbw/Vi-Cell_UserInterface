using System;
using System.Windows;
using System.Windows.Controls;

namespace ScoutUI.Common.Controls
{
    public class CircumferencePanelControl : Panel
    {
        public Thickness Padding { get; set; }
        public bool OrderCounterClockwise { get; set; }

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement element in Children)
            {
                element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            }

            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count == 0) return finalSize;

            var curAngle = 90.0 * (Math.PI / 180.0);
            var radiansPerElement = (360.0 / Children.Count) * (Math.PI / 180.0);
            var radiansX = finalSize.Width / 2.0 - Padding.Left;
            var radiansY = finalSize.Height / 2.0 - Padding.Top;
            foreach (UIElement element in Children)
            {
                var childPoint = new Point(
                    Math.Cos(curAngle) * radiansX,
                    -Math.Sin(curAngle) * radiansY);
                var centerPoint = new Point(
                    childPoint.X + finalSize.Width / 2 - element.DesiredSize.Width / 2,
                    childPoint.Y + finalSize.Height / 2 - element.DesiredSize.Height / 2);

                var boundingBox = new Rect(centerPoint, element.DesiredSize);
                element.Arrange(boundingBox);

                if (OrderCounterClockwise)
                    curAngle += radiansPerElement;
                else
                    curAngle -= radiansPerElement;
            }

            return finalSize;
        }
    }
}