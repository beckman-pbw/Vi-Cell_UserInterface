using ApiProxies.Generic;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ScoutUI.Common.Controls
{
    public class CustomProgressBar : ProgressBar
    {
      
        public static readonly DependencyProperty EllipseDiameterProperty =
            DependencyProperty.Register("EllipseDiameter", typeof(double), typeof(CustomProgressBar),
                new PropertyMetadata(default(double)));

        public static readonly DependencyProperty EllipseOffsetProperty =
            DependencyProperty.Register("EllipseOffset", typeof(double), typeof(CustomProgressBar),
                new PropertyMetadata(default(double)));

        private Storyboard _indeterminateStoryboard;

        private readonly object _locke = new object();
        
        static CustomProgressBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomProgressBar),
                new FrameworkPropertyMetadata(typeof(CustomProgressBar)));
        }

        private void LoadedHandler(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= LoadedHandler;
            SizeChangedHandler(null, null);
            SizeChanged += SizeChangedHandler;
        }

        public CustomProgressBar()
        {
            SizeChanged += SizeChangedHandler;
        }

        public double EllipseDiameter
        {
            get { return (double) GetValue(EllipseDiameterProperty); }
            set { SetValue(EllipseDiameterProperty, value); }
        }

        public double EllipseOffset
        {
            get { return (double) GetValue(EllipseOffsetProperty); }
            set { SetValue(EllipseOffsetProperty, value); }
        }

        private void SizeChangedHandler(object sender, SizeChangedEventArgs e)
        {
            var actualWidth = ActualWidth;
            var bar = this;
            bar.SetEllipseDiameter(actualWidth);
            bar.SetEllipseOffset(actualWidth);
            bar.ResetStoryboard(actualWidth);
        }


        private void ResetStoryboard(double width)
        {
            lock (this)
            {
                var containerAnimationStart = CalculateContainerAnimationStart(width);
                var containerAnimationEnd = CalculateContainerAnimationEnd(width);
                var ellipseAnimationWell = CalculateEllipseAnimationWell(width);
                var ellipseAnimationEnd = CalculateEllipseAnimationEnd(width);
                try
                {
                    var indeterminate = GetIndeterminate();
                    if (indeterminate == null || _indeterminateStoryboard == null)
                        return;
                    var newStoryboard = _indeterminateStoryboard.Clone();
                    var doubleAnimation = newStoryboard.Children.First(t => t.Name == "MainDoubleAnim");
                    doubleAnimation.SetValue(DoubleAnimation.FromProperty, containerAnimationStart);
                    doubleAnimation.SetValue(DoubleAnimation.ToProperty, containerAnimationEnd);
                    var namesOfElements = new[] {"E1", "E2", "E3", "E4", "E5"};
                    foreach (var elemName in namesOfElements)
                    {
                        var doubleAnimationParent =
                            (DoubleAnimationUsingKeyFrames) newStoryboard.Children.First(t => t.Name == elemName + "Anim");
                        DoubleKeyFrame first, second, third;
                        if (elemName == "E1")
                        {
                            first = doubleAnimationParent.KeyFrames[1];
                            second = doubleAnimationParent.KeyFrames[2];
                            third = doubleAnimationParent.KeyFrames[3];
                        }
                        else
                        {
                            first = doubleAnimationParent.KeyFrames[2];
                            second = doubleAnimationParent.KeyFrames[3];
                            third = doubleAnimationParent.KeyFrames[4];
                        }

                        first.Value = ellipseAnimationWell;
                        second.Value = ellipseAnimationWell;
                        third.Value = ellipseAnimationEnd;
                        first.InvalidateProperty(DoubleKeyFrame.ValueProperty);
                        second.InvalidateProperty(DoubleKeyFrame.ValueProperty);
                        third.InvalidateProperty(DoubleKeyFrame.ValueProperty);
                        doubleAnimationParent.InvalidateProperty(Storyboard.TargetPropertyProperty);
                        doubleAnimationParent.InvalidateProperty(Storyboard.TargetNameProperty);
                    }

                    var containingGrid = (FrameworkElement) GetTemplateChild("ContainingGrid");
                    if (indeterminate.Storyboard != null)
                    {
                        if (containingGrid != null)
                        {
                            indeterminate.Storyboard.Stop(containingGrid);
                            indeterminate.Storyboard.Remove(containingGrid);
                        }
                    }

                    indeterminate.Storyboard = newStoryboard;
                    if (containingGrid != null)
                        indeterminate.Storyboard?.Begin(containingGrid, true);
                }
                catch (Exception ex)
                {
                    ExceptionHelper.HandleExceptions(ex, ScoutLanguageResources.LanguageResourceHelper.Get("LID_EXCEPTIONMSG_PROGRESSBAR_RESET_STORYBOARD"));
                }
            }
        }

        private VisualState GetIndeterminate()
        {
            var templateGrid = GetTemplateChild("ContainingGrid") as FrameworkElement;
            if (templateGrid == null)
            {
                ApplyTemplate();
                templateGrid = GetTemplateChild("ContainingGrid") as FrameworkElement;
                if (templateGrid == null)
                    return null;
            }

            var groups = VisualStateManager.GetVisualStateGroups(templateGrid);
            return groups?.Cast<VisualStateGroup>()
                .SelectMany(@group => @group.States.Cast<VisualState>())
                .FirstOrDefault(state => state.Name == "Indeterminate");
        }


        private void SetEllipseDiameter(double width)
        {
            if (width <= 380)
            {
                EllipseDiameter = 4;
                return;
            }
            if (width <= 580)
            {
                EllipseDiameter = 5;
                return;
            }
            EllipseDiameter = 6;
        }

   
        private void SetEllipseOffset(double width)
        {
            if (width <= 180)
            {
                EllipseOffset = 4;
                return;
            }
            if (width <= 280)
            {
                EllipseOffset = 7;
                return;
            }
            EllipseOffset = 9;
        }

   
        private static double CalculateContainerAnimationStart(double width)
        {
            if (width <= 180)
                return -34;
            if (width <= 280)
                return -50.5;

            return -63;
        }

     
        private static double CalculateContainerAnimationEnd(double width)
        {
            var firstPart = 0.4352 * width;
            if (width <= 180)
                return firstPart - 25.731;
            if (width <= 280)
                return firstPart + 27.84;

            return firstPart + 58.862;
        }

    
        private static double CalculateEllipseAnimationWell(double width)
        {
            return width * 1.0 / 3.0;
        }

    
        private static double CalculateEllipseAnimationEnd(double width)
        {
            return width * 2.0 / 3.0;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            lock (_locke)
            {
                _indeterminateStoryboard = TryFindResource("IndeterminateStoryboard") as Storyboard;
            }

            SizeChangedHandler(null, null);
            Loaded -= LoadedHandler;
            Loaded += LoadedHandler;
        }
    }
}