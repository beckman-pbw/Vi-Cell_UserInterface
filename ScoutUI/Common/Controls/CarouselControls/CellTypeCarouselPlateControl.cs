using ScoutDomains.Common;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ScoutUI.Common.Controls
{
    public class CellTypeCarouselPlateControl : BaseCarouselControl
    {
        #region Constructor

        static CellTypeCarouselPlateControl()
        {
            MyItemsSourceProperty = DependencyProperty.Register(nameof(MyItemsSource), typeof(List<SampleDomain>), typeof(CellTypeCarouselPlateControl));
            
            ChangesDialogResultProperty = DependencyProperty.Register(nameof(ChangesDialogResult), typeof(bool), typeof(CellTypeCarouselPlateControl),
                new PropertyMetadata(ChangesDialogResultPropertyValueChangeCallback));
            
            HighlightedSampleProperty = DependencyProperty.Register(nameof(HighLightedSample), typeof(int), typeof(CellTypeCarouselPlateControl));
            
            PlayItemPositionProperty = DependencyProperty.Register(nameof(PlayItemPosition), typeof(int), typeof(CellTypeCarouselPlateControl),
                new PropertyMetadata((s, e) => PlayItemPositionValueChangeCallback(s as CellTypeCarouselPlateControl, e.OldValue, e.NewValue)));
            
            IsClearingProperty = DependencyProperty.Register(nameof(IsClearing), typeof(bool), typeof(CellTypeCarouselPlateControl), 
                new PropertyMetadata(ClearingSampleQueueCallBack));

            DefaultStyleKeyProperty.OverrideMetadata(typeof(CellTypeCarouselPlateControl), new FrameworkPropertyMetadata(typeof(CellTypeCarouselPlateControl)));

            StopSampleQueueProperty = DependencyProperty.Register("StopSampleQueue", typeof(bool), typeof(CellTypeCarouselPlateControl), 
                new PropertyMetadata(StopSampleQueueCallBack));
        }

        public CellTypeCarouselPlateControl()
        {
            Instance = this;
            DefinedSelectedSample = new StringBuilder();
            SetGridRotateAngle = 15;
            CheckDefinedSampleList();
            DataContext = this;
        }

        #endregion

        #region Static Properties

        public static CellTypeCarouselPlateControl Instance { get; set; }

        private static CellTypeCirclePanelControl _parentControlInstanceButton;
        private static CellTypeCirclePanelControl _parentControlInstanceLabel;

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

        #region Dependency Property

        public static readonly DependencyProperty StopSampleQueueProperty;

        private static void ChangesDialogResultPropertyValueChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var carousel = (CellTypeCarouselPlateControl)d;
            if (!carousel.ChangesDialogResult)
                return;

            if (carousel.MyItemsSource == null)
                return;

            var selectedList = carousel.MyItemsSource.Where(p => p.SampleStatusColor.Equals(SampleStatusColor.Selected)).ToList();
            var concentrationType1List = carousel.MyItemsSource.Where(p =>
                p.SampleStatusColor.Equals(SampleStatusColor.ConcentrationType1) ||
                p.SampleStatusColor.Equals(SampleStatusColor.ConcentrationType2) ||
                p.SampleStatusColor.Equals(SampleStatusColor.ConcentrationType3)).ToList();

            var buttonList = carousel.Template.FindName("CirclePanelButton", carousel) as CellTypeCirclePanelControl;
            if (buttonList == null)
                return;

            foreach (var item in buttonList.Children)
            {
                var button = item as Button;
                if (button == null)
                    return;

                if (selectedList.Any(p =>
                    Convert.ToInt32(p.SamplePosition.Column) == Convert.ToInt32(button.Tag) && (
                        !p.SampleID.Equals("(Empty)") || !string.IsNullOrEmpty(p.SampleID))))
                {
                    button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Selected)));
                }
                else
                {
                    button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Empty)));
                }

                if (concentrationType1List.Any(p =>
                    Convert.ToInt32(p.SamplePosition.Column) == Convert.ToInt32(button.Tag)))
                {
                    var sample = concentrationType1List.FirstOrDefault(
                        p => Convert.ToInt32(p.SamplePosition.Column) == Convert.ToInt32(button.Tag));
                    if (sample != null)
                        button.Background = (SolidColorBrush)(new BrushConverter()
                            .ConvertFrom(GetEnumDescription.GetDescription(sample.SampleStatusColor)));
                }
                else
                {
                    button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Empty)));
                }
            }
        }

        private static void StopSampleQueueCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var carousel = (CellTypeCarouselPlateControl)d;
            if (carousel.MyItemsSource == null)
                return;

            var buttonList = carousel.Template.FindName("CirclePanelButton", carousel) as CellTypeCirclePanelControl;
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

        private static void ClearingSampleQueueCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var carousel = (CellTypeCarouselPlateControl)d;
            if (carousel.MyItemsSource == null)
                return;

            var buttonList = carousel.Template.FindName("CirclePanelButton", carousel) as CellTypeCirclePanelControl;
            if (!carousel.IsClearing)
                return;

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

        private static void PlayItemPositionValueChangeCallback(CellTypeCarouselPlateControl carouselPlateControl, object oldValue, object newValue)
        {
            var buttonList = carouselPlateControl.Template.FindName("CirclePanelButton", carouselPlateControl) as CellTypeCirclePanelControl;
            if (buttonList == null)
                return;

            var labelList = carouselPlateControl.Template.FindName("CirclePanelLabel", carouselPlateControl) as CellTypeCirclePanelControl;
            if (labelList == null)
                return;

            var incrementToRotatePosition = 0;
            var oldPosition = (int)oldValue;
            var newPosition = (int)newValue;
            if (newPosition == -1)
            {
                foreach (var item in buttonList.Children)
                {
                    var button = item as Button;
                    if (button == null)
                        return;

                    button.BorderBrush = new SolidColorBrush(Colors.Transparent);
                }
            }

            if (oldPosition < 0)
                return;

            if (newPosition < 0)
            {
                carouselPlateControl.PlayItemPosition = newPosition = oldPosition;
                return;
            }

            if (carouselPlateControl.PlayItemPosition == 0)
                oldValue = newValue = 1;

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
                carouselPlateControl.RotateGrid(labelList);
            }

            foreach (var item in buttonList.Children)
            {
                var button = item as Button;
                if (button == null)
                    return;
                if (button.Tag.ToString().Equals(newPosition.ToString()))
                    button.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFF9B3"));
                else
                    button.BorderBrush = new SolidColorBrush(Colors.Transparent);
                if (carouselPlateControl.MyItemsSource != null)
                {
                    var sample = carouselPlateControl.MyItemsSource.FirstOrDefault(
                        x => x.SamplePosition.Column.ToString() == button.Tag.ToString());
                    if (sample != null)
                    {
                        if (sample.SampleStatusColor == SampleStatusColor.Selected)
                            button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Selected)));
                        else if (sample.SampleStatusColor == SampleStatusColor.Completed)
                            button.Background = Brushes.Gray;
                        else
                            button.Background = Brushes.Black;
                    }
                }
            }
        }

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
            DependencyProperty.Register(nameof(ControlEnable), typeof(bool), typeof(CellTypeCarouselPlateControl), new FrameworkPropertyMetadata());

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
            get { return (bool) GetValue(IsStatusCompletedProperty); }
            set { SetValue(IsStatusCompletedProperty, value); }
        }

        public static readonly DependencyProperty IsStatusCompletedProperty =
            DependencyProperty.Register(nameof(IsStatusCompleted), typeof(bool), typeof(CellTypeCarouselPlateControl), new PropertyMetadata(StatusCompletedCallBack));

        private static void StatusCompletedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is CellTypeCarouselPlateControl carousel))
                return;
            if (!carousel.IsStatusCompleted)
                return;
            if (carousel.MyItemsSource == null)
                return;
            if (!(carousel.Template.FindName(CirclePanelButton, carousel) is CellTypeCirclePanelControl buttonList))
                return;

            var processedList = carousel.MyItemsSource.Where(p => p.SampleStatusColor.Equals(SampleStatus.Completed)).ToList();
            
            foreach (var child in buttonList.Children)
            {
                if (!(child is Button button))
                    return;

                if (processedList.Any(p => Convert.ToInt32(p.SamplePosition.Column) == Convert.ToInt32(button.Tag)))
                {
                    var sample = processedList.FirstOrDefault(p => Convert.ToInt32(p.SamplePosition.Column) == Convert.ToInt32(button.Tag));
                    if (sample != null)
                    {
                        button.Background = (SolidColorBrush) (new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Defined)));
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Override Methods

        public override void OnApplyTemplate()
        {
            if (_parentControlInstanceButton == null)
            {
                _parentControlInstanceButton = Template.FindName(CirclePanelButton, this) as CellTypeCirclePanelControl;
            }

            if (_parentControlInstanceLabel == null)
            {
                _parentControlInstanceLabel = Template.FindName(CirclePanelLabel, this) as CellTypeCirclePanelControl;
            }

            _txtPosition = Template.FindName("txtPosition", this) as TextBox;
            if (_txtPosition != null)
                _txtPosition.TextChanged += txtPosition_TextChanged;
            base.OnApplyTemplate();
        }

        protected override void txtPosition_TextChanged(object sender, TextChangedEventArgs e)
        {
            SelectedPositions = _txtPosition.Text;
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
            NotifyPropertyChanged(nameof(SetGridRotateAngle));
            var circlePanelControl = objGrid as CellTypeCirclePanelControl;
            RotateLabelCounterWise(circlePanelControl);
            HighLightButtonOnMark();
        }

        private void CheckDefinedSampleList()
        {
            ConsecutiveItemList = new List<int>();
            DefinedSelectedSample = new StringBuilder();
            CellTypeCirclePanelControl.ApiList.Sort();
            for (var i = 0; i < CellTypeCirclePanelControl.ApiList.Count; i++)
            {
                if (CellTypeCirclePanelControl.ApiList.Count != i + 1)
                {
                    if (CellTypeCirclePanelControl.ApiList[i + 1] - CellTypeCirclePanelControl.ApiList[i] == 1)
                    {
                        ConsecutiveItemList.Add(CellTypeCirclePanelControl.ApiList[i]);
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
            if (consecutiveItemListParam.Count != 0)
            {
                consecutiveItemListParam.Add(CellTypeCirclePanelControl.ApiList[countValue]);
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
                    DefinedSelectedSample.Append(CellTypeCirclePanelControl.ApiList[countValue]);
                }
                else
                {
                    DefinedSelectedSample.Append("," + CellTypeCirclePanelControl.ApiList[countValue]);
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

            if (!(Template.FindName("CirclePanelButton", this) is CellTypeCirclePanelControl buttonList))
                return;

            foreach (var item in buttonList.Children)
            {
                if (!(item is Button button))
                    return;

                if (button.Tag.ToString().Equals(HighLightedSample.ToString()))
                    button.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFF9B3"));
                else
                    button.BorderBrush = new SolidColorBrush(Colors.Transparent);
            }
        }

        #endregion

        #region Public Methods

        public void RotateLabelCounterWise(CellTypeCirclePanelControl circlePanelControlObjParam)
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

        #region Static Notify Implementation

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        private static void NotifyStaticPropertyChanged(string propertyName)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}