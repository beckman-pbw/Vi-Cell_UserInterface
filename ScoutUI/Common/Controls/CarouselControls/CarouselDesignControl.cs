using ScoutDomains.Common;
using ScoutUtilities.Enums;
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
    public class CarouselDesignControl : BaseCarouselControl
    {
        #region Constructor

        static CarouselDesignControl()
        {
            MyItemsSourceProperty = DependencyProperty.Register(nameof(MyItemsSource), typeof(List<SampleDomain>), typeof(CarouselDesignControl));
            ChangesDialogResultProperty = DependencyProperty.Register(nameof(ChangesDialogResult), typeof(bool), typeof(CarouselDesignControl),
                new PropertyMetadata(ChangesDialogResultPropertyCallback));
            HighlightedSampleProperty = DependencyProperty.Register(nameof(HighLightedSample), typeof(int), typeof(CarouselDesignControl));
            PlayItemPositionProperty = DependencyProperty.Register(nameof(PlayItemPosition), typeof(int), typeof(CarouselDesignControl),
                new PropertyMetadata(PlayItemPositionValueChangeCallback));
            IsClearingProperty = DependencyProperty.Register(nameof(IsClearing), typeof(bool), typeof(CarouselDesignControl), new PropertyMetadata(ClearingSampleQueueCallBack));
            ChangesCarouselCreationProperty = DependencyProperty.Register(nameof(ChangesCarouselCreation), typeof(bool), typeof(CarouselDesignControl),
                new PropertyMetadata(ChangesCarouselCreationPropertyCallback));
            SelectCarouselPositionProperty = DependencyProperty.Register(nameof(SelectCarouselPosition), typeof(bool), typeof(CarouselDesignControl),
                new PropertyMetadata(SelectCarouselPositionPropertyCallback));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CarouselDesignControl), new FrameworkPropertyMetadata(typeof(CarouselDesignControl)));
            ChangesEditCarouselCreationProperty = DependencyProperty.Register(nameof(ChangesEditCarouselCreation), typeof(bool), typeof(CarouselDesignControl),
                new PropertyMetadata(ChangesEditCarouselCreationPropertyCallback));

            StopSampleQueueProperty = DependencyProperty.Register("StopSampleQueue", typeof(bool), typeof(CarouselDesignControl), new PropertyMetadata(StopSampleQueueCallBack));
        }

        public CarouselDesignControl()
        {
            ColorsCount = 1;
            DefinedSelectedSample = new StringBuilder();
            SetGridRotateAngle = 15;
            CheckDefinedSampleList();
            AllowDrop = true;
            ButtonTagList = new List<int>()
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24
            };

            Instance = this;
            DataContext = this;
        }

        #endregion

        #region Properties & Fields

        public int ColorsCount { get; set; }
        public bool IsMouseClickInsideButton { get; set; }
        public List<int> ButtonTagList { get; set; }
        public Button SelectedCarouselButton { get; set; }

        #region Static Properties

        public static CarouselDesignControl Instance { get; set; }

        private static string _selectedPositions;
        public static string SelectedPositions
        {
            get { return _selectedPositions; }
            set
            {
                if (value != _selectedPositions)
                {
                    _selectedPositions = value;
                    NotifyStaticPropertyChanged("SelectedPositions");
                }
            }
        }

        #endregion

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty StopSampleQueueProperty;

        private static void SelectCarouselPositionPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is CarouselDesignControl carousel))
                return;
            if (!(carousel.Template.FindName(CirclePanelButton, carousel) is CarouselDesignButtonControl buttonList))
                return;

            carousel._txtPosition.Text = string.Empty;
            SelectedPositions = string.Empty;
            var selectedList = carousel.MyItemsSource.Where(p => p.SampleStatusColor.Equals(SampleStatusColor.Selected)).ToList();
            CarouselDesignButtonControl.ApiList = new List<int>();

            foreach (var item in buttonList.Children)
            {
                if (!(item is Button button))
                    return;

                if (selectedList.Any(p => Convert.ToInt32(p.SamplePosition.Column) == Convert.ToInt32(button.Tag)
                                          && (!p.SampleID.Equals("(Empty)") || !string.IsNullOrEmpty(p.SampleID))))
                {
                    CarouselDesignButtonControl.ApiList.Add(Convert.ToInt32(button.Tag));
                    carousel.CheckDefinedSampleList();
                    button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Selected)));
                    button.IsEnabled = true;
                }
                else
                {
                    button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Empty)));
                    button.IsEnabled = false;
                }
            }
        }

        private static void ChangesEditCarouselCreationPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) { }

        private static void ChangesCarouselCreationPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var carousel = (CarouselDesignControl)d;
            if (!carousel.ChangesCarouselCreation)
                return;

            if (carousel.MyItemsSource == null)
                return;

            var selectedList = carousel.MyItemsSource.Where(p => p.SampleStatusColor.Equals(SampleStatusColor.Defined)).ToList();
            var buttonList = carousel.Template.FindName(CirclePanelButton, carousel) as CarouselDesignButtonControl;
            if (buttonList == null)
                return;

            {
                foreach (var item in buttonList.Children)
                {
                    var button = item as Button;
                    if (button == null)
                        return;
                    if (selectedList.Count == 0)
                    {
                        button.Background =
                            (SolidColorBrush)(new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Empty)));
                        button.IsEnabled = true;
                    }
                    else
                    {
                        if (selectedList.Any(
                            p => Convert.ToInt32(p.SamplePosition.Column) == Convert.ToInt32(button.Tag) && (
                                     !p.SampleID.Equals("(Empty)") || !string.IsNullOrEmpty(p.SampleID))))
                        {
                            button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Defined)));
                            button.IsEnabled = true;
                        }
                        else
                        {
                            button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Empty)));
                            button.IsEnabled = true;
                        }
                    }
                }
            }
            if (!carousel.IsClearFromEdit)
                return;
            carousel.CheckDefinedSampleListForEdit(carousel);
        }

        private static void ChangesDialogResultPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var carousel = (CarouselDesignControl)d;
            if (!carousel.ChangesDialogResult)
                return;

            if (carousel.MyItemsSource == null)
                return;

            var selectedList = carousel.MyItemsSource.Where(p => p.SampleStatusColor.Equals(SampleStatusColor.Defined)).ToList();
            var buttonList = carousel.Template.FindName(CirclePanelButton, carousel) as CarouselDesignButtonControl;
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
            }
        }

        private static void PlayItemPositionValueChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var carousel = d as CarouselDesignControl;
            if (carousel == null)
                return;

            if (carousel.PlayItemPosition <= 0)
                return;

            var buttonList = carousel.Template.FindName(CirclePanelButton, carousel) as CarouselDesignButtonControl;
            if (buttonList == null)
                return;

            var labelList = carousel.Template.FindName(CirclePanelLabel, carousel) as CarouselDesignButtonControl;
            if (labelList == null)
                return;

            var newPosition = carousel.PlayItemPosition;
            var oldPosition = carousel.HighLightedSample;
            var currentPosition = 0;

            if (oldPosition < newPosition)
            {
                currentPosition = newPosition - oldPosition;
            }
            else if (oldPosition > newPosition)
            {
                currentPosition = 24 - oldPosition + newPosition;
            }

            for (var i = 0; i < currentPosition; i++)
            {
                carousel.RotateGrid(labelList);
            }

            carousel.SetButtonSelectionColor(buttonList);
        }

        private static void StopSampleQueueCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var carousel = (CarouselDesignControl)d;
            var buttonList = carousel.Template.FindName(CirclePanelButton, carousel) as CarouselDesignButtonControl;
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
            var carousel = (CarouselDesignControl)d;
            if (!carousel.IsClearing)
                return;

            var buttonList = carousel.Template.FindName(CirclePanelButton, carousel) as CarouselDesignButtonControl;
            if (buttonList == null)
                return;

            carousel._txtPosition.Text = string.Empty;
            SelectedPositions = string.Empty;
            CarouselDesignButtonControl.ApiList = new List<int>();
            foreach (var item in buttonList.Children)
            {
                if (!(item is Button button))
                    continue;

                button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Empty)));
                button.IsEnabled = true;
            }
        }

        #region CarouselButtonPosition

        public int CarouselButtonPosition
        {
            get { return (int)GetValue(CarouselButtonPositionProperty); }
            set { SetValue(CarouselButtonPositionProperty, value); }
        }

        public static readonly DependencyProperty CarouselButtonPositionProperty = DependencyProperty.Register(nameof(CarouselButtonPosition), typeof(int),
            typeof(CarouselDesignControl), new PropertyMetadata(CarouselButtonColorChangedCallBack));

        private static void CarouselButtonColorChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is CarouselDesignControl carousel) || carousel.CarouselButtonPosition.Equals(0))
                return;
            if (!(carousel.Template.FindName(CirclePanelButton, carousel) is CarouselDesignButtonControl buttonList))
                return;

            var selectedItem = carousel.MyItemsSource.FirstOrDefault(a => a.SamplePosition.Column.ToString().Equals(carousel.CarouselButtonPosition.ToString()));

            foreach (var item in buttonList.Children)
            {
                if (!(item is Button button) || selectedItem == null || !button.Tag.ToString().Equals(selectedItem.SamplePosition.Column.ToString()))
                    continue;

                if (selectedItem.SampleStatusColor.Equals(SampleStatusColor.Empty))
                {
                    button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Empty)));
                }
                else
                {
                    button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Defined)));
                }
            }
        }

        #endregion

        #region ChangesEditCarouselCreation

        public bool ChangesEditCarouselCreation
        {
            get { return (bool)GetValue(ChangesEditCarouselCreationProperty); }
            set { SetValue(ChangesEditCarouselCreationProperty, value); }
        }

        public static readonly DependencyProperty ChangesEditCarouselCreationProperty;

        #endregion

        #region ChangesCarouselCreation

        public bool ChangesCarouselCreation
        {
            get { return (bool)GetValue(ChangesCarouselCreationProperty); }
            set { SetValue(ChangesCarouselCreationProperty, value); }
        }

        public static readonly DependencyProperty ChangesCarouselCreationProperty;

        #endregion

        #region SelectCarouselPosition

        public bool SelectCarouselPosition
        {
            get { return (bool)GetValue(SelectCarouselPositionProperty); }
            set { SetValue(SelectCarouselPositionProperty, value); }
        }

        public static readonly DependencyProperty SelectCarouselPositionProperty;

        #endregion

        #region Positions

        public string Positions
        {
            get { return (string)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(nameof(Positions), typeof(string), typeof(CarouselDesignControl), new PropertyMetadata(PositionsChangedPropertyCallback));

        private static void PositionsChangedPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var circlePanelControl = d as CarouselDesignButtonControl;
            circlePanelControl?.SetValue(PositionProperty, e.NewValue);
        }

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
            get { return ChangesDialogResultProperty != null && (bool)GetValue(ChangesDialogResultProperty); }
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
            get { return ControlEnableProperty != null && (bool)GetValue(ControlEnableProperty); }
            set { SetValue(ControlEnableProperty, value); }
        }

        public static readonly DependencyProperty ControlEnableProperty =
            DependencyProperty.Register(nameof(ControlEnable), typeof(bool), typeof(CarouselDesignControl), new FrameworkPropertyMetadata());

        #endregion

        #region IsClearing

        public bool IsClearing
        {
            get { return IsClearingProperty != null && (bool)GetValue(IsClearingProperty); }
            set { SetValue(IsClearingProperty, value); }
        }

        public static readonly DependencyProperty IsClearingProperty;

        #endregion

        #region IsClearFromEdit

        public bool IsClearFromEdit
        {
            get { return IsClearFromEditProperty != null && (bool)GetValue(IsClearFromEditProperty); }
            set { SetValue(IsClearFromEditProperty, value); }
        }

        public static readonly DependencyProperty IsClearFromEditProperty =
            DependencyProperty.Register(nameof(IsClearFromEdit), typeof(bool), typeof(CarouselDesignControl), new PropertyMetadata(ClearFromEditCallBack));

        private static void ClearFromEditCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e) { }

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

        #region Event Handlers

        public void OnCarouselDesignButtonPressUp(object sender, MouseButtonEventArgs e)
        {
            IsMouseClickInsideButton = false;
            SelectedCarouselButton = sender as Button;
        }

        public void OnCarouselDesignButtonPressDown(object sender, MouseButtonEventArgs e)
        {
            IsMouseClickInsideButton = true;
        }

        #endregion

        #region Public Methods

        public void CheckDefinedSampleList()
        {
            if (CarouselDesignButtonControl.ApiList == null)
                return;

            CarouselDesignButtonControl.ApiList.Sort();
            ConsecutiveItemList = new List<int>();
            DefinedSelectedSample = new StringBuilder();

            for (var i = 0; i < CarouselDesignButtonControl.ApiList.Count; i++)
            {
                if (CarouselDesignButtonControl.ApiList.Count != i + 1)
                {
                    if (CarouselDesignButtonControl.ApiList[i + 1] - CarouselDesignButtonControl.ApiList[i] == 1)
                    {
                        ConsecutiveItemList.Add(CarouselDesignButtonControl.ApiList[i]);
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

        public void RotateLabelCounterWise(CarouselDesignButtonControl circlePanelControlObjParam)
        {
            var counterRotate = new RotateTransform(-SetGridRotateAngle);
            foreach (var child in circlePanelControlObjParam.Children)
            {
                if (!(child is Label label))
                    return;
                label.RenderTransform = counterRotate;
                label.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
                yield break;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T dependencyObject)
                {
                    yield return dependencyObject;
                }

                foreach (var childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }

        #endregion

        #region Protected Methods

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
            var circlePanelControlObj = objGrid as CarouselDesignButtonControl;
            NotifyPropertyChanged(nameof(SetGridRotateAngle));
            RotateLabelCounterWise(circlePanelControlObj);
            HighLightButtonOnMark();
        }

        private void UpdateDefinedSelectedSample(List<int> consecutiveItemListParam, int countValue)
        {
            if (consecutiveItemListParam != null && consecutiveItemListParam.Count != 0)
            {
                consecutiveItemListParam.Add(CarouselDesignButtonControl.ApiList[countValue]);
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
                    DefinedSelectedSample.Append(CarouselDesignButtonControl.ApiList[countValue]);
                }
                else
                {
                    DefinedSelectedSample.Append("," + CarouselDesignButtonControl.ApiList[countValue]);
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

            if (!(Template.FindName(CirclePanelButton, this) is CarouselDesignButtonControl circle))
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

        private void SetButtonSelectionColor(CarouselDesignButtonControl carousel)
        {
            if (MyItemsSource == null)
                return;

            var selectedList = MyItemsSource.Where(p => p.SampleStatusColor.Equals(SampleStatusColor.Defined)).ToList();
            foreach (var item in carousel.Children)
            {
                if (!(item is Button button))
                    return;

                if (selectedList.Any(p => Convert.ToInt32(p.SamplePosition.Column) == Convert.ToInt32(button.Tag) &&
                                          (!p.SampleID.Equals("(Empty)") || !string.IsNullOrEmpty(p.SampleID))))
                {
                    button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Selected)));
                }
                else
                {
                    button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription(SampleStatusColor.Empty)));
                }
            }
        }

        private void CheckDefinedSampleListForEdit(CarouselDesignControl carousel)
        {
            CarouselDesignButtonControl.ApiList = new List<int>();
            var item = carousel.MyItemsSource.Where(p => p.SampleStatusColor.Equals(SampleStatusColor.Defined)).Select(a => a.SamplePosition.Column).ToList();

            if (item.Count == 0)
            {
                carousel._txtPosition.Text = string.Empty;
                SelectedPositions = string.Empty;
                CarouselDesignButtonControl.ApiList = new List<int>();
            }
            else
            {
                item.ForEach(a => { CarouselDesignButtonControl.ApiList.Add(Convert.ToInt32(a)); });
                ConsecutiveItemList = new List<int>();
                DefinedSelectedSample = new StringBuilder();
                CarouselDesignButtonControl.ApiList.Sort();
                for (var i = 0; i < CarouselDesignButtonControl.ApiList.Count; i++)
                {
                    if (CarouselDesignButtonControl.ApiList.Count != i + 1)
                    {
                        if (CarouselDesignButtonControl.ApiList[i + 1] - CarouselDesignButtonControl.ApiList[i] == 1)
                        {
                            ConsecutiveItemList.Add(CarouselDesignButtonControl.ApiList[i]);
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