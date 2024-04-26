using ScoutDomains.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Helper;
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
    public class MotorRegCarouselPlateControl : BaseCarouselControl
    {
        #region Constructor

        static MotorRegCarouselPlateControl()
        {
            MyItemsSourceProperty = DependencyProperty.Register(nameof(MyItemsSource), typeof(List<SampleDomain>), typeof(MotorRegCarouselPlateControl));
            HighlightedSampleProperty = DependencyProperty.Register(nameof(HighLightedSample), typeof(int), typeof(MotorRegCarouselPlateControl));
            PlayItemPositionProperty = DependencyProperty.Register(nameof(PlayItemPosition), typeof(int), typeof(MotorRegCarouselPlateControl),
                new PropertyMetadata((s, e) => PlayItemPositionValueChangeCallback(s as MotorRegCarouselPlateControl, e.OldValue, e.NewValue)));
            IsClearingProperty = DependencyProperty.Register(nameof(IsClearing), typeof(bool), typeof(MotorRegCarouselPlateControl), new PropertyMetadata(ClearingSampleQueueCallBack));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MotorRegCarouselPlateControl), new FrameworkPropertyMetadata(typeof(MotorRegCarouselPlateControl)));
        }

        public MotorRegCarouselPlateControl()
        {
            DefinedSelectedSample = new StringBuilder();
            SetGridRotateAngle = 15;
            CheckDefinedSampleList();
            DataContext = this;
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

        #region Dependency Properties

        private static void PlayItemPositionValueChangeCallback(MotorRegCarouselPlateControl carousel, object oldValue, object newValue)
        {
            if (!(carousel.Template.FindName(CirclePanelButton, carousel) is CirclePanelControl buttonList))
                return;
            if (!(carousel.Template.FindName(CirclePanelLabel, carousel) is CirclePanelControl labelList))
                return;
            if (carousel.PlayItemPosition <= 0)
                return;

            var currentPosition = 0;
            var oldPosition = (int) oldValue;
            var newPosition = (int) newValue;

            if (oldPosition == 0)
            {
                oldPosition = 1;
            }

            if (oldPosition < newPosition)
            {
                currentPosition = newPosition - oldPosition;
            }
            else if (oldPosition > newPosition)
            {
                currentPosition = (24 - (oldPosition - newPosition));
            }

            for (var i = 0; i < currentPosition; i++)
            {
                carousel.RotateGrid(labelList);
            }

            foreach (var item in buttonList.Children)
            {
                if (!(item is Button button))
                    return;

                button.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF000000");
                
                if (button.Tag.ToString().Equals(newPosition.ToString()))
                    button.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFF9B3"));
                else
                    button.BorderBrush = new SolidColorBrush(Colors.Transparent);
            }
        }

        private static void ClearingSampleQueueCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is MotorRegCarouselPlateControl carousel))
                return;
            if (carousel.MyItemsSource == null)
                return;
            if (!carousel.IsClearing)
                return;
            if (!(carousel.Template.FindName("CirclePanelButton", carousel) is CirclePanelControl buttonList))
                return;
            
            foreach (var item in buttonList.Children)
            {
                if (!(item is Button button))
                    return;

                button.Background = (SolidColorBrush) (new BrushConverter().ConvertFrom("#FF000000"));
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

        #region IsNotchVisible

        public bool IsNotchVisible
        {
            get { return (bool)GetValue(IsNotchVisibleProperty); }
            set { SetValue(IsNotchVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsNotchVisibleProperty =
            DependencyProperty.Register(nameof(IsNotchVisible), typeof(bool), typeof(MotorRegCarouselPlateControl));

        #endregion

        #region PosThirteenHighlight

        public bool PosThirteenHighlight
        {
            get { return (bool)GetValue(PosThirteenHighlightProperty); }
            set { SetValue(PosThirteenHighlightProperty, value); }
        }

        public static readonly DependencyProperty PosThirteenHighlightProperty = DependencyProperty.Register(nameof(PosThirteenHighlight), typeof(bool),
            typeof(MotorRegCarouselPlateControl), new PropertyMetadata(OnPosThirteenHighlightCallBack));

        private static void OnPosThirteenHighlightCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MotorRegCarouselPlateControl motorRegControl)
            {
                motorRegControl.SetPositionHighlight();
            }
        }

        private void SetPositionHighlight()
        {
            const int highlightedPosition = 13;
            var buttonList = Template.FindName(CirclePanelButton, this) as CirclePanelControl;
            if (buttonList == null)
                return;

            foreach (var item in buttonList.Children)
            {
                if (!(item is Button button))
                    return;

                if (PosThirteenHighlight)
                {
                    if (button.Tag.ToString().Equals(highlightedPosition.ToString()))
                    {
                        button.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(
                            GetEnumDescription.GetDescription(SampleStatusColor.Selected));
                    }
                }
                else
                {
                    button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF000000"));
                }
            }
        }

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
            DependencyProperty.Register(nameof(ControlEnable), typeof(bool), typeof(MotorRegCarouselPlateControl), new FrameworkPropertyMetadata());

        #endregion

        #region MotorRegType

        public string MotorRegType
        {
            get { return (string)GetValue(MotorRegHighlightPropertyProperty); }
            set { SetValue(MotorRegHighlightPropertyProperty, value); }
        }

        public static readonly DependencyProperty MotorRegHighlightPropertyProperty = DependencyProperty.Register(nameof(MotorRegType), typeof(string), typeof(MotorRegCarouselPlateControl),
            new PropertyMetadata(MotorRegHighlightPropertyCallBack));

        private static void MotorRegHighlightPropertyCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is MotorRegCarouselPlateControl motorRegCarouselPlateControl))
                return;

            var highlightType = EnumUtilities.ConvertStringToEnum(motorRegCarouselPlateControl.MotorRegType);
            switch (highlightType)
            {
                case MotorRegHighlightType.Black:
                    motorRegCarouselPlateControl.SetButtonBackgroundBlack();
                    break;
                case MotorRegHighlightType.Border:
                    motorRegCarouselPlateControl.HighLightButtonOnMark();
                    break;
                case MotorRegHighlightType.Yellow:
                    motorRegCarouselPlateControl.SetSetButtonBackgroundYellow();
                    break;
            }
        }

        #endregion

        #endregion

        #region Private Methods

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

        private void HighLightButtonOnMark()
        {
            var buttonList = Template.FindName("CirclePanelButton", this) as CirclePanelControl;
            if (buttonList == null)
                return;
            var labelList = Template.FindName("CirclePanelLabel", this) as CirclePanelControl;
            if (labelList == null)
                return;
            if (PlayItemPosition <= 0)
                return;

            foreach (var item in buttonList.Children)
            {
                var button = item as Button;
                if (button == null)
                    return;
                button.Background = (SolidColorBrush) new BrushConverter().ConvertFrom("#FF000000");
                if (button.Tag.ToString().Equals(PlayItemPosition.ToString()))
                    button.BorderBrush = (SolidColorBrush) (new BrushConverter().ConvertFrom("#FFFFF9B3"));
                else
                    button.BorderBrush = new SolidColorBrush(Colors.Transparent);
            }
        }

        private void SetButtonBackgroundBlack()
        {
            var buttonList = Template.FindName("CirclePanelButton", this) as CirclePanelControl;
            if (buttonList == null)
                return;
            foreach (var item in buttonList.Children)
            {
                var button = item as Button;
                if (button == null)
                    return;
                button.BorderBrush = Brushes.Transparent;
                button.Background = (SolidColorBrush) new BrushConverter().ConvertFrom("#FF000000");
            }
        }

        private void SetSetButtonBackgroundYellow()
        {
            var buttonList = Template.FindName("CirclePanelButton", this) as CirclePanelControl;
            if (buttonList == null)
                return;
            foreach (var item in buttonList.Children)
            {
                var button = item as Button;
                if (button == null)
                    return;
                button.BorderBrush = new SolidColorBrush(Colors.Transparent);
                //Setting First position 
                if (button.Tag.ToString().Equals("1"))
                    button.Background =
                        (SolidColorBrush) new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Selected));

                else
                    button.Background =
                        (SolidColorBrush) new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Empty));
            }
        }

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

        private void UpdateDefinedSelectedSample(ICollection<int> consecutiveItemListParam, int countValue)
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
