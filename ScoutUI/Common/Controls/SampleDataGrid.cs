using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ScoutUI.Common.Controls
{
    public class SampleDataGrid : DataGrid
    {
        public double ScrollToVerticalOffsetValue
        {
            get { return (double) GetValue(ScrollToVerticalOffsetValueProperty); }
            set { SetValue(ScrollToVerticalOffsetValueProperty, value); }
        }

        public static readonly DependencyProperty ScrollToVerticalOffsetValueProperty =
            DependencyProperty.Register("ScrollToVerticalOffsetValue", typeof(double), typeof(SampleDataGrid),
                new PropertyMetadata(0.0,
                    (sender, e) => ScrollToVerticalPropertyChange(sender as SampleDataGrid, e.NewValue)));

        private static void ScrollToVerticalPropertyChange(SampleDataGrid sampleDataGrid, object newValue)
        {
            ScrollViewer scrollViewer = FindVisualChild<ScrollViewer>(sampleDataGrid);
            double verticalOffSet = ScoutUtilities.Misc.DoubleTryParse(newValue.ToString()) ?? 0.0;
            scrollViewer.ScrollToVerticalOffset(verticalOffSet);
            sampleDataGrid.UpdateLayout();
        }

        private static childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem) child;

                childItem childOfChild = FindVisualChild<childItem>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        static SampleDataGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SampleDataGrid),
                new FrameworkPropertyMetadata(typeof(SampleDataGrid)));
        }
    }
}
