using ScoutDomains;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScoutUI.Views.CommonControls
{
    public partial class ImageOnlyView : BaseImageView
    {
        public ImageOnlyView()
        {
            InitializeComponent();
        }

        #region Dependency Properties

        private static readonly object SelectedImagePropertyCallbackLock = new object();

        #region Adjust State

        public AdjustValue AdjustState
        {
            get { return (AdjustValue)GetValue(AdjustStateProperty); }
            set { SetValue(AdjustStateProperty, value); }
        }

        public static readonly DependencyProperty AdjustStateProperty = DependencyProperty.Register(nameof(AdjustState),
            typeof(AdjustValue), typeof(ImageOnlyView), new PropertyMetadata(null));

        #endregion

        #region Image List

        public ObservableCollection<SampleImageRecordDomain> ImageList
        {
            get { return (ObservableCollection<SampleImageRecordDomain>)GetValue(ImageListProperty); }
            set { SetValue(ImageListProperty, value); }
        }

        public static readonly DependencyProperty ImageListProperty = DependencyProperty.Register(nameof(ImageList),
            typeof(ObservableCollection<SampleImageRecordDomain>), typeof(ImageOnlyView));

        #endregion

        #region Selected Image Index

        public KeyValuePair<int, string> SelectedImageIndex
        {
            get { return (KeyValuePair<int, string>)GetValue(SelectedImageIndexProperty); }
            set { SetValue(SelectedImageIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedImageIndexProperty =
            DependencyProperty.Register(nameof(SelectedImageIndex), typeof(KeyValuePair<int, string>), 
                typeof(ImageOnlyView));

        #endregion

        #region Image Index List

        public ObservableCollection<KeyValuePair<int, string>> ImageIndexList
        {
            get { return (ObservableCollection<KeyValuePair<int, string>>)GetValue(ImageIndexListProperty); }
            set { SetValue(ImageIndexListProperty, value); }
        }

        public static readonly DependencyProperty ImageIndexListProperty = DependencyProperty.Register(
            nameof(ImageIndexList), typeof(ObservableCollection<KeyValuePair<int, string>>), 
            typeof(ImageOnlyView), new PropertyMetadata(null));

        #endregion

        #region Selected Image Type

        public ImageType SelectedImageType
        {
            get { return (ImageType)GetValue(SelectedImageTypeProperty); }
            set { SetValue(SelectedImageTypeProperty, value); }
        }

        public static readonly DependencyProperty SelectedImageTypeProperty = DependencyProperty.Register(
            nameof(SelectedImageType), typeof(ImageType), typeof(ImageOnlyView), new PropertyMetadata(null));

        #endregion

        #region Selected Image with Callback

        public SampleImageRecordDomain SelectedImage
        {
            get { return (SampleImageRecordDomain)GetValue(SelectedImageProperty); }
            set { SetValue(SelectedImageProperty, value); }
        }

        public static readonly DependencyProperty SelectedImageProperty = DependencyProperty.Register("SelectedImage",
            typeof(SampleImageRecordDomain), typeof(ImageOnlyView), new PropertyMetadata(SelectedImagePropertyCallBack));

        private static void SelectedImagePropertyCallBack(DependencyObject d, DependencyPropertyChangedEventArgs eventArgs)
        {
            var imageView = d as ImageOnlyView;
            if (imageView?.ImageList == null)
            {
                return;
            }

            imageView.Rectangle.Visibility = Visibility.Collapsed;
            var newSelectedImage = eventArgs.NewValue as SampleImageRecordDomain;
           
            // This lock does not guarantee the correct order, but it eliminates race-conditions.
            lock (SelectedImagePropertyCallbackLock)
            {
	            if (imageView.SelectedImage?.ResultPerImage == null)
	            {
		            imageView.BindedImage.Source = null;
		            return;
	            }

				if (imageView.ImageList.Any())
                {
                    var index = new KeyValuePair<int, string>(Convert.ToInt32(newSelectedImage?.SequenceNumber), newSelectedImage?.SequenceNumber.ToString());
                    if (!index.Equals(imageView.SelectedImageIndex))
                    {
                        imageView.SelectedImageIndex = index;
                    }
                }

                if (newSelectedImage?.ImageSet?.BrightfieldImage == null)
                {
                    return;
                }
                var imageData = newSelectedImage.ImageSet.BrightfieldImage.ImageSource;
				imageView.SetImage(imageData);

                if (!string.IsNullOrEmpty(imageView.MenuSelectionName) &&
                    imageView.MenuSelectionName.Equals(ApplicationConstants.ImageViewRightClickMenuImageActualSize))
                {
                    imageView.UpdateImageActualSize();
                }
                else
                {
                    imageView.SetImageSize();
                }
            }
        }

        #endregion

        #endregion

        #region Private Methods

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
        }

        private void UpdateImageActualSize()
        {
            ImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            ImageScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            var selectedImage = SelectedImage;

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

        #endregion
    }
}
