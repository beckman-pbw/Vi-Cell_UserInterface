using ScoutDomains.Common;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ScoutUI.Common.Controls
{
    public class CarouselDesignButtonControl : Panel, INotifyPropertyChanged
    {
        static CarouselDesignButtonControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CarouselDesignButtonControl), new FrameworkPropertyMetadata(typeof(CarouselDesignButtonControl)));
        }

        public static Button CirclePanelButton = new Button();
        public static List<int> ApiList = new List<int>();

        #region INotify Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string param)
        {
            DispatcherHelper.ApplicationExecute(() =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(param)));
        }

        #endregion
        
        #region Dependency Properties

        #region OuterRadius

        public double OuterRadius
        {
            get { return (double)GetValue(OuterRadiusProperty); }
            set { SetValue(OuterRadiusProperty, value); }
        }

        public static readonly DependencyProperty OuterRadiusProperty = DependencyProperty.Register(nameof(OuterRadius), typeof(double),
            typeof(CarouselDesignButtonControl), new UIPropertyMetadata(0.0, OuterRadiusPropertyCallback));

        private static void OuterRadiusPropertyCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is CarouselDesignButtonControl circlePanelControl)
            {
                circlePanelControl.Width = (double)e.NewValue * 2;
                circlePanelControl.Height = (double)e.NewValue * 2;
            }
        }

        #endregion

        #region ControlEnable

        public bool ControlEnable
        {
            get { return (bool)GetValue(ControlEnableProperty); }
            set { SetValue(ControlEnableProperty, value); }
        }

        public static readonly DependencyProperty ControlEnableProperty =
            DependencyProperty.Register(nameof(ControlEnable), typeof(bool), typeof(CarouselDesignButtonControl), new FrameworkPropertyMetadata());

        #endregion

        #region MyItemsSource

        public List<SampleDomain> MyItemsSource
        {
            get { return (List<SampleDomain>)GetValue(MyItemsSourceProperty); }
            set
            {
                SetValue(MyItemsSourceProperty, value);
                NotifyPropertyChanged(nameof(MyItemsSource));
            }
        }

        public static readonly DependencyProperty MyItemsSourceProperty =
            DependencyProperty.Register(nameof(MyItemsSource), typeof(List<SampleDomain>), typeof(CarouselDesignButtonControl), new FrameworkPropertyMetadata());

        #endregion

        #region HighlightedSample

        public int HighlightedSample
        {
            get { return (int)GetValue(HighlightedSampleProperty); }
            set
            {
                SetValue(HighlightedSampleProperty, value);
                NotifyPropertyChanged(nameof(HighlightedSample));
            }
        }

        public static readonly DependencyProperty HighlightedSampleProperty =
            DependencyProperty.Register(nameof(HighlightedSample), typeof(int), typeof(CarouselDesignButtonControl), new FrameworkPropertyMetadata());

        #endregion

        #region InnerRadius

        public double InnerRadius
        {
            get { return (double)GetValue(InnerRadiusProperty); }
            set { SetValue(InnerRadiusProperty, value); }
        }

        public static readonly DependencyProperty InnerRadiusProperty =
            DependencyProperty.Register(nameof(InnerRadius), typeof(double), typeof(CarouselDesignButtonControl), new UIPropertyMetadata(0.0));

        #endregion

        #region Positions

        public string Positions
        {
            get { return (string)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(nameof(Positions), typeof(string), typeof(CarouselDesignButtonControl), new PropertyMetadata(PositionsChangedPropertyCallback));

        private static void PositionsChangedPropertyCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is CarouselDesignButtonControl circlePanelControl))
            {
                throw new ArgumentNullException(nameof(circlePanelControl));
            }
            circlePanelControl.SetValue(PositionProperty, e.NewValue);
        }

        #endregion

        #region CirclePanelName

        public string CirclePanelName
        {
            get { return (string)GetValue(CirclePanelNameProperty); }
            set { SetValue(CirclePanelNameProperty, value); }
        }

        public static readonly DependencyProperty CirclePanelNameProperty =
            DependencyProperty.Register(nameof(CirclePanelName), typeof(string), typeof(CarouselDesignButtonControl), new UIPropertyMetadata());

        #endregion

        #region ControlFrom

        public string ControlFrom
        {
            get { return (string)GetValue(ControlFromProperty); }
            set { SetValue(ControlFromProperty, value); }
        }

        public static readonly DependencyProperty ControlFromProperty =
            DependencyProperty.Register(nameof(ControlFrom), typeof(string), typeof(CarouselDesignButtonControl), new UIPropertyMetadata());

        #endregion

        #endregion

        #region Override Methods

        protected override Size MeasureOverride(Size availableSize)
        {
            var count = Children.Count;
            switch (CirclePanelName)
            {
                case "Button":
                    if (count < 24)
                    {
                        for (var i = 24; i >= 1; i--)
                        {
                            CirclePanelButton = new Button
                            {
                                Tag = i,
                                BorderThickness = new Thickness(0),
                                Style = Application.Current.FindResource("RunSampleRoundButtonStyle") as Style
                            };

                            CirclePanelButton.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Empty)));
                            CirclePanelButton.PreviewMouseLeftButtonUp += CarouselDesignControl.Instance.OnCarouselDesignButtonPressUp;
                            CirclePanelButton.PreviewMouseLeftButtonDown += CarouselDesignControl.Instance.OnCarouselDesignButtonPressDown;
                            Children.Add(CirclePanelButton);
                        }
                    }
                    break;

                case "Label":
                    if (count < 24)
                    {
                        for (var i = 24; i >= 1; i--)
                        {
                            var circlePanelLabel = new Label()
                            {
                                Width = 40,
                                Height = 40,
                                FontSize = 18,
                                FontWeight = FontWeights.Bold,
                                HorizontalContentAlignment = HorizontalAlignment.Center,
                                VerticalContentAlignment = VerticalAlignment.Center,
                                Content = i
                            };
                            Children.Add(circlePanelLabel);
                        }

                        CarouselDesignControl.Instance?.RotateLabelCounterWise(this);
                    }
                    break;
            }

            foreach (UIElement child in Children)
            {
                child.Measure(availableSize);
            }

            return new Size(2 * OuterRadius, 2 * OuterRadius);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var currentPosition = new Point(OuterRadius, (OuterRadius - InnerRadius) / 2);
            var childCount = Children.Count;
            double perAngle = 2 * Math.PI / childCount;
            for (var i = 0; i < childCount; i++)
            {
                double OffsetX = 0.0, OffsetY = 0.0;
                var child = Children[i];
                var angle = (i + 1) * perAngle;
                OffsetX = Math.Sin(angle) * (OuterRadius + InnerRadius) / 2;
                OffsetY = (1 - Math.Cos(angle)) * (OuterRadius + InnerRadius) / 2;

                var childRect = new Rect(
                    new Point(currentPosition.X - child.DesiredSize.Width / 2, currentPosition.Y - child.DesiredSize.Height / 2),
                    new Point(currentPosition.X + child.DesiredSize.Width / 2, currentPosition.Y + child.DesiredSize.Height / 2));
                child.Arrange(childRect);
                currentPosition.X = OffsetX + OuterRadius;
                currentPosition.Y = OffsetY + (OuterRadius - InnerRadius) / 2;
            }

            return new Size(2 * OuterRadius, 2 * OuterRadius);
        }

        #endregion
    }
}