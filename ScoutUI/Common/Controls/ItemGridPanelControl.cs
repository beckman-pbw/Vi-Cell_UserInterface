using System;
using System.Windows;
using System.Windows.Controls;
using ScoutUtilities.Common;

namespace ScoutUI.Common.Controls
{
    public class ItemGridPanelControl : Panel
    {
        public Thickness Padding { get; set; }

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

            var curX = 0;
            var curY = 0;
            var maxX = ApplicationConstants.PlateNumColumnsCount;
            var maxY = ApplicationConstants.PlateNumRowsCount;

            foreach (UIElement element in Children)
            {
                var posX = (finalSize.Width / maxX - Padding.Left) * curX;
                var posY = (finalSize.Height / maxY - Padding.Top) * curY;
                var childPoint = new Point(posX, posY);
                var boundingBox = new Rect(childPoint, element.DesiredSize);

                element.Arrange(boundingBox);

                curX++;
                if (curX == maxX)
                {
                    curX = 0;
                    curY++;
                    if (curY > maxY) throw new NotSupportedException("ItemGridPanelControl has too many children!");
                }
            }

            return finalSize;
        }
    }
}