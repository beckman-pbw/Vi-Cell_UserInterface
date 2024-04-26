using System.Windows;
using System.Windows.Controls;

namespace ScoutUI.Common.Controls
{
    public class AnimatedContentControl : ContentControl
    {
      
        public static readonly DependencyProperty ReverseTransitionProperty =
            DependencyProperty.Register("ReverseTransition", typeof(bool), typeof(AnimatedContentControl),
                new FrameworkPropertyMetadata(false));

        public bool ReverseTransition
        {
            get { return (bool) GetValue(ReverseTransitionProperty); }
            set { SetValue(ReverseTransitionProperty, value); }
        }


        public AnimatedContentControl()
        {
            DefaultStyleKey = typeof(AnimatedContentControl);
            Loaded += MetroContentControlLoaded;
            Unloaded += MetroContentControlUnloaded;
            IsVisibleChanged += MetroContentControlIsVisibleChanged;
        }

        void MetroContentControlIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible)
                VisualStateManager.GoToState(this, ReverseTransition ? "AfterUnLoadedReverse" : "AfterUnLoaded", false);
            else
                VisualStateManager.GoToState(this, ReverseTransition ? "AfterLoadedReverse" : "AfterLoaded", true);
        }

    
        private void MetroContentControlUnloaded(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, ReverseTransition ? "AfterUnLoadedReverse" : "AfterUnLoaded", false);
        }

     
        private void MetroContentControlLoaded(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, ReverseTransition ? "AfterLoadedReverse" : "AfterLoaded", true);
        }

     
        public void Reload()
        {
            if (ReverseTransition)
            {
                VisualStateManager.GoToState(this, "BeforeLoaded", true);
                VisualStateManager.GoToState(this, "AfterUnLoadedReverse", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "BeforeLoaded", true);
                VisualStateManager.GoToState(this, "AfterLoaded", true);
            }
        }
    }
}