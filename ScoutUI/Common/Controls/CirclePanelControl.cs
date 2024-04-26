using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using ScoutUtilities.Enums;
using System.ComponentModel;
using ScoutDomains.Common;
using ScoutUtilities.Common;

namespace ScoutUI.Common.Controls
{
    public class CirclePanelControl : Panel, INotifyPropertyChanged
    {
        public static Button CirclePanelButton = new Button();

        public static List<int> ApiList = new List<int>();

        public double OuterRadius
        {
            get { return (double) GetValue(OuterRadiusProperty); }
            set { SetValue(OuterRadiusProperty, value); }
        }

        public static readonly DependencyProperty OuterRadiusProperty =
            DependencyProperty.Register("OuterRadius", typeof(double), typeof(CirclePanelControl),
                new UIPropertyMetadata(0.0, (o, e) =>
                {
                    var circlePanelControl = o as CirclePanelControl;
                    if (circlePanelControl != null)
                        circlePanelControl.Width = (double) e.NewValue * 2;

                    var panelControl = o as CirclePanelControl;
                    if (panelControl != null)
                        panelControl.Height = (double)e.NewValue * 2;
                }));

    
        public bool ControlEnable
        {
            get { return (bool) GetValue(ControlEnableProperty); }
            set { SetValue(ControlEnableProperty, value); }
        }

        public static readonly DependencyProperty ControlEnableProperty =
            DependencyProperty.Register("ControlEnable", typeof(bool), typeof(CirclePanelControl),
                new FrameworkPropertyMetadata { });

        public List<SampleDomain> MyItemsSource
        {
            get { return (List<SampleDomain>) GetValue(MyItemsSourceProperty); }
            set
            {
                SetValue(MyItemsSourceProperty, value);
                NotifyPropertyChanged(nameof(MyItemsSource));
            }
        }

        public static readonly DependencyProperty HighlightedSampleProperty =
            DependencyProperty.Register("HighlightedSample", typeof(int), typeof(CirclePanelControl),
                new FrameworkPropertyMetadata { });

        public int HighlightedSample
        {
            get { return (int) GetValue(HighlightedSampleProperty); }
            set
            {
                SetValue(HighlightedSampleProperty, value);
                NotifyPropertyChanged(nameof(HighlightedSample));
            }
        }

        public static readonly DependencyProperty MyItemsSourceProperty =
            DependencyProperty.Register("MyItemsSource", typeof(List<SampleDomain>), typeof(CirclePanelControl), new FrameworkPropertyMetadata { });

        public double InnerRadius
        {
            get { return (double) GetValue(InnerRadiusProperty); }
            set { SetValue(InnerRadiusProperty, value); }
        }

        public static readonly DependencyProperty InnerRadiusProperty =
            DependencyProperty.Register("InnerRadius", typeof(double), typeof(CirclePanelControl), new UIPropertyMetadata(0.0));

        public string Positions
        {
            get { return (string) GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

   
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Positions", typeof(string), typeof(CirclePanelControl),
                new PropertyMetadata(PositionsChangedPropertyCallback));

        private static void PositionsChangedPropertyCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var circlePanelControl = sender as CirclePanelControl;
            if (circlePanelControl == null)
                throw new ArgumentNullException(nameof(circlePanelControl));
            circlePanelControl?.SetValue(PositionProperty, e.NewValue);
        }

        public string CirclePanelName
        {
            get { return (string) GetValue(CirclePanelNameProperty); }
            set { SetValue(CirclePanelNameProperty, value); }
        }

        public static readonly DependencyProperty CirclePanelNameProperty =
            DependencyProperty.Register("CirclePanelName", typeof(string), typeof(CirclePanelControl), new UIPropertyMetadata());


        public string ControlFrom
        {
            get { return (string) GetValue(ControlFromProperty); }
            set { SetValue(ControlFromProperty, value); }
        }

        public static readonly DependencyProperty ControlFromProperty =
            DependencyProperty.Register("ControlFrom", typeof(string), typeof(CirclePanelControl), new UIPropertyMetadata());

        static CirclePanelControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CirclePanelControl), new FrameworkPropertyMetadata(typeof(CirclePanelControl)));
        }

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
                            CirclePanelButton.IsEnabled = ControlEnable;
                            if (i == 1)
                            {
                                CirclePanelButton.BorderBrush = (SolidColorBrush) (new BrushConverter().ConvertFrom(
                                    GetEnumDescription.GetDescription(SampleStatusColor.HighLighted)));
                            }
                            else
                            {
                                CirclePanelButton.BorderBrush = new SolidColorBrush(Colors.Transparent);
                            }

                            CirclePanelButton.Tag = i;
                            CirclePanelButton.BorderThickness = new Thickness(8);
                            CirclePanelButton.Style = Application.Current.FindResource("RunSampleRoundButtonStyle") as Style;
                            if (ControlFrom != null)
                            {
                                CirclePanelButton.Background = (SolidColorBrush) (new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Empty)));
                            }
                            else
                            {
                                CirclePanelButton.Background = (SolidColorBrush) (new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Empty)));
                            }

                            CirclePanelButton.Click += CirclePanelButton_Click;
                            Children.Add(CirclePanelButton);
                            CirclePanelButton = new Button();
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

                        CarouselRunControl.Instance?.RotateLabelCounterWise(this);
                    }

                    break;
            }

            foreach (UIElement child in Children)
            {
                child.Measure(availableSize);
            }

            return new Size(2 * OuterRadius, 2 * OuterRadius);
        }

        private void CirclePanelButton_Click(object sender, RoutedEventArgs e)
        {
            var addCarouselPlateControl = new CarouselDesignControl();
            var sendButton = sender as Button;
            var result = MyItemsSource.FirstOrDefault(
                list => sendButton != null && list.SamplePosition.Column.ToString() == sendButton.Tag.ToString());
            if (sendButton != null && result != null)
            {
                if (result.SampleStatusColor == SampleStatusColor.Empty)
                {
                    sendButton.Background = (SolidColorBrush) (new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Selected)));
                    result.SampleStatusColor = SampleStatusColor.Selected;
                    result.CurrentSelectedSample = true;
                    ApiList.Add(Convert.ToInt32(sendButton.Tag));
                }
                else
                {
                    sendButton.Background = (SolidColorBrush) (new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Empty)));
                    result.CurrentSelectedSample = false;
                    result.SampleStatusColor = SampleStatusColor.Empty;
                    ApiList.Remove(Convert.ToInt32(sendButton.Tag));
                }
            }

            //To create
            addCarouselPlateControl.CheckDefinedSampleList();
            Positions = CarouselDesignControl.SelectedPositions;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var currentPosition = new Point(OuterRadius, (OuterRadius - InnerRadius) / 2);
            var childCount = Children.Count;

            double perAngle = 2 * Math.PI / childCount;
            double OffsetX = 0.0, OffsetY = 0.0;
            for (var i = 0; i < childCount; i++)
            {
                var child = Children[i];
                var angle = (i + 1) * perAngle;
                OffsetX = Math.Sin(angle) * (OuterRadius + InnerRadius) / 2;
                OffsetY = (1 - Math.Cos(angle)) * (OuterRadius + InnerRadius) / 2;
                var childRect = new Rect(
                    new Point(currentPosition.X - child.DesiredSize.Width / 2,
                        currentPosition.Y - child.DesiredSize.Height / 2),
                    new Point(currentPosition.X + child.DesiredSize.Width / 2,
                        currentPosition.Y + child.DesiredSize.Height / 2));
                child.Arrange(childRect);
                currentPosition.X = OffsetX + OuterRadius;
                currentPosition.Y = OffsetY + (OuterRadius - InnerRadius) / 2;
            }

            return new Size(2 * OuterRadius, 2 * OuterRadius);
        }

        #region INotify Implementation

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected void NotifyPropertyChanged(string param)
        {
            DispatcherHelper.ApplicationExecute(() =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(param)));
        }

        #endregion
    }
}
