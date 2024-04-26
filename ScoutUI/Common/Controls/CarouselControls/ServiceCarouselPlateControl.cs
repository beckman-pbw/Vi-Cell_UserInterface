using ScoutDomains.Common;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ScoutUI.Common.Controls
{
    public class ServiceCarouselPlateControl : BaseCarouselControl
    {
        #region Constructor

        static ServiceCarouselPlateControl()
        {
            const int defaultPosition = 1;
            MyItemsSourceProperty = DependencyProperty.Register(nameof(MyItemsSource), typeof(ObservableCollection<SampleDomain>),  typeof(ServiceCarouselPlateControl));
            ChangesDialogResultProperty = DependencyProperty.Register(nameof(ChangesDialogResult), typeof(bool), typeof(ServiceCarouselPlateControl), new PropertyMetadata(ChangesDialogResultPropertyValueChangeCallback));
            HighlightedSampleProperty = DependencyProperty.Register(nameof(HighLightedSample), typeof(int), typeof(ServiceCarouselPlateControl));
            PlayItemPositionProperty = DependencyProperty.Register(nameof(PlayItemPosition), typeof(int), typeof(ServiceCarouselPlateControl), new PropertyMetadata(defaultPosition, (s, e) => PlayItemPositionValueChangeCallback(s as ServiceCarouselPlateControl, e.OldValue, e.NewValue)));
            IsClearingProperty = DependencyProperty.Register(nameof(IsClearing), typeof(bool), typeof(ServiceCarouselPlateControl), new PropertyMetadata(ClearingSampleQueueCallBack));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ServiceCarouselPlateControl), new FrameworkPropertyMetadata(typeof(ServiceCarouselPlateControl)));

            StopSampleQueueProperty = DependencyProperty.Register("StopSampleQueue", typeof(bool), typeof(ServiceCarouselPlateControl), new PropertyMetadata(StopSampleQueueCallBack));
        }

        public ServiceCarouselPlateControl()
        {
            DefinedSelectedSample = new StringBuilder();
            SetGridRotateAngle = 15;
            CheckDefinedSampleList();
            DataContext = this;
        }

        #endregion

        #region Static Properties

        private static CirclePanelControl _parentControlInstanceButton;
        private static CirclePanelControl _parentControlInstanceLabel;

        private static string _selectedPositions;
        public static string SelectedPositions
        {
            get { return _selectedPositions; }
            set
            {
                if (value != _selectedPositions)
                {
                    _selectedPositions = value;
                    NotifyStaticPropertyChanged(nameof(SelectedPositions));
                }
            }
        }

        #endregion

        #region Overriden Methods

        public override void OnApplyTemplate()
        {
            if (_parentControlInstanceButton == null)
            {
                _parentControlInstanceButton = Template.FindName(CirclePanelButton, this) as CirclePanelControl;
            }

            if (_parentControlInstanceLabel == null)
            {
                _parentControlInstanceLabel = Template.FindName(CirclePanelLabel, this) as CirclePanelControl;
            }

            _txtPosition = Template.FindName("txtPosition", this) as TextBox;
            if (_txtPosition != null)
            {
                _txtPosition.TextChanged += txtPosition_TextChanged;
            }
            base.OnApplyTemplate();
        }

        protected override void txtPosition_TextChanged(object sender, TextChangedEventArgs e)
        {
            SelectedPositions = _txtPosition.Text;
        }

        #endregion

        #region Dependency Property

        public static readonly DependencyProperty StopSampleQueueProperty;

        private static void ChangesDialogResultPropertyValueChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ServiceCarouselPlateControl carousel))
                return;
            if (!carousel.ChangesDialogResult)
                return;
            if (carousel.MyItemsSource == null)
                return;
            if (!(carousel.Template.FindName(CirclePanelButton, carousel) is CirclePanelControl buttonList))
                return;

            var concentrationType1List = carousel.MyItemsSource.Where(p =>
                p.SampleStatusColor.Equals(SampleStatusColor.ConcentrationType1) ||
                p.SampleStatusColor.Equals(SampleStatusColor.ConcentrationType2) ||
                p.SampleStatusColor.Equals(SampleStatusColor.ConcentrationType3)).ToList();

            var concentrationCompletedList = carousel.MyItemsSource
                .Where(p => p.SampleStatusColor.Equals(SampleStatusColor.Completed)).ToList();

            foreach (var item in buttonList.Children)
            {
                if (!(item is Button button))
                    return;

                if (concentrationType1List.Any(p => Convert.ToInt32(p.SamplePosition.Column) == Convert.ToInt32(button.Tag)))
                {
                    var sample = concentrationType1List.FirstOrDefault(p => Convert.ToInt32(p.SamplePosition.Column) == Convert.ToInt32(button.Tag));
                    if (sample != null)
                    {
                        button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(sample.SampleStatusColor)));
                    }
                }
                else
                {
                    var sample = concentrationCompletedList.FirstOrDefault(p => Convert.ToInt32(p.SamplePosition.Column) == Convert.ToInt32(button.Tag));
                    if (sample != null)
                    {
                        button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(sample.SampleStatusColor)));
                    }
                    else
                    {
                        button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Empty)));
                    }
                }
            }
        }

        private static void PlayItemPositionValueChangeCallback(ServiceCarouselPlateControl carousel, object oldValue, object newValue)
        {
            if (carousel.MyItemsSource == null)
                return;
            if (!(carousel.Template.FindName(CirclePanelButton, carousel) is CirclePanelControl buttonList))
                return;
            if (!(carousel.Template.FindName(CirclePanelLabel, carousel) is CirclePanelControl labelList))
                return;

            var incrementToRotatePosition = 0;
            var oldPosition = (int)oldValue;
            var newPosition = (int)newValue;

            if (newPosition == -1)
            {
                foreach (var item in buttonList.Children)
                {
                    if (!(item is Button button))
                        return;

                    button.BorderBrush = new SolidColorBrush(Colors.Transparent);
                }
            }

            if (oldPosition < 0)
                return;

            if (newPosition < 0)
            {
                carousel.PlayItemPosition = newPosition = oldPosition;
                return;
            }

            if (carousel.PlayItemPosition == 0)
            {
                oldValue = newValue = 1;
            }

            if (oldPosition < newPosition)
            {
                incrementToRotatePosition = newPosition - oldPosition;
            }
            else if (oldPosition > newPosition)
            {
                incrementToRotatePosition = (24 - (oldPosition - newPosition));
            }

            carousel.HighLightedSample = newPosition;

            for (var i = 0; i < incrementToRotatePosition; i++)
            {
                carousel.RotateGrid(labelList);
            }

            carousel.SetButtonSelectionColor(carousel);
        }

        private static void StopSampleQueueCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ServiceCarouselPlateControl carousel))
                return;
            if (carousel.MyItemsSource == null)
                return;
            if (!(carousel.Template.FindName(CirclePanelButton, carousel) is CirclePanelControl buttonList))
                return;

            foreach (var item in buttonList.Children)
            {
                if (!(item is Button button))
                    return;

                button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF000000"));
            }
        }

        private static void ClearingSampleQueueCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ServiceCarouselPlateControl carousel))
                return;
            if (carousel.MyItemsSource == null)
                return;
            if (!carousel.IsClearing)
                return;
            if (!(carousel.Template.FindName(CirclePanelButton, carousel) is CirclePanelControl buttonList))
                return;

            foreach (var item in buttonList.Children)
            {
                if (!(item is Button button))
                    return;

                button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF000000"));
            }
        }

        #region MyItemsSource

        public ObservableCollection<SampleDomain> MyItemsSource
        {
            get { return (ObservableCollection<SampleDomain>)GetValue(MyItemsSourceProperty); }
            set
            {
                SetValue(MyItemsSourceProperty, value);
                NotifyPropertyChanged(nameof(MyItemsSource));
            }
        }

        public static readonly DependencyProperty MyItemsSourceProperty;

        #endregion

        #region IsRotateButtonEnable

        public bool IsRotateButtonEnable
        {
            get { return (bool)GetValue(IsRotateButtonEnableProperty); }
            set { SetValue(IsRotateButtonEnableProperty, value); }
        }

        public static readonly DependencyProperty IsRotateButtonEnableProperty =
            DependencyProperty.Register(nameof(IsRotateButtonEnable), typeof(bool), typeof(ServiceCarouselPlateControl), new PropertyMetadata(false));

        #endregion

        #region HighLightedSample

        public int HighLightedSample
        {
            get { return (int)GetValue(HighlightedSampleProperty); }
            set { SetValue(HighlightedSampleProperty, value); }
        }

        public static readonly DependencyProperty HighlightedSampleProperty;

        #endregion

        #region ChangesDialogResult

        public bool ChangesDialogResult
        {
            get { return (bool)GetValue(ChangesDialogResultProperty); }
            set { SetValue(ChangesDialogResultProperty, value); }
        }

        public static readonly DependencyProperty ChangesDialogResultProperty;

        #endregion

        #region PlayItemPosition

        public int PlayItemPosition
        {
            get { return (int)GetValue(PlayItemPositionProperty); }
            set { SetValue(PlayItemPositionProperty, value); }
        }

        public static readonly DependencyProperty PlayItemPositionProperty;

        #endregion

        #region ControlEnable

        public bool ControlEnable
        {
            get { return (bool)GetValue(ControlEnableProperty); }
            set { SetValue(ControlEnableProperty, value); }
        }

        public static readonly DependencyProperty ControlEnableProperty =
            DependencyProperty.Register(nameof(ControlEnable), typeof(bool), typeof(ServiceCarouselPlateControl), new FrameworkPropertyMetadata());

        #endregion

        #region IsClearing

        public bool IsClearing
        {
            get { return (bool)GetValue(IsClearingProperty); }
            set { SetValue(IsClearingProperty, value); }
        }

        public static readonly DependencyProperty IsClearingProperty;

        #endregion

        #region IsStatusCompleted

        public bool IsStatusCompleted
        {
            get { return (bool)GetValue(IsStatusCompletedProperty); }
            set { SetValue(IsStatusCompletedProperty, value); }
        }

        public static readonly DependencyProperty IsStatusCompletedProperty =
            DependencyProperty.Register(nameof(IsStatusCompleted), typeof(bool), typeof(ServiceCarouselPlateControl), new PropertyMetadata(StatusCompletedCallBack));

        private static void StatusCompletedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ServiceCarouselPlateControl serviceCarouselPlateControl))
                return;
            if (!serviceCarouselPlateControl.IsStatusCompleted)
                return;
            if (serviceCarouselPlateControl.MyItemsSource == null)
                return;
            if (!(serviceCarouselPlateControl.Template.FindName("CirclePanelButton", serviceCarouselPlateControl) is CirclePanelControl buttonList))
                return;

            var processedList = serviceCarouselPlateControl.MyItemsSource.Where(p => p.SampleStatusColor.Equals(SampleStatusColor.Completed)).ToList();

            foreach (var child in buttonList.Children)
            {
                if (!(child is Button button))
                    return;

                if (processedList.Any(p => Convert.ToInt32(p.SamplePosition.Column) == Convert.ToInt32(button.Tag)))
                {
                    var sample = processedList.FirstOrDefault(p => Convert.ToInt32(p.SamplePosition.Column) == Convert.ToInt32(button.Tag));
                    if (sample != null)
                    {
                        button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Defined)));
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Dependency Property Commands

        public ICommand RotateGridCommand
        {
            get { return (ICommand)GetValue(RotateGridCommandProperty); }
            set { SetValue(RotateGridCommandProperty, value); }
        }

        public static readonly DependencyProperty RotateGridCommandProperty =
            DependencyProperty.Register(nameof(RotateGridCommand), typeof(ICommand), typeof(ServiceCarouselPlateControl), new PropertyMetadata(null));

        #endregion

        #region Private Methods

        private void RotateGrid(object objGrid)
        {
            if (Math.Abs(SetGridRotateAngle - 360.0) < 1)
            {
                SetGridRotateAngle = 0;
            }
            SetGridRotateAngle += 15;
            var circlePanelControlObj = objGrid as CirclePanelControl;
            NotifyPropertyChanged(nameof(SetGridRotateAngle));
            RotateLabelCounterWise(circlePanelControlObj);
            HighLightButtonOnMark();
        }

        private void RotateLabelCounterWise(CirclePanelControl circlePanelControlObjParam)
        {
            var counterRotate = new RotateTransform(-SetGridRotateAngle);
            foreach (var child in circlePanelControlObjParam.Children)
            {
                var label = child as Label;
                if (label == null)
                    return;
                label.RenderTransform = counterRotate;
                label.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }

        private void CheckDefinedSampleList()
        {
            ConsecutiveItemList = new List<int>();
            DefinedSelectedSample = new StringBuilder();
            CirclePanelControl.ApiList.Sort();
            for (var i = 0; i < CirclePanelControl.ApiList.Count; i++)
            {
                if (CirclePanelControl.ApiList.Count != i + 1)
                {
                    if (CirclePanelControl.ApiList[i + 1] - CirclePanelControl.ApiList[i] == 1)
                    {
                        ConsecutiveItemList.Add(CirclePanelControl.ApiList[i]);
                    }
                    else
                    {
                        UpdateDefinedSelectedSample(ConsecutiveItemList, i);
                        ConsecutiveItemList = new List<int>();
                    }
                }
                else
                {
                    UpdateDefinedSelectedSample(ConsecutiveItemList, i);
                }
            }

            SelectedPositions = DefinedSelectedSample.ToString();
        }


        private void UpdateDefinedSelectedSample(ICollection<int> consecutiveItemListParam, int countValue)
        {
            if (consecutiveItemListParam != null && consecutiveItemListParam.Count != 0)
            {
                consecutiveItemListParam.Add(CirclePanelControl.ApiList[countValue]);
                if (DefinedSelectedSample.Length == 0)
                {
                    DefinedSelectedSample.Append(consecutiveItemListParam.Min(x => x) + "-" +
                                                 consecutiveItemListParam.Max(x => x));
                }
                else
                {
                    DefinedSelectedSample.Append("," + consecutiveItemListParam.Min(x => x) + "-" +
                                                 consecutiveItemListParam.Max(x => x));
                }
            }
            else
            {
                if (DefinedSelectedSample.Length == 0)
                {
                    DefinedSelectedSample.Append(CirclePanelControl.ApiList[countValue]);
                }
                else
                {
                    DefinedSelectedSample.Append("," + CirclePanelControl.ApiList[countValue]);
                }
            }
        }

        private void HighLightButtonOnMark()
        {
            var buttonList = Template.FindName("CirclePanelButton", this) as CirclePanelControl;
            if (buttonList == null)
                return;
            foreach (var item in buttonList.Children)
            {
                var button = item as Button;
                if (button == null)
                    return;
                if (button.Tag.ToString().Equals(HighLightedSample.ToString()))
                    button.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFF9B3"));
                else
                    button.BorderBrush = new SolidColorBrush(Colors.Transparent);
            }
        }

        private void SetButtonSelectionColor(ServiceCarouselPlateControl serviceCarouselPlateControl)
        {
            if (MyItemsSource == null)
                return;
            if (!(serviceCarouselPlateControl.Template.FindName(CirclePanelButton, serviceCarouselPlateControl) is CirclePanelControl buttonList))
                return;

            var selectedList = MyItemsSource.Where(p =>
                p.SampleStatusColor.Equals(SampleStatusColor.ConcentrationType1) ||
                p.SampleStatusColor.Equals(SampleStatusColor.ConcentrationType2) ||
                p.SampleStatusColor.Equals(SampleStatusColor.ConcentrationType3)).ToList();

            var processedList = MyItemsSource.Where(p => p.SampleStatusColor.Equals(SampleStatusColor.Completed)).ToList();

            foreach (var child in buttonList.Children)
            {
                if (!(child is Button button))
                    return;

                if (selectedList.Any(p => Convert.ToInt32(p.SamplePosition.Column) == Convert.ToInt32(button.Tag)))
                {
                    var sample = selectedList.FirstOrDefault(p => Convert.ToInt32(p.SamplePosition.Column) == Convert.ToInt32(button.Tag));
                    if (sample != null)
                    {
                        button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(sample.SampleStatusColor)));
                    }
                }

                if (processedList.Any(p => Convert.ToInt32(button.Tag) == Convert.ToInt32(p.SamplePosition.Column - 1)))
                {
                    var sample = selectedList.FirstOrDefault(p => Convert.ToInt32(p.SamplePosition.Column) == Convert.ToInt32(button.Tag));
                    if (sample != null)
                    {
                        button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Defined)));
                    }
                }
            }
        }

        #endregion

        #region Static Notify Implementation

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        private static void NotifyStaticPropertyChanged(string propertyName)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}