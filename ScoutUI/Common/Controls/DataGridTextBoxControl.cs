using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ScoutUI.Common.Controls
{
    public static class DataGridTextBoxControl
    {
        public static object GetSelectedItem(DependencyObject obj)
        {
            return (object) obj.GetValue(SelectedItemProperty);
        }
        
        public static void SetSelectedItem(DependencyObject obj, object value)
        {
            obj.SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.RegisterAttached("SelectedItem", typeof(object), typeof(DataGridTextBoxControl),
                new PropertyMetadata(false, OnSelectedItem));

        private static void OnSelectedItem(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (!(dp is DataGrid))
                return;
            var grid = (DataGrid) dp;
            grid.Dispatcher.InvokeAsync(() =>
            {
                grid.UpdateLayout();
                if (grid.SelectedItem != null)
                    grid.ScrollIntoView(grid.SelectedItem, null);
            });
        }

        public static T FindVisualChildByName<T>(this DependencyObject parent, string name) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var controlName = child.GetValue(FrameworkElement.NameProperty) as string;
                if (controlName == name)
                {
                    return child as T;
                }

                T result = FindVisualChildByName<T>(child, name);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static T FindElementByName<T>(FrameworkElement element, string sChildName) where T : FrameworkElement
        {
            T childElement = null;
            var nChildCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < nChildCount; i++)
            {
                FrameworkElement child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;
                if (child == null)
                    continue;
                if (child is T && child.Name.Equals(sChildName))
                {
                    childElement = (T) child;
                    break;
                }
                childElement = FindElementByName<T>(child, sChildName);
                if (childElement != null)
                    break;
            }

            return childElement;
        }

        public static T GetChildOfType<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
                return null;
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}