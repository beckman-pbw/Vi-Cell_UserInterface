using ScoutDomains;
using ScoutDomains.ClusterDomain;
using ScoutLanguageResources;
using ScoutModels.Service;
using ScoutUI.Views.CommonControls;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using ScoutViewModels.Common.Helper;
using ScoutViewModels.ViewModels.CellTypes;
using ScoutViewModels.ViewModels.Common;
using ScoutViewModels.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ScoutDomains.DataTransferObjects;
using Point = System.Windows.Point;

namespace ScoutUI.Views.ucCommon
{
    public partial class ImageView : BaseImageView
    {
        #region Constructor

        public ImageView()
        {
            InitializeComponent();
            Initialize();
            OnUpdateChartVisibility(this, false);
            BtnImageFitToWindow.Visibility = Visibility.Collapsed;
            LblBubble.Visibility = Visibility.Collapsed;
            ShowSlideShowButtons = true;
            EnableDisablePagination(false);
            BindedImage.Focusable = true;
            Keyboard.Focus(BindedImage);
        }

        private void Initialize()
        {
            IsValidate = true;
            UcHorizontalPaginationView.Visibility = Visibility.Hidden;
            ImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            ImageScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            UcHorizontalPaginationView.DataContext = this;
            ComboBoxImageType.ItemsSource = GetImageTypes();
            SelectedImageType = GetImageTypes().FirstOrDefault();
        }

        #endregion

        #region Properties & Fields

        public static readonly object SyncLockImageList = new object();
        public static readonly object SyncLockTraversal = new object();
        private static readonly object SelectedImagePropertyCallbackLock = new object();

        #endregion

        #region Commands

        protected override void Traversal(object parameter)
        {
            lock (SyncLockTraversal)
            {
                switch (parameter.ToString())
                {
                    case "Left":
                        AdjustState = AdjustValue.Left;
                        break;
                    case "Right":
                        AdjustState = AdjustValue.Right;
                        break;
                }
            }
        }

        protected override void OnTapImage()
        {
            if (DataContext is CreateCellViewModel cellViewModel)
            {
                cellViewModel.ReviewViewModel.LastTappedPixel = ImageService.GetClickLocation(BindedImage, Mouse.GetPosition(BindedImage));
                OnTapImage(cellViewModel.ReviewViewModel.SelectedImageType, cellViewModel.ReviewViewModel.LastSelectedBlob);
            }
            else if (DataContext is ImageViewModel imageViewModel)
            {
                imageViewModel.LastTappedPixel = ImageService.GetClickLocation(BindedImage, Mouse.GetPosition(BindedImage));
                OnTapImage(imageViewModel.SelectedImageType, imageViewModel.LastSelectedBlob);
            }
            else if (DataContext is SampleResultDialogViewModel sampleResultDialogVm)
            {
                sampleResultDialogVm.LastTappedPixel = ImageService.GetClickLocation(BindedImage, Mouse.GetPosition(BindedImage));
                OnTapImage(sampleResultDialogVm.SelectedImageType, sampleResultDialogVm.LastSelectedBlob);
            }
        }

        private void OnTapImage(ImageType imageType, BlobMeasurementDomain blob)
        {
            if (!imageType.Equals(ImageType.Annotated) || Misc.IsHistogramEnable)
            {
                PopupEnable = false;
                return;
            }

            if (blob != null)
            {
                var inverseScalingX = BindedImage.Source.Width / BindedImage.Width;
                var inverseScalingY = BindedImage.Source.Height / BindedImage.Height;
                var x = blob.Coordinates.X / inverseScalingX;
                var y = blob.Coordinates.Y / inverseScalingY;

                AnnotatedPopup.HorizontalOffset = x - Rectangle.Width / 2;
                AnnotatedPopup.VerticalOffset = y + Rectangle.Width / 2;
                
                var mousePos = Mouse.GetPosition(Application.Current.MainWindow);
                
                if (mousePos.Y > (ApplicationConstants.WindowHeight / 2))
                {
                    //The user tapped somewhere in the bottom half of the screen.
                    //Move the popup above the rectangle so it isn't shifted upward blocking the cell if it's too close to the bottom.
                    AnnotatedPopup.VerticalOffset = y - Rectangle.Width / 2;
                    AnnotatedPopup.Placement = PlacementMode.Top;
                }
                else
                {
                    //Offline - the top left corner of the popup is at the bottom left corner of the rectangle
                    //Instrument - the top right corner of the popup is at the bottom left corner of the rectangle
                    AnnotatedPopup.VerticalOffset = y + Rectangle.Width / 2;
                    AnnotatedPopup.Placement = PlacementMode.Relative;
                }
                
                Canvas.SetLeft(Rectangle, x - Rectangle.Width / 2);
                Canvas.SetTop(Rectangle, y - Rectangle.Width / 2);

                PopupEnable = true;
            }
        }

        #endregion

        #region Dependency Properties

        #region Expand Command

        public ICommand ExpandCommand
        {
            get { return (ICommand)GetValue(ExpandCommandProperty); }
            set { SetValue(ExpandCommandProperty, value); }
        }

        public static readonly DependencyProperty ExpandCommandProperty = DependencyProperty.Register("ExpandCommand",
            typeof(ICommand), typeof(ImageView), new PropertyMetadata(null));

        #endregion

        #region Play SlideShow Command

        public ICommand PlaySlideShowCommand
        {
            get { return (ICommand)GetValue(PlaySlideShowCommandProperty); }
            set { SetValue(PlaySlideShowCommandProperty, value); }
        }

        public static readonly DependencyProperty PlaySlideShowCommandProperty = DependencyProperty.Register(
            nameof(PlaySlideShowCommand), typeof(ICommand), typeof(ImageView),
            new PropertyMetadata(null));

        #endregion

        #region Pause SlideShow Command

        public ICommand PauseSlideShowCommand
        {
            get { return (ICommand)GetValue(PauseSlideShowCommandProperty); }
            set { SetValue(PauseSlideShowCommandProperty, value); }
        }

        public static readonly DependencyProperty PauseSlideShowCommandProperty = DependencyProperty.Register(
            nameof(PauseSlideShowCommand), typeof(ICommand), typeof(ImageView),
            new PropertyMetadata(null));

        #endregion

        #region Image Page Controls Visibility

        public Visibility ImageViewPageControlsVisibility
        {
            get { return (Visibility) GetValue(ImageViewPageControlsVisibilityProperty); }
            set { SetValue(ImageViewPageControlsVisibilityProperty, value); }
        }

        public static readonly DependencyProperty ImageViewPageControlsVisibilityProperty = DependencyProperty.Register(
            nameof(ImageViewPageControlsVisibility), typeof(Visibility), typeof(ImageView), new PropertyMetadata(Visibility.Visible));

        #endregion

        #region Is Image Type Available

        public bool IsImageTypeAvailable
        {
            get { return (bool)GetValue(IsImageTypeAvailableProperty); }
            set { SetValue(IsImageTypeAvailableProperty, value); }
        }

        public static readonly DependencyProperty IsImageTypeAvailableProperty = DependencyProperty.Register(
            "IsImageTypeAvailable", typeof(bool), typeof(ImageView), new PropertyMetadata(true));

        #endregion

        #region Is Image Type Enabled

        public bool IsImageTypeEnable
        {
            get { return (bool)GetValue(IsImageTypeEnableProperty); }
            set { SetValue(IsImageTypeEnableProperty, value); }
        }

        public static readonly DependencyProperty IsImageTypeEnableProperty = DependencyProperty.Register(
            "IsImageTypeEnable", typeof(bool), typeof(ImageView), new PropertyMetadata(true));

        #endregion

        #region Adjust State

        public AdjustValue AdjustState
        {
            get { return (AdjustValue)GetValue(AdjustStateProperty); }
            set { SetValue(AdjustStateProperty, value); }
        }

        public static readonly DependencyProperty AdjustStateProperty = DependencyProperty.Register("AdjustState",
            typeof(AdjustValue), typeof(ImageView), new PropertyMetadata(null));

        #endregion

        #region Image List

        public ObservableCollection<SampleImageRecordDomain> ImageList
        {
            get { return (ObservableCollection<SampleImageRecordDomain>)GetValue(ImageListProperty); }
            set { SetValue(ImageListProperty, value); }
        }

        public static readonly DependencyProperty ImageListProperty = DependencyProperty.Register("ImageList",
            typeof(ObservableCollection<SampleImageRecordDomain>), typeof(ImageView));

        #endregion

        #region Annotated Details

        public List<KeyValuePair<string, string>> AnnotatedDetails
        {
            get { return (List<KeyValuePair<string, string>>)GetValue(AnnotatedDetailsProperty); }
            set { SetValue(AnnotatedDetailsProperty, value); }
        }

        public static readonly DependencyProperty AnnotatedDetailsProperty =
            DependencyProperty.Register("AnnotatedDetails", typeof(List<KeyValuePair<string, string>>), typeof(ImageView),
                new PropertyMetadata(null));

        #endregion

        #region Total Image Count

        public int TotalImageCount
        {
            get { return (int)GetValue(TotalImageCountProperty); }
            set { SetValue(TotalImageCountProperty, value); }
        }

        public static readonly DependencyProperty TotalImageCountProperty =
            DependencyProperty.Register("TotalImageCount", typeof(int), typeof(ImageView), new PropertyMetadata(0));

        #endregion

        #region Selected Image Index

        public KeyValuePair<int, string> SelectedImageIndex
        {
            get { return (KeyValuePair<int, string>)GetValue(SelectedImageIndexProperty); }
            set { SetValue(SelectedImageIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedImageIndexProperty =
            DependencyProperty.Register("SelectedImageIndex", typeof(KeyValuePair<int, string>), typeof(ImageView));

        #endregion

        #region Image Index List

        public ObservableCollection<KeyValuePair<int, string>> ImageIndexList
        {
            get { return (ObservableCollection<KeyValuePair<int, string>>)GetValue(ImageIndexListProperty); }
            set { SetValue(ImageIndexListProperty, value); }
        }

        public static readonly DependencyProperty ImageIndexListProperty = DependencyProperty.Register("ImageIndexList",
            typeof(ObservableCollection<KeyValuePair<int, string>>), typeof(ImageView), new PropertyMetadata(null));

        #endregion

        #region Is Live Image On

        public bool IsLiveImageOn
        {
            get { return (bool)GetValue(IsLiveImageOnProperty); }
            set { SetValue(IsLiveImageOnProperty, value); }
        }

        public static readonly DependencyProperty IsLiveImageOnProperty = DependencyProperty.Register("IsLiveImageOn",
            typeof(bool), typeof(ImageView), new PropertyMetadata(false));

        #endregion

        #region Selected Image Type

        public ImageType SelectedImageType
        {
            get { return (ImageType)GetValue(SelectedImageTypeProperty); }
            set { SetValue(SelectedImageTypeProperty, value); }
        }

        public static readonly DependencyProperty SelectedImageTypeProperty = DependencyProperty.Register(
            "SelectedImageType", typeof(ImageType), typeof(ImageView), new PropertyMetadata(null));

        #endregion

        #region Is Context Menu Available

        public bool IsContextMenuAvailable
        {
            get { return (bool)GetValue(IsContextMenuAvailableProperty); }
            set { SetValue(IsContextMenuAvailableProperty, value); }
        }

        public static readonly DependencyProperty IsContextMenuAvailableProperty = DependencyProperty.Register(
            "IsContextMenuAvailable", typeof(bool), typeof(ImageView), new PropertyMetadata(true));

        #endregion

        #region Is Chart Control Visible with Callback

        public bool IsChartControlVisible
        {
            get { return (bool)GetValue(IsChartControlVisibleProperty); }
            set { SetValue(IsChartControlVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsChartControlVisibleProperty =
            DependencyProperty.Register("IsChartControlVisible", typeof(bool), typeof(ImageView),
                new PropertyMetadata(false, (sender, e) =>
                    OnUpdateChartVisibility(sender as ImageView, (bool)e.NewValue)));

        private static void OnUpdateChartVisibility(ImageView imageView, bool eNewValue)
        {
            Misc.IsHistogramEnable = eNewValue;
            imageView.BtnImageFitToWindow.Visibility = imageView.GridChartControl.Visibility =
                eNewValue ? Visibility.Visible : Visibility.Collapsed;
            imageView.GridImageControl.Visibility = eNewValue ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion

        #region Is Horizontal Pagination Visible with Callback

        public bool IsHorizontalPaginationVisible
        {
            get { return (bool)GetValue(IsHorizontalPaginationVisibleProperty); }
            set { SetValue(IsHorizontalPaginationVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsHorizontalPaginationVisibleProperty = DependencyProperty.Register(
            "IsHorizontalPaginationVisible", typeof(bool), typeof(ImageView),
            new PropertyMetadata(HorizontalPaginationCallBack));

        private static void HorizontalPaginationCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var imageView = (ImageView)d;
            imageView.UcHorizontalPaginationView.Visibility = imageView.IsHorizontalPaginationVisible
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        #endregion

        #region Selected Image Service with Callback

        public SampleImageRecordDomain SelectedImageService
        {
            get { return (SampleImageRecordDomain)GetValue(SelectedImageServiceProperty); }
            set { SetValue(SelectedImageServiceProperty, value); }
        }

        public static readonly DependencyProperty SelectedImageServiceProperty = DependencyProperty.Register(
            "SelectedImageService", typeof(SampleImageRecordDomain), typeof(ImageView),
            new PropertyMetadata(OnSelectedImagePropertyCallBack));

        private static void OnSelectedImagePropertyCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var imageView = d as ImageView;
            if (imageView == null)
            {
                return;
            }

            var newSelectedImage = e.NewValue as SampleImageRecordDomain;
            lock (SyncLockImageList)
            {
                if (newSelectedImage?.ImageSet?.BrightfieldImage != null)
                {
                    imageView.chartHisto.ContextMenu.Visibility = imageView.IsLiveImageOn
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                    imageView.BindedImage.ContextMenu.Visibility = imageView.IsLiveImageOn
                        ? Visibility.Collapsed
                        : Visibility.Visible;

                    var imageDto = imageView.SelectedImageService.ImageSet.BrightfieldImage;
                    if (imageDto.ImageSource == null)
                    {
                        Log.Error("UpdatedFinalImage", new NullReferenceException(Environment.StackTrace));
                    }
                    imageView.BindedImage.Source = imageDto.ImageSource;
                    //for Dust ref, live image,set focus ResultPerImage shall be null so ignoring the image process status 
                    imageView.LblBubble.Visibility = Visibility.Collapsed;
                    if (newSelectedImage.ResultPerImage != null)
                        imageView.UpdateImageProcessStatus(newSelectedImage);
                }

                if (imageView.ImageViewPageControlsVisibility.Equals(Visibility.Collapsed))
                {
                    imageView.SetImagePerfectSize();
                }
                else
                {
                    imageView.SetImageSize();
                }
                if (Misc.IsHistogramEnable)
                {
                    imageView.IsChartControlVisible = true;
                    imageView.UpdateHistogramImage();
                }
                else
                {
                    imageView.IsChartControlVisible = false;
                }
            }
        }

        #endregion

        #region Popup Enabled with Callback

        public bool PopupEnable
        {
            get { return (bool)GetValue(PopupEnableProperty); }
            set { SetValue(PopupEnableProperty, value); }
        }

        public static readonly DependencyProperty PopupEnableProperty =
            DependencyProperty.Register("PopupEnable", typeof(bool), typeof(ImageView),
                new PropertyMetadata(OnEnablePopup));

        private static void OnEnablePopup(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageView imageView)
            {
                imageView.AnnotatedPopup.IsOpen = imageView.PopupEnable;
                imageView.Rectangle.Visibility = imageView.PopupEnable ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion

        #region Selected Image with Callback

        public SampleImageRecordDomain SelectedImage
        {
            get { return (SampleImageRecordDomain)GetValue(SelectedImageProperty); }
            set { SetValue(SelectedImageProperty, value); }
        }

        public static readonly DependencyProperty SelectedImageProperty = DependencyProperty.Register("SelectedImage",
            typeof(SampleImageRecordDomain), typeof(ImageView), new PropertyMetadata(SelectedImagePropertyCallBack));

        private static void SelectedImagePropertyCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var imageView = d as ImageView;
            if (imageView?.ImageList == null || e.NewValue == null)
            {
                return;
            }

            imageView.Rectangle.Visibility = Visibility.Collapsed;
            var newSelectedImage = e.NewValue as SampleImageRecordDomain;
            imageView.LblBubble.Visibility = Visibility.Collapsed;

            // This lock does not guarantee the correct order, but it eliminates race-conditions.
            lock (SelectedImagePropertyCallbackLock)
            {
	            if (imageView.SelectedImage?.ImageSet == null)
                {
                    imageView.BindedImage.Source = null;
                    imageView.EnableDisablePagination(false);
                    return;
                }

                imageView.EnableDisablePagination(true); // we allow pagination while running samples (PC3549-3840)

                if (imageView.ImageList.Any())
                {
                    var index = new KeyValuePair<int, string>(Convert.ToInt32(newSelectedImage.SequenceNumber), newSelectedImage.SequenceNumber.ToString());
                    if (!index.Equals(imageView.SelectedImageIndex))
                    {
                        imageView.SelectedImageIndex = index;
                    }

                    //for Dust ref, live image,set focus ResultPerImage shall be null so ignoring the image process status 
                    if (newSelectedImage.ResultPerImage != null)
                    {
                        imageView.UpdateImageProcessStatus(newSelectedImage);
                    }
                }

                if (imageView.ImageList != null)
                {
                    imageView.TotalImageCount = imageView.ImageList.Count;
                }

                if (newSelectedImage?.ImageSet.BrightfieldImage == null)
                {
                    return;
                }
				var imageData = newSelectedImage.ImageSet.BrightfieldImage.ImageSource;
                imageView.SetImage(imageData);

                if (imageView.ImageViewPageControlsVisibility.Equals(Visibility.Collapsed))
                {
                    imageView.SetImagePerfectSize();
                }
                else if (!string.IsNullOrEmpty(imageView.MenuSelectionName) && 
                         imageView.MenuSelectionName.Equals(ApplicationConstants.ImageViewRightClickMenuImageActualSize))
                {
                    imageView.UpdateImageActualSize();
                }
                else
                {
                    imageView.SetImageSize();
                }

                if (Misc.IsHistogramEnable)
                {
                    imageView.IsChartControlVisible = true;
                    imageView.UpdateHistogramImage();
                    imageView.MenuSelectionName = ApplicationConstants.ImageViewRightClickMenuImageFitSize;
                }
                else
                {
                    imageView.IsChartControlVisible = false;
                }
            }
        }

        #endregion

        #region Image View Type with Callback

        public string SelectedRightClickImageType
        {
            get { return (string)GetValue(SelectedRightClickImageTypeProperty); }
            set { SetValue(SelectedRightClickImageTypeProperty, value); }
        }

        public static readonly DependencyProperty SelectedRightClickImageTypeProperty =
            DependencyProperty.Register(nameof(SelectedRightClickImageType), typeof(string), typeof(ImageView),
                new PropertyMetadata(ApplicationConstants.ImageViewRightClickMenuImageFitSize, ImageViewTypeChangeCallback));

        private static void ImageViewTypeChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageView imageView)
            {
                imageView.MenuSelectionName = imageView.SelectedRightClickImageType;
                imageView.OnMenuItemSelectionChanged(imageView.MenuSelectionName);
            }
        }

        #endregion

        #region Is Full Screen Visible with Callback

        public bool IsFullScreenVisible
        {
            get { return (bool)GetValue(IsFullScreenVisibleProperty); }
            set { SetValue(IsFullScreenVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsFullScreenVisibleProperty = DependencyProperty.Register(
            "IsFullScreenVisible", typeof(bool), typeof(ImageView), new PropertyMetadata(FullScreenVisibleCallBack));

        private static void FullScreenVisibleCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ImageView imageView))
                return;

            imageView.BtnExpand.Visibility = imageView.IsFullScreenVisible ? Visibility.Collapsed : Visibility.Visible;
            imageView.SetSize();
        }

        #endregion

        #region Is Pagination Button Enabled with Callback

        public bool IsPaginationButtonEnable
        {
            get { return (bool)GetValue(IsPaginationButtonEnableProperty); }
            set { SetValue(IsPaginationButtonEnableProperty, value); }
        }

        public static readonly DependencyProperty IsPaginationButtonEnableProperty = DependencyProperty.Register(
            "IsPaginationButtonEnable", typeof(bool), typeof(ImageView), new PropertyMetadata(OnPaginationButtonEnable));

        private static void OnPaginationButtonEnable(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageView imageView)
            {
                imageView.EnableDisablePagination(imageView.IsPaginationButtonEnable);
            }
        }

        #endregion

        #region Show Slideshow Buttons

        public bool ShowSlideShowButtons
        {
            get { return (bool)GetValue(ShowSlideShowButtonsProperty); }
            set { SetValue(ShowSlideShowButtonsProperty, value); }
        }

        public static readonly DependencyProperty ShowSlideShowButtonsProperty = DependencyProperty.Register(
            nameof(ShowSlideShowButtons), typeof(bool), typeof(ImageView), new PropertyMetadata(null));

        #endregion

        #endregion

        #region Private Methods

        private IEnumerable<ImageType> GetImageTypes()
        {
            return Enum.GetValues(typeof(ImageType)).Cast<ImageType>();
        }

        private void SetSize()
        {
            if (IsFullScreenVisible)
            {
                BindedImage.Height = 630;
                BindedImage.Width = 1200;
            }
            else
            {
                CanvasImage.Width = CanvasImage.Height = BindedImage.Height = BindedImage.Width = 540;
            }
        }

        private void UpdateImageActualSize()
        {
            ImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            ImageScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            var selectedImage = SelectedImage ?? SelectedImageService;

            double updatedWidth;
            double updatedHeight;

            if (selectedImage?.ImageSet?.BrightfieldImage != null)
            {
                var imageDto = selectedImage.ImageSet.BrightfieldImage;
                BindedImage.Source = imageDto.ImageSource;
                updatedWidth = imageDto.Cols;
                updatedHeight = imageDto.Rows;
            }
            else
            {
                Log.Warn("UpdateImageActualSize : Invalid image name or invalid image data!");
                return;
            }

            BindedImage.Height = updatedHeight;
            BindedImage.Width = updatedWidth;
            CanvasImage.Width = updatedWidth;
            CanvasImage.Height = updatedHeight;
        }

        private void DisableScrollBar()
        {
            ImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            ImageScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }

        private void UpdateHistogramImage()
        {
            var histogramHelper = new HistogramUtilityHelper();
            ObservableCollection<HistogramChartItem> itemCollection;

            if (SelectedImage == null)
            {
                itemCollection = histogramHelper.GetChartItemsWithStream(BindedImage.Source);
            }
            else
            {
                itemCollection = histogramHelper.GetChartItems();
            }

            lineChart.ItemsSource = itemCollection;
        }

        private void MenuItemHandler(HeaderedItemsControl menuItem)
        {
            if (menuItem?.Header is StackPanel header)
            {
                SetChildPathVisibility(header, Visibility.Visible);
            }

            if (!(menuItem?.Parent is ContextMenu par))
                return;

            foreach (var item in par.Items)
            {
                if (!(item is MenuItem mi) || mi == null) 
                    continue;

                if (!mi.Name.Equals(ApplicationConstants.ImageViewRightClickMenuHistogram) &&
                    !mi.Name.Equals(ApplicationConstants.ImageViewRightClickMenuImageActualSize) &&
                    !mi.Name.Equals(ApplicationConstants.ImageViewRightClickMenuImageFitSize))
                {
                    continue;
                }

                if (Equals(mi, menuItem) || !(mi?.Header is StackPanel smiHeader))
                    continue;

                SetChildPathVisibility(smiHeader, Visibility.Hidden);
            }
        }

        private void SetContextMenuItemSelectedToSelect()
        {
            if (BindedImage.ContextMenu == null)
                return;

            foreach (var item in BindedImage.ContextMenu.Items)
            {
                if (!(item is MenuItem menuItem) || !(menuItem?.Header is StackPanel header))
                    return;

                var isVisible = menuItem.Name.Equals(MenuSelectionName) ? Visibility.Visible : Visibility.Hidden;
                SetChildPathVisibility(header, isVisible);
            }
        }

        private void SetImage(BitmapSource imageData)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

			BindedImage.Source = imageData;
        }

        private void SetImageSize()
        {
            BindedImage.Width = CanvasImage.Width = ControlDefaultHeightAndWidth;
            BindedImage.Height = CanvasImage.Height = ControlDefaultHeightAndWidth;
            BindedImage.RenderTransformOrigin = new Point(0, 0);
            BindedImage.HorizontalAlignment = HorizontalAlignment.Stretch;
            BindedImage.VerticalAlignment = VerticalAlignment.Stretch;
            BindedImage.Stretch = Stretch.Fill;
            SetContextMenuItemSelectedToSelect();
        }

        private void SetImagePerfectSize()
        {
            if (GridImageControl.ActualWidth > GridImageControl.ActualHeight)
            {
                BindedImage.Width = CanvasImage.Width = BindedImage.Height = CanvasImage.Height = ControlDefaultHeightAndWidth;
            }
            else
            {
                BindedImage.Height = CanvasImage.Height = BindedImage.Width = CanvasImage.Width = ControlDefaultHeightAndWidth;
            }

            BindedImage.HorizontalAlignment = HorizontalAlignment.Center;
            BindedImage.RenderTransformOrigin = new Point(0, 0);
            BindedImage.HorizontalAlignment = HorizontalAlignment.Stretch;
            BindedImage.VerticalAlignment = VerticalAlignment.Center;
            BindedImage.Stretch = Stretch.Fill;
            SetContextMenuItemSelectedToSelect();
        }

        private void UpdateImageProcessStatus(SampleImageRecordDomain newSelectedImage)
        {
            switch (newSelectedImage.ResultPerImage?.ProcessedStatus)
            {
                case E_ERRORCODE.eSuccess:
                    LblBubble.Visibility = Visibility.Collapsed;
                    break;
                case E_ERRORCODE.eInvalidBackgroundIntensity:
                    UpdateImageProcessMessage(LanguageResourceHelper.Get("LID_eInvalidBackgroundIntensity"));
                    break;
                case E_ERRORCODE.eBubbleImage:
                    UpdateImageProcessMessage(LanguageResourceHelper.Get("LID_eBubbleImage"));
                    break;
                default:
                    UpdateImageProcessMessage(LanguageResourceHelper.Get("LID_ImageProcessingError"));
                    break;
            }
        }
        
        private void UpdateImageProcessMessage(string imageProcessStatusMessage)
        {
            LblBubble.Visibility = Visibility.Visible;
            LblBubble.Content = imageProcessStatusMessage;
        }

        private void EnableDisablePagination(bool isEnable)
        {
            UcHorizontalPaginationView.btnPrevious.IsEnabled = UcHorizontalPaginationView.btnNext.IsEnabled = isEnable;
        }

        #endregion

        #region EventHandlers

        private void BtnImageFitToWindow_OnClick(object sender, RoutedEventArgs e)
        {
            IsChartControlVisible = false;
            SetImageSize();
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            IsChartControlVisible = false;
            if (!(sender is MenuItem menuItem))
                return;

            MenuItemHandler(menuItem);
            SelectedRightClickImageType = menuItem.Name;

            if (!menuItem.Name.Equals(ApplicationConstants.ImageViewRightClickMenuHistogram))
            {
                MenuSelectionName = SelectedRightClickImageType;
            }
           
            OnMenuItemSelectionChanged(menuItem.Name);
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            PopupEnable = false;
        }

        private void ImageScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (ImageScrollViewer.HorizontalScrollBarVisibility.Equals(ScrollBarVisibility.Visible) &&
                ImageScrollViewer.VerticalScrollBarVisibility.Equals(ScrollBarVisibility.Visible))
            {
                PopupEnable = false;
            }
        }

        private void OnMenuItemSelectionChanged(string newMenuItemName)
        {
            switch (newMenuItemName)
            {
                // Set  Histogram
                case ApplicationConstants.ImageViewRightClickMenuHistogram:
                    IsChartControlVisible = true;
                    Misc.IsHistogramEnable = true;
                    UpdateHistogramImage();
                    SelectedRightClickImageType = ApplicationConstants.ImageViewRightClickMenuHistogram;
                    MenuSelectionName = SelectedRightClickImageType;
                    break;

                // Set Image actual size
                case ApplicationConstants.ImageViewRightClickMenuImageActualSize:
                    Misc.IsHistogramEnable = false;
                    UpdateImageActualSize();
                    SelectedRightClickImageType = ApplicationConstants.ImageViewRightClickMenuImageActualSize;
                    MenuSelectionName = SelectedRightClickImageType;
                    break;

                // Set Image fit to window
                case ApplicationConstants.ImageViewRightClickMenuImageFitSize:
                    Misc.IsHistogramEnable = false;
                    SelectedRightClickImageType = ApplicationConstants.ImageViewRightClickMenuImageFitSize;
                    MenuSelectionName = SelectedRightClickImageType;
                    DisableScrollBar();
                    if (SelectedImage == null)
                    {
                        if (SelectedImageService == null)
                            return; //In review first time no sample binded so selectedimage and SelectedImageService both null
                        var imageDto = SelectedImageService.ImageSet.BrightfieldImage;
                        BindedImage.Source = imageDto.ImageSource;
                    }
                    else
                    {
                        SetImage(SelectedImage.ImageSet.BrightfieldImage.ImageSource);
                    }
                    BindedImage.ClipToBounds = true;
                    SetImageSize();
                    break;
            }

        }
        
        #endregion
    }
}
