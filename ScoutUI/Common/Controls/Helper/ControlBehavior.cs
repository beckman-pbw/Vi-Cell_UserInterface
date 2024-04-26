using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ScoutUI.Common.Controls.Helper
{
  
    public class ControlBehavior
    {
      
        public static DependencyProperty OnDataContextChangedProperty =
            DependencyProperty.RegisterAttached("OnDataContextChanged", typeof(bool), typeof(ControlBehavior),
                new PropertyMetadata(OnDataContextChanged));

      
        public static bool GetOnDataContextChanged(AnimatedContentControl element)
        {
            return (bool) element.GetValue(OnDataContextChangedProperty);
        }

      
        public static void SetOnDataContextChanged(AnimatedContentControl element, bool value)
        {
            element.SetValue(OnDataContextChangedProperty, value);
        }

   
        private static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AnimatedContentControl) d).DataContextChanged += ReloadDataContextChanged;
        }

    
        static void ReloadDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((AnimatedContentControl) sender).Reload();
        }

     
        public static DependencyProperty OnSelectedTabChangedProperty =
            DependencyProperty.RegisterAttached("OnSelectedTabChanged", typeof(bool), typeof(ControlBehavior),
                new PropertyMetadata(OnSelectedTabChanged));

    
        public static bool GetOnSelectedTabChanged(AnimatedContentControl element)
        {
            return (bool) element.GetValue(OnDataContextChangedProperty);
        }

     
        public static void SetOnSelectedTabChanged(AnimatedContentControl element, bool value)
        {
            element.SetValue(OnDataContextChangedProperty, value);
        }

    
        private static void OnSelectedTabChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AnimatedContentControl) d).Loaded += ReloadLoaded;
        }

      
        static void ReloadLoaded(object sender, RoutedEventArgs e)
        {
            var metroContentControl = ((AnimatedContentControl) sender);
            var tab = Ancestors(metroContentControl).OfType<TabControl>().FirstOrDefault();
            if (tab == null)
                return;
            SetMetroContentControl(tab, metroContentControl);
            tab.SelectionChanged += ReloadSelectionChanged;
        }

     
        private static IEnumerable<DependencyObject> Ancestors(DependencyObject obj)
        {
            var parent = VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                yield return parent;
                obj = parent;
                parent = VisualTreeHelper.GetParent(obj);
            }
        }

     
        static void ReloadSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource != sender)
                return;
            GetMetroContentControl((TabControl) sender).Reload();
        }

    
        public static readonly DependencyProperty MetroContentControlProperty =
            DependencyProperty.RegisterAttached("MetroContentControl", typeof(AnimatedContentControl),
                typeof(ControlBehavior), new PropertyMetadata(default(AnimatedContentControl)));

      
        public static void SetMetroContentControl(UIElement element, AnimatedContentControl value)
        {
            element.SetValue(MetroContentControlProperty, value);
        }

   
        public static AnimatedContentControl GetMetroContentControl(UIElement element)
        {
            return (AnimatedContentControl) element.GetValue(MetroContentControlProperty);
        }
    }
}