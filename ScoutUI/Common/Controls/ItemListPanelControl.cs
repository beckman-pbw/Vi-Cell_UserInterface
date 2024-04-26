using System;
using System.Windows;
using System.Windows.Controls;

namespace ScoutUI.Common.Controls
{
    public class ItemListPanelControl : Panel
    {
        public Thickness Padding { get; set; }
        public int NumberOfItems { get; set; }
        public bool VerticalLayout { get; set; }

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

            var curNum = 0;
            var maxNum = NumberOfItems;

            foreach (UIElement element in Children)
            {
                Point childPoint;
                if (VerticalLayout)
                {
                    var pos = (finalSize.Height / maxNum - Padding.Top) * curNum;
                    childPoint = new Point(0, pos);
                }
                else
                {
                    var pos = (finalSize.Width / maxNum - Padding.Left) * curNum;
                    childPoint = new Point(pos, 0);
                }
                
                var boundingBox = new Rect(childPoint, element.DesiredSize);
                element.Arrange(boundingBox);

                curNum++;
                if (curNum > maxNum)
                {
                    throw new NotSupportedException("ItemListPanelControl has too many children!");
                }
            }

            return finalSize;
        }
    }
}