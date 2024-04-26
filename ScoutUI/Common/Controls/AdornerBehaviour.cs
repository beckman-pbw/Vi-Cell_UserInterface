using ScoutUI.Common.Helper;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace ScoutUI.Common.Controls
{
    public class AdornerBehaviour : ContentControl
    {
       
        public static readonly DependencyProperty IsAdornerVisibleProperty =
            DependencyProperty.Register("IsAdornerVisible", typeof(bool), typeof(AdornerBehaviour),
                new FrameworkPropertyMetadata(IsAdornerVisible_PropertyChanged));

        public static readonly DependencyProperty AdornerContentProperty =
            DependencyProperty.Register("AdornerContent", typeof(FrameworkElement), typeof(AdornerBehaviour),
                new FrameworkPropertyMetadata(AdornerContent_PropertyChanged));

        public static readonly DependencyProperty HorizontalAdornerPlacementProperty =
            DependencyProperty.Register("HorizontalAdornerPlacement", typeof(AdornerPlacement), typeof(AdornerBehaviour),
                new FrameworkPropertyMetadata(AdornerPlacement.Inside));

        public static readonly DependencyProperty VerticalAdornerPlacementProperty =
            DependencyProperty.Register("VerticalAdornerPlacement", typeof(AdornerPlacement), typeof(AdornerBehaviour),
                new FrameworkPropertyMetadata(AdornerPlacement.Inside));

        public static readonly DependencyProperty AdornerOffsetXProperty =
            DependencyProperty.Register("AdornerOffsetX", typeof(double), typeof(AdornerBehaviour));

        public static readonly DependencyProperty AdornerOffsetYProperty =
            DependencyProperty.Register("AdornerOffsetY", typeof(double), typeof(AdornerBehaviour));

        public static readonly RoutedCommand ShowAdornerCommand =
            new RoutedCommand("ShowAdorner", typeof(AdornerBehaviour));

        public static readonly RoutedCommand HideAdornerCommand =
            new RoutedCommand("HideAdorner", typeof(AdornerBehaviour));

        public AdornerBehaviour()
        {
            DataContextChanged += AdornerBehaviour_DataContextChanged;
        }

       
        private void AdornerBehaviour_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateAdornerDataContext();
        }

     
        private void UpdateAdornerDataContext()
        {
            if (AdornerContent != null)
            {
                AdornerContent.DataContext = DataContext;
            }
        }

     
        public void ShowAdorner()
        {
            IsAdornerVisible = true;
        }

        public void HideAdorner()
        {
            IsAdornerVisible = false;
        }

        public bool IsAdornerVisible
        {
            get { return (bool) GetValue(IsAdornerVisibleProperty); }
            set { SetValue(IsAdornerVisibleProperty, value); }
        }

        public FrameworkElement AdornerContent
        {
            get { return (FrameworkElement) GetValue(AdornerContentProperty); }
            set { SetValue(AdornerContentProperty, value); }
        }

    
        public AdornerPlacement HorizontalAdornerPlacement
        {
            get { return (AdornerPlacement) GetValue(HorizontalAdornerPlacementProperty); }
            set { SetValue(HorizontalAdornerPlacementProperty, value); }
        }

     
        public AdornerPlacement VerticalAdornerPlacement
        {
            get { return (AdornerPlacement) GetValue(VerticalAdornerPlacementProperty); }
            set { SetValue(VerticalAdornerPlacementProperty, value); }
        }

     
        public double AdornerOffsetX
        {
            get { return (double) GetValue(AdornerOffsetXProperty); }
            set { SetValue(AdornerOffsetXProperty, value); }
        }

        public double AdornerOffsetY
        {
            get { return (double) GetValue(AdornerOffsetYProperty); }
            set { SetValue(AdornerOffsetYProperty, value); }
        }

        #region Private Data Members

     
        private static readonly CommandBinding ShowAdornerCommandBinding =
            new CommandBinding(ShowAdornerCommand, ShowAdornerCommand_Executed);

        private static readonly CommandBinding HideAdornerCommandBinding =
            new CommandBinding(HideAdornerCommand, HideAdornerCommand_Executed);

     
        private AdornerLayer _adornerLayer;

      
        private AdornerHelper _adorner;

        #endregion


        static AdornerBehaviour()
        {
            CommandManager.RegisterClassCommandBinding(typeof(AdornerBehaviour), ShowAdornerCommandBinding);
            CommandManager.RegisterClassCommandBinding(typeof(AdornerBehaviour), HideAdornerCommandBinding);
        }

        private static void ShowAdornerCommand_Executed(object target, ExecutedRoutedEventArgs e)
        {
            AdornerBehaviour c = (AdornerBehaviour) target;
            c.ShowAdorner();
        }

     
        private static void HideAdornerCommand_Executed(object target, ExecutedRoutedEventArgs e)
        {
            AdornerBehaviour c = (AdornerBehaviour) target;
            c.HideAdorner();
        }

     
        private static void IsAdornerVisible_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            AdornerBehaviour c = (AdornerBehaviour) o;
            c.ShowOrHideAdornerInternal();
        }

     
        private static void AdornerContent_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            AdornerBehaviour c = (AdornerBehaviour) o;
            c.ShowOrHideAdornerInternal();
        }

        private void ShowOrHideAdornerInternal()
        {
            if (IsAdornerVisible)
            {
                ShowAdornerInternal();
            }
            else
            {
                HideAdornerInternal();
            }
        }

    
        private void ShowAdornerInternal()
        {
            if (_adorner != null)
            {
                // Already adorned.
                return;
            }
            if (AdornerContent == null)
                return;
            if (_adornerLayer == null)
            {
                _adornerLayer = AdornerLayer.GetAdornerLayer(this);
            }
            if (_adornerLayer != null)
            {
                _adorner = new AdornerHelper(AdornerContent, this, HorizontalAdornerPlacement,
                    VerticalAdornerPlacement, AdornerOffsetX, AdornerOffsetY);
                _adornerLayer.Add(_adorner);
                UpdateAdornerDataContext();
            }
        }

     
        private void HideAdornerInternal()
        {
            if (_adornerLayer == null || _adorner == null)
            {
                // Not already adorned.
                return;
            }
            _adornerLayer.Remove(_adorner);
            _adorner.DisconnectChild();
            _adorner = null;
            _adornerLayer = null;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ShowOrHideAdornerInternal();
        }
    }

    public enum AdornerPlacement
    {
        Inside,
        Outside
    }
}