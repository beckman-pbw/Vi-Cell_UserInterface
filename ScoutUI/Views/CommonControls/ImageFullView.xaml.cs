using ApiProxies.Generic;
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

namespace ScoutUI.Views.ucCommon
{
    public partial class ImageFullView : BaseImageView
    {
        #region Constructor

        public ImageFullView()
        {
            InitializeComponent();
            Initialize();
            LblBubble.Visibility = Visibility.Hidden;
        }

        private void Initialize()
        {
            IsValidate = true;
            ImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            ImageScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            ShowSlideShowButtons = true;
            ComboBoxFullImageType.ItemsSource = GetImageTypes();
        }

        #endregion

        #region Properties & Fields

        public bool IsChartVisible
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                OnUpdateChartVisibility(value);
            }
        }

        public int TotalImageCount
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Commands

        protected override void Traversal(object parameter)
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

        protected override void OnTapImage()
        {
            if (DataContext is ImageViewModel imageVm)
            {
                imageVm.LastTappedPixel = ImageService.GetClickLocation(BindedImage, Mouse.GetPosition(BindedImage));
                OnTapImage(imageVm.SelectedImageType, imageVm.LastSelectedBlob);
            }
            else if (DataContext is ExpandedImageGraphViewModel expandVm)
            {
                expandVm.LastTappedPixel = ImageService.GetClickLocation(BindedImage, Mouse.GetPosition(BindedImage));
                OnTapImage(expandVm.SelectedImageType, expandVm.LastSelectedBlob);
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

        #region Control Form

        public string ControlFrom
        {
            get { return (string)GetValue(ControlFromProperty); }
            set { SetValue(ControlFromProperty, value); }
        }

        public static readonly DependencyProperty ControlFromProperty = DependencyProperty.Register("ControlFrom",
            typeof(string), typeof(ImageFullView), new PropertyMetadata(null));

        #endregion

        #region Image List

        public List<SampleImageRecordDomain> ImageList
        {
            get { return (List<SampleImageRecordDomain>)GetValue(ImageListProperty); }
            set { SetValue(ImageListProperty, value); }
        }

        public static readonly DependencyProperty ImageListProperty = DependencyProperty.Register("ImageList",
            typeof(List<SampleImageRecordDomain>), typeof(ImageFullView));

        #endregion

        #region Is Horizontal Pagination Visible

        public bool IsHorizontalPaginationVisible
        {
            get { return (bool)GetValue(IsHorizontalPaginationVisibleProperty); }
            set { SetValue(IsHorizontalPaginationVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsHorizontalPaginationVisibleProperty = DependencyProperty.Register(
            "IsHorizontalPaginationVisible", typeof(bool), typeof(ImageFullView), new PropertyMetadata(false));

        #endregion

        #region Show Slideshow Buttons

        public bool ShowSlideShowButtons
        {
            get { return (bool)GetValue(ShowSlideShowButtonsProperty); }
            set { SetValue(ShowSlideShowButtonsProperty, value); }
        }

        public static readonly DependencyProperty ShowSlideShowButtonsProperty = DependencyProperty.Register(
            nameof(ShowSlideShowButtons), typeof(bool), typeof(ImageFullView),
            new PropertyMetadata(null));

        #endregion

        #region Is Selection Changed

        public bool IsSelectionChanged
        {
            get { return (bool)GetValue(IsSelectionChangedProperty); }
            set { SetValue(IsSelectionChangedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectionChangedProperty =
            DependencyProperty.Register("IsSelectionChanged", typeof(bool), typeof(ImageFullView));

        #endregion

        #region Annotated Details

        public List<KeyValuePair<string, string>> AnnotatedDetails
        {
            get { return (List<KeyValuePair<string, string>>)GetValue(AnnotatedDetailsProperty); }
            set { SetValue(AnnotatedDetailsProperty, value); }
        }

        public static readonly DependencyProperty AnnotatedDetailsProperty =
            DependencyProperty.Register("AnnotatedDetails", typeof(List<KeyValuePair<string, string>>), typeof(ImageFullView), new PropertyMetadata(null));

        #endregion

        #region Image Index List

        public ObservableCollection<KeyValuePair<int, string>> ImageIndexList
        {
            get { return (ObservableCollection<KeyValuePair<int, string>>)GetValue(ImageIndexListProperty); }
            set { SetValue(ImageIndexListProperty, value); }
        }

        public static readonly DependencyProperty ImageIndexListProperty = DependencyProperty.Register("ImageIndexList",
            typeof(ObservableCollection<KeyValuePair<int, string>>), typeof(ImageFullView));

        #endregion

        #region Adjust State

        public AdjustValue AdjustState
        {
            get { return (AdjustValue)GetValue(AdjustStateProperty); }
            set { SetValue(AdjustStateProperty, value); }
        }

        public static readonly DependencyProperty AdjustStateProperty = DependencyProperty.Register("AdjustState",
            typeof(AdjustValue), typeof(ImageFullView), new PropertyMetadata(null));

        #endregion

        #region Selected Image Index

        public KeyValuePair<int, string> SelectedImageIndex
        {
            get { return (KeyValuePair<int, string>)GetValue(SelectedImageIndexProperty); }
            set { SetValue(SelectedImageIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedImageIndexProperty = DependencyProperty.Register(
            "SelectedImageIndex", typeof(KeyValuePair<int, string>), typeof(ImageFullView), new PropertyMetadata(null));

        #endregion

        #region Selected Image Service with Callback

        public SampleImageRecordDomain SelectedImageService
        {
            get { return (SampleImageRecordDomain)GetValue(SelectedImageServiceProperty); }
            set { SetValue(SelectedImageServiceProperty, value); }
        }

        public static readonly DependencyProperty SelectedImageServiceProperty = DependencyProperty.Register(
            "SelectedImageService", typeof(SampleImageRecordDomain), typeof(ImageFullView),
            new PropertyMetadata(OnSelectedImagePropertyCallBack));

        private static void OnSelectedImagePropertyCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ImageFullView imageView))
            {
                return;
            }

            SetImageFromImageDto(imageView, imageView.SelectedImageService);
        }

        #endregion

        #region Popup Enable with Callback

        public bool PopupEnable
        {
            get { return (bool)GetValue(PopupEnableProperty); }
            set { SetValue(PopupEnableProperty, value); }
        }

        public static readonly DependencyProperty PopupEnableProperty =
            DependencyProperty.Register("PopupEnable", typeof(bool), typeof(ImageFullView), new PropertyMetadata(OnEnablePopup));

        private static void OnEnablePopup(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var imageView = d as ImageFullView;
            if (imageView != null)
            {
                imageView.AnnotatedPopup.IsOpen = imageView.PopupEnable;
                imageView.Rectangle.Visibility =
                    imageView.PopupEnable ? Visibility.Visible : Visibility.Collapsed;
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
            typeof(SampleImageRecordDomain), typeof(ImageFullView),
            new PropertyMetadata(SelectedImagePropertyCallBack));

        private static void SelectedImagePropertyCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ImageFullView imageView) || imageView.ImageList == null || !imageView.IsValidate)
            {
                return;
            }

            imageView.TotalImageCount = imageView.ImageIndexList?.Count ?? imageView.ImageList.Count;

            if (imageView.SelectedImage?.ImageSet == null)
            {
	            if (imageView.SelectedImage?.ImageSet?.BrightfieldImage != null)
	            {
		            // try to set the image another way
		            SetImageFromImageDto(imageView, imageView.SelectedImage);
	            }
	            return;
            }

            if (imageView.ImageList.Any())
            {
				// Handle special case for focusing where the SequenceNumber is zero.
	            if (imageView.SelectedImage.SequenceNumber == 0)
	            {
		            imageView.SelectedImage.SequenceNumber++;
	            }

                imageView.SelectedImageIndex = new KeyValuePair<int, string>(Convert.ToInt32(imageView.SelectedImage.SequenceNumber), imageView.SelectedImage.SequenceNumber.ToString());
            }

            var imageData = imageView.SelectedImage.ImageSet.BrightfieldImage.ImageSource;
            
            imageView.SetImage(imageData);
            SetImageView(imageView, imageView.SelectedImage);
        }

        #endregion

        #region Selected Right Click Image Type with Callback

        public string SelectedRightClickImageType
        {
            get { return (string)GetValue(SelectedRightClickImageTypeProperty); }
            set { SetValue(SelectedRightClickImageTypeProperty, value); }
        }

        public static readonly DependencyProperty SelectedRightClickImageTypeProperty =
            DependencyProperty.Register(nameof(SelectedRightClickImageType), typeof(string), typeof(ImageFullView), 
                new PropertyMetadata(ApplicationConstants.ImageViewRightClickMenuImageFitSize, ImageViewTypeChangeCallback));

        private static void ImageViewTypeChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageFullView imageView)
            {
                imageView.MenuSelectionName = imageView.SelectedRightClickImageType;
                imageView.OnMenuItemSelectionChanged(imageView.MenuSelectionName);
            }
        }

        #endregion

        #region Is Image Type Available

        public bool IsImageTypeAvailable
        {
            get { return (bool)GetValue(IsImageTypeAvailableProperty); }
            set { SetValue(IsImageTypeAvailableProperty, value); }
        }

        public static readonly DependencyProperty IsImageTypeAvailableProperty = DependencyProperty.Register(
            nameof(IsImageTypeAvailable), typeof(bool), typeof(ImageFullView), new PropertyMetadata(true));

        #endregion

        #region Is Image Type Enabled

        public bool IsImageTypeEnable
        {
            get { return (bool)GetValue(IsImageTypeEnableProperty); }
            set { SetValue(IsImageTypeEnableProperty, value); }
        }

        public static readonly DependencyProperty IsImageTypeEnableProperty = DependencyProperty.Register(
            nameof(IsImageTypeEnable), typeof(bool), typeof(ImageFullView), new PropertyMetadata(true));

        #endregion

        #region Selected Image Type

        public ImageType SelectedImageType
        {
            get { return (ImageType)GetValue(SelectedImageTypeProperty); }
            set { SetValue(SelectedImageTypeProperty, value); }
        }

        public static readonly DependencyProperty SelectedImageTypeProperty = DependencyProperty.Register(
            nameof(SelectedImageType), typeof(ImageType), typeof(ImageFullView),
            new PropertyMetadata(SelectedImagePropertyCallBack));

        #endregion

        #endregion

        #region Private Methods

        private IEnumerable<ImageType> GetImageTypes()
        {
            return Enum.GetValues(typeof(ImageType)).Cast<ImageType>();
        }

        private void OnUpdateChartVisibility(bool eNewValue)
        {
            GridMain.Visibility = eNewValue ? Visibility.Visible : Visibility.Collapsed;
            ImageView.Visibility = eNewValue ? Visibility.Collapsed : Visibility.Visible;
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

        private void UpdateImageFullWidth()
        {
            ImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            ImageScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            BindedImage.Height = ImageScrollViewerWidth;
            BindedImage.Width = ImageScrollViewerWidth;
            CanvasImage.Width = ImageScrollViewerWidth;
            CanvasImage.Height = ImageScrollViewerWidth;
            ImageScrollViewer.Height = ImageScrollViewerHeight;
            ImageScrollViewer.Width = ImageScrollViewerWidth;
        }

        private void UpdateImageActualSize()
        {
            ImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            ImageScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            var selectedImage = SelectedImage ?? SelectedImageService;
            double updatedWidth;
            double updatedHeight;
            
            if(selectedImage?.ImageSet?.BrightfieldImage != null)
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
            ImageScrollViewer.Height = ImageScrollViewerHeight;
            ImageScrollViewer.Width = ImageScrollViewerWidth;
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
                if (item.GetType() != typeof(MenuItem))
                    continue;
                
                var mi = item as MenuItem;
                if (mi != null &&
                    !mi.Name.Equals(ApplicationConstants.ImageViewRightClickMenuHistogram) &&
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

        #endregion

        #region EventHandlers

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                IsChartVisible = false;
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
            catch (IOException ioException)
            {
                ExceptionHelper.HandleExceptions(ioException, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_IMAGEFULLVIEW_IMAGEFILE"));
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_IMAGEFULLVIEW_CLICK"));
            }
        }

        private void OnMenuItemSelectionChanged(string newMenuItemName)
        {
            switch (newMenuItemName)
            {
                // Set  Histogram
                case ApplicationConstants.ImageViewRightClickMenuHistogram:
                    IsChartVisible = true;
                    Misc.IsHistogramEnable = true;
                    DisableScrollBar();
                    ImageScrollViewer.Height = HistogramControlHeight;
                    ImageScrollViewer.Width = HistogramControlWidth;
                    UpdateHistogramImage();
                    SelectedRightClickImageType = ApplicationConstants.ImageViewRightClickMenuHistogram;
                    MenuSelectionName = SelectedRightClickImageType;
                    break;
                // Set Image actual size
                case ApplicationConstants.ImageViewRightClickMenuImageActualSize:
                    Misc.IsHistogramEnable = false;
                    SelectedRightClickImageType = ApplicationConstants.ImageViewRightClickMenuImageActualSize;
                    MenuSelectionName = SelectedRightClickImageType;
                    UpdateImageActualSize();
                    break;
                // Set Image fit to window
                case ApplicationConstants.ImageViewRightClickMenuImageFitSize:
                    Misc.IsHistogramEnable = false;
                    SelectedRightClickImageType = ApplicationConstants.ImageViewRightClickMenuImageFitSize;
                    MenuSelectionName = SelectedRightClickImageType;
                    UpdateImageFullWidth();
                    break;
            }
        }

        private void ScrollViewerManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        private void SetContextMenuItemSelectedToSelect()
        {
            SetVisibility(BindedImage?.ContextMenu);
            SetVisibility(chartHisto?.ContextMenu);
        }

        private void SetVisibility(ContextMenu contextMenu)
        {
            if (contextMenu?.Items == null)
                return;

            foreach (var item in contextMenu.Items)
            {
                if (item == null) return;

                if (!(item is MenuItem menuItem) || menuItem == null) return;
                if (!(menuItem.Header is StackPanel header) || header == null) return;

                var visibility = Equals(menuItem.Name, MenuSelectionName) ? Visibility.Visible : Visibility.Hidden;
                SetChildPathVisibility(header, visibility);
            }
        }

        private void SetImage(BitmapSource imageData)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

	        BindedImage.Source = imageData;
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

        private static void SetImageFromImageDto(ImageFullView imageView, SampleImageRecordDomain sampleImage)
        {
            if (imageView == null)
                return;

            if (sampleImage?.ImageSet?.BrightfieldImage != null)
            {
                var imageDto = sampleImage.ImageSet.BrightfieldImage;

                imageView.LblBubble.Visibility = Visibility.Collapsed;
                imageView.BindedImage.Source = imageDto.ImageSource;
                imageView.SetContextMenuItemSelectedToSelect();
            }

            SetImageView(imageView, sampleImage);
        }

        private static void SetImageView(ImageFullView imageView, SampleImageRecordDomain sampleImage)
        {
            if (imageView == null) return;

            if (sampleImage?.ImageSet?.BrightfieldImage != null)
            {
                // For Dust-ref, live-image, and set-focus ResultPerImage shall be null so ignoring the image process status 
                if (sampleImage?.ResultPerImage != null)
                    imageView.UpdateImageProcessStatus(sampleImage);

                imageView.ImageScrollViewer.Height = ImageScrollViewerHeight;
                imageView.ImageScrollViewer.Width = ImageScrollViewerWidth;
                imageView.BindedImage.Height = imageView.CanvasImage.Height = ParentControlHeightAndWidth;
                imageView.BindedImage.Width = imageView.CanvasImage.Width = ParentControlHeightAndWidth;
                imageView.BindedImage.Stretch = Stretch.Fill;
                imageView.ImageScrollViewer.HorizontalScrollBarVisibility = imageView.ImageScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                imageView.SetContextMenuItemSelectedToSelect();
            }

            if (imageView.MenuSelectionName.Equals(ApplicationConstants.ImageViewRightClickMenuImageFitSize))
            {
                imageView.UpdateImageFullWidth();
            }

            if (Misc.IsHistogramEnable)
            {
                imageView.IsChartVisible = true;
                imageView.UpdateHistogramImage();

                // This is needed so that the context menus are updated properly 
                // when showing the histogram on entry into full view
                UpdateChildrenVisibility(imageView?.chartHisto?.ContextMenu?.Items);
                UpdateChildrenVisibility(imageView?.BindedImage?.ContextMenu?.Items);
            }
            else
            {
                imageView.IsChartVisible = false;
            }
        }

        private static void UpdateChildrenVisibility(ItemCollection itemCollection)
        {
            if (itemCollection?[0] == null) return;

            foreach (var contextMenuItem in itemCollection)
            {
                SetChildVisibility(contextMenuItem, Visibility.Hidden);
            }

            SetChildVisibility(itemCollection[0], Visibility.Visible);
        }

        private static void SetChildVisibility(object obj, Visibility visibility)
        {
            if (obj == null) return;

            if (obj is MenuItem menuItem && menuItem != null &&
                menuItem.Header is StackPanel stackPanel && 
                stackPanel?.Children?[0] != null)
            {
                stackPanel.Children[0].Visibility = visibility;
            }
        }

        #endregion
    }
}
