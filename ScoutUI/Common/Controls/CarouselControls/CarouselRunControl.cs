using ScoutDomains.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ScoutUI.Common.Controls
{
    public class CarouselRunControl : BaseCarouselControl
    {   
        #region Constructor

        public CarouselRunControl()
        {
            Instance = this;
            DefinedSelectedSample = new StringBuilder();
            SetGridRotateAngle = 15;
            CheckDefinedSampleList();
            if (Instance != null)
                DataContext = Instance;
        }

        static CarouselRunControl()
        {
            const int defaultPosition = 1;
            MyItemsSourceProperty = DependencyProperty.Register(nameof(MyItemsSource), typeof(List<SampleDomain>), typeof(CarouselRunControl));
            ChangesDialogResultProperty = DependencyProperty.Register(nameof(ChangesDialogResult), typeof(bool), typeof(CarouselRunControl), new PropertyMetadata(ChangesDialogResultPropertyValueChangeCallback));
            HighlightedSampleProperty = DependencyProperty.Register(nameof(HighLightedSample), typeof(int), typeof(CarouselRunControl));
            PlayItemPositionProperty = DependencyProperty.Register(nameof(PlayItemPosition), typeof(int), typeof(CarouselRunControl),
                new PropertyMetadata(defaultPosition, (s, e) => PlayItemPositionValueChangeCallback(s as CarouselRunControl, e.OldValue, e.NewValue)));
            IsClearingProperty = DependencyProperty.Register(nameof(IsClearing), typeof(bool), typeof(CarouselRunControl), new PropertyMetadata(ClearingSampleQueueCallBack));
            ChangesCarouselCreationProperty = DependencyProperty.Register(nameof(ChangesCarouselCreation), typeof(bool), typeof(CarouselRunControl), new PropertyMetadata(ChangesCarouselCreationPropertyValueChangeCallback));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CarouselRunControl), new FrameworkPropertyMetadata(typeof(CarouselRunControl)));

            StopSampleQueueProperty = DependencyProperty.Register("StopSampleQueue", typeof(bool), typeof(CarouselRunControl), new PropertyMetadata(StopSampleQueueCallBack));
        }

        #endregion

        #region Static Properties

        public static CarouselRunControl Instance { get; set; }

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

        protected override void txtPosition_TextChanged(object sender, TextChangedEventArgs e)
        {
            SelectedPositions = _txtPosition.Text;
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty StopSampleQueueProperty; // this doesn't have a local getter/setter property
        
        private static void ChangesCarouselCreationPropertyValueChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) { }
        private static void ChangesDialogResultPropertyValueChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) { }
        private static void StopSampleQueueCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e) { }

        private static void ClearingSampleQueueCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var carousel = (CarouselRunControl)d;
            if (!carousel.IsClearing)
                return;
            var buttonList = carousel.Template.FindName("CirclePanelButton", carousel) as CirclePanelControl;
            if (buttonList == null)
                return;
            foreach (var item in buttonList.Children)
            {
                var button = item as Button;
                if (button == null)
                    return;
                button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF000000"));
            }
        }

        private static void PlayItemPositionValueChangeCallback(CarouselRunControl carousel, object oldValue, object newValue)
        {
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

            if (oldPosition < newPosition)
            {
                incrementToRotatePosition = newPosition - oldPosition;
            }
            else if (oldPosition > newPosition)
            {
                incrementToRotatePosition = (24 - (oldPosition - newPosition));
            }

            for (var i = 0; i < incrementToRotatePosition; i++)
            {
                carousel.RotateGrid(labelList);
            }

            foreach (var item in buttonList.Children)
            {
                if (!(item is Button button))
                    return;

                if (button.Tag.ToString().Equals(newPosition.ToString()))
                {
                    button.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFF9B3"));
                }
                else
                {
                    button.BorderBrush = new SolidColorBrush(Colors.Transparent);
                }

                button.Background = Brushes.Black;
            }
        }

        #region UpdateCurrentPositionAfterSwitch

        public bool UpdateCurrentPositionAfterSwitch
        {
            get { return (bool)GetValue(UpdateCurrentPositionAfterSwitchProperty); }
            set { SetValue(UpdateCurrentPositionAfterSwitchProperty, value); }
        }

        public static readonly DependencyProperty UpdateCurrentPositionAfterSwitchProperty = DependencyProperty.Register(nameof(UpdateCurrentPositionAfterSwitch), typeof(bool),
            typeof(CarouselRunControl), new PropertyMetadata((s, e) => OnUpdateCurrentPositionAfterSwitch(s as CarouselRunControl)));

        private static void OnUpdateCurrentPositionAfterSwitch(CarouselRunControl carouselPlateControl)
        {
            var carousel = carouselPlateControl;

            var buttonList = carousel.Template.FindName("CirclePanelButton", carousel) as CirclePanelControl;
            if (buttonList == null)
                return;

            var labelList = carousel.Template.FindName("CirclePanelLabel", carousel) as CirclePanelControl;
            if (labelList == null)
                return;

            foreach (var item in buttonList.Children)
            {
                var button = item as Button;
                if (button == null)
                    return;

                button.Background = Brushes.Black;
            }
        }

        #endregion

        #region IsRotateButtonEnable

        public bool IsRotateButtonEnable
        {
            get { return (bool)GetValue(IsRotateButtonEnableProperty); }
            set { SetValue(IsRotateButtonEnableProperty, value); }
        }

        public static readonly DependencyProperty IsRotateButtonEnableProperty =
            DependencyProperty.Register(nameof(IsRotateButtonEnable), typeof(bool), typeof(CarouselRunControl), new PropertyMetadata(false));

        #endregion

        #region ChangesCarouselCreation

        public bool ChangesCarouselCreation
        {
            get { return (bool)GetValue(ChangesCarouselCreationProperty); }
            set { SetValue(ChangesCarouselCreationProperty, value); }
        }

        public static readonly DependencyProperty ChangesCarouselCreationProperty;

        #endregion

        #region IsClearing

        public bool IsClearing
        {
            get { return (bool)GetValue(IsClearingProperty); }
            set { SetValue(IsClearingProperty, value); }
        }

        public static readonly DependencyProperty IsClearingProperty;

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
            DependencyProperty.Register(nameof(ControlEnable), typeof(bool), typeof(CarouselRunControl), new FrameworkPropertyMetadata());

        #endregion

        #region ClearPausePosition

        public string ClearPausePosition
        {
            get { return (string)GetValue(ClearPausePositionProperty); }
            set { SetValue(ClearPausePositionProperty, value); }
        }

        public static readonly DependencyProperty ClearPausePositionProperty =
            DependencyProperty.Register(nameof(ClearPausePosition), typeof(string), typeof(CarouselRunControl), new PropertyMetadata(ClearPausePositionCallBack));

        private static void ClearPausePositionCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is CarouselRunControl carousel))
                return;

            if (string.IsNullOrEmpty(carousel.ClearPausePosition))
                return;
            
            var position = Convert.ToInt32(carousel.ClearPausePosition);
            if (position == 0)
                return;

            if (!(carousel.Template.FindName(CirclePanelButton, carousel) is CirclePanelControl buttonList))
                return;
            
            foreach (var item in buttonList.Children)
            {
                if (!(item is Button button))
                    return;

                if (Convert.ToInt32(button.Tag).Equals(position))
                    button.Background = Brushes.Gray;
            }
        }

        #endregion

        #region GrayOutPosition

        public int GrayOutPosition
        {
            get { return (int)GetValue(GrayOutPositionProperty); }
            set { SetValue(GrayOutPositionProperty, value); }
        }

        public static readonly DependencyProperty GrayOutPositionProperty =
            DependencyProperty.Register(nameof(GrayOutPosition), typeof(int), typeof(CarouselRunControl), new PropertyMetadata(GrayOutCallBack));

        private static void GrayOutCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is CarouselRunControl carousel))
                return;
            
            if ((carousel.GrayOutPosition.Equals(0)))
                return;
            
            if (!(carousel.Template.FindName(CirclePanelButton, carousel) is CirclePanelControl buttonList))
                return;
            
            foreach (var item in buttonList.Children)
            {
                if (!(item is Button button))
                    return;

                if (Convert.ToInt32(button.Tag).Equals(carousel.GrayOutPosition))
                    button.Background = Brushes.Gray;
            }
        }

        #endregion

        #region IsCarouselColorUpdated

        public bool IsCarouselColorUpdated
        {
            get { return (bool)GetValue(IsCarouselColorUpdatedProperty); }
            set { SetValue(IsCarouselColorUpdatedProperty, value); }
        }

        public static readonly DependencyProperty IsCarouselColorUpdatedProperty =
            DependencyProperty.Register(nameof(IsCarouselColorUpdated), typeof(bool), typeof(CarouselRunControl), new PropertyMetadata(ColorChangedCallBack));

        private static void ColorChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is CarouselRunControl carousel))
                return;
            
            if (!carousel.IsCarouselColorUpdated)
                return;

            var buttonList = carousel.Template.FindName(CirclePanelButton, carousel) as CirclePanelControl;
        }

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

        public static readonly DependencyProperty MyItemsSourceProperty;

        #endregion

        #endregion

        #region Dependency Property Commands

        public ICommand RotateGridCommand
        {
            get { return (ICommand) GetValue(RotateGridCommandProperty); }
            set { SetValue(RotateGridCommandProperty, value); }
        }

        public static readonly DependencyProperty RotateGridCommandProperty =
            DependencyProperty.Register(nameof(RotateGridCommand), typeof(ICommand), typeof(CarouselRunControl), new PropertyMetadata(null));

        #endregion

        #region Public Methods

        public void RotateLabelCounterWise(CirclePanelControl circlePanelControlObjParam)
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

        private void CheckDefinedSampleList()
        {
            ConsecutiveItemList = new List<int>();
            DefinedSelectedSample = new StringBuilder();
            if (CirclePanelControl.ApiList == null)
                return;
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

        private void UpdateDefinedSelectedSample(List<int> consecutiveItemListParam, int countValue)
        {
            if (consecutiveItemListParam.Count != 0)
            {
                consecutiveItemListParam.Add(CirclePanelControl.ApiList[countValue]);
                if (DefinedSelectedSample.Length == 0)
                {
                    DefinedSelectedSample.Append(consecutiveItemListParam.Min(x => x) + "-" + consecutiveItemListParam.Max(x => x));
                }
                else
                {
                    DefinedSelectedSample.Append("," + consecutiveItemListParam.Min(x => x) + "-" + consecutiveItemListParam.Max(x => x));
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
            if (HighLightedSample == 24)
            {
                HighLightedSample = 1;
            }
            else
            {
                HighLightedSample++;
            }

            if (!(Template.FindName(CirclePanelButton, this) is CirclePanelControl circle))
                return;

            foreach (var child in circle.Children)
            {
                if (child == null || !(child is Button button))
                    continue;
                    
                if (button.Tag.ToString().Equals(HighLightedSample.ToString()))
                    button.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFF9B3"));
                else
                    button.BorderBrush = new SolidColorBrush(Colors.Transparent);
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