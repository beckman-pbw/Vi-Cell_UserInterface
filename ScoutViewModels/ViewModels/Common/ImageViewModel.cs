using ApiProxies.Generic;
using ScoutDomains;
using ScoutDomains.ClusterDomain;
using ScoutLanguageResources;
using ScoutModels.Common;
using ScoutUtilities;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutViewModels.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using ScoutUtilities.Common;
using CoordinatePair = System.Drawing.Point;
using Point = System.Windows.Point;

namespace ScoutViewModels.ViewModels.Common
{
    public class ImageViewModel : BaseViewModel
    {
        public ImageViewModel(ResultRecordHelper recordHelper)
        {
            RecordHelper = recordHelper;
        }

        protected override void DisposeUnmanaged()
        {
            RecordHelper?.Dispose();
            base.DisposeUnmanaged();
        }

        #region Properties & Fields

        public ResultRecordHelper RecordHelper { get; set; }
        public DetailedResultMeasurementsDomain DetailedMeasurements { get; private set; }

        public string CurrentImageLabel
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        private SampleRecordDomain _selectedSampleFromList;
        public SampleRecordDomain SelectedSampleFromList
        {
            get { return _selectedSampleFromList ?? (_selectedSampleFromList = new SampleRecordDomain()); }
            set
            {
                if (_selectedSampleFromList != null && _selectedSampleFromList.Equals(value)) return;
                if (_selectedSampleFromList != null) _selectedSampleFromList.PropertyChanged -= OnUpdateSelectedResultRecord;

                _selectedSampleFromList = value;
                NotifyPropertyChanged(nameof(SelectedSampleFromList));

                if (value != null)
                {
                    OnSelectedSampleRecord(value);
                    OnUpdateSelectedResultRecord();
                    value.PropertyChanged += OnUpdateSelectedResultRecord;
                }

                UpdateCurrentImageLabel();
            }
        }

        private BlobMeasurementDomain _lastSelectedBlob;
        // Detailed measurements of the blob closest to where the user last
        // tapped; null if the image has not yet been tapped or if the user
        // last tapped on a cluster instead of a blob.
        public BlobMeasurementDomain LastSelectedBlob
        {
            get { return _lastSelectedBlob; }
            private set
            {
                if (value != null
                    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    // This code is duplicated in three locations.
                    //     + ExpandedImageGraphViewModel
                    //     + ImageViewModel
                    //     + SampleResultDialogViewModel
                    // Change all at the same time
                    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    //
                    // Check that the blob data includes the expected characteristics
                    // These values are used as an index so they MUST ALL be there or we crash
                    // Clicking on a cluster was causing a crash
                    && value.Measurements.ContainsKey(BlobCharacteristicKeys.DiameterInMicrons)
                    && value.Measurements.ContainsKey(BlobCharacteristicKeys.IsPOI)
                    && value.Measurements.ContainsKey(BlobCharacteristicKeys.Circularity)
                    && value.Measurements.ContainsKey(BlobCharacteristicKeys.AvgSpotBrightness)
                    && value.Measurements.ContainsKey(BlobCharacteristicKeys.CellSpotArea))
                {
                    Log.Info($"\nNearest to blob with coordinates: {value.Coordinates}\n"
                        + $"Diameter: {value.Measurements[BlobCharacteristicKeys.DiameterInMicrons]}\n"
                        + $"Viable? {value.Measurements[BlobCharacteristicKeys.IsPOI]}\n");
                    _lastSelectedBlob = value;
                }
                else
                {
                    Log.Info("No cells present in current image.");
                    _lastSelectedBlob = null;   // `value` *may* not be `null` (see extra conditions above)
                }

                NotifyPropertyChanged(nameof(LastSelectedBlob));
                if (_lastSelectedBlob != null)
                {
                    UpdateBlobData();
                }
            }
        }

        // Detailed measurements of the cluster closest to where the user last
        // tapped; null if the image has not yet been tapped or if the user
        // last tapped on a blob instead of a cluster.
        public LargeClusterDataDomain LastSelectedCluster
        {
            get { return GetProperty<LargeClusterDataDomain>(); }
            private set
            {
                Log.Info(value != null
                    ? $"Nearest to cluster with top-left coordinates:\n\t[{value.top_left_x}, {value.top_left_y}]\nNumber of cells in cluster: {value.numCell}"
                    : "No large clusters present in current image.");

                SetProperty(value);
            }
        }

        public AdjustValue AdjustState
        {
            get { return GetProperty<AdjustValue>(); }
            set
            {
                SetProperty(value);
                OnChannelTraversal(value);
            }
        }

        public virtual KeyValuePair<int, string> SelectedImageIndex
        {
            get { return GetProperty<KeyValuePair<int, string>>(); }
            set
            {
                if (GetProperty<KeyValuePair<int, string>>().Equals(value)) return;

                SetProperty(value);
                var index = SelectedSampleFromList.ImageIndexList.IndexOf(value);
                if (index < 0)
                {
                    var list = string.Join(", ", SelectedSampleFromList.ImageIndexList.Select(i => i.Key));
                    Log.Warn($"Failed to find image index '{value.Key}' in SelectedSampleFromList.ImageIndexList." +
                             $"{Environment.NewLine}" +
                             $"SelectedSampleFromList.ImageIndexList values: {list}" +
                             $"{Environment.NewLine}" +
                             $"Count: {SelectedSampleFromList.ImageIndexList.Count}" +
                             $"{Environment.NewLine}" +
                             $"SelectedSampleFromList.ImageIndexList: {SelectedSampleFromList.ImageIndexList}");
                }
                SetImage(index);
            }
        }

        public string ImageViewType
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public ImageType SelectedImageType
        {
            get { return GetProperty<ImageType>(); }
            set
            {
                if (GetProperty<ImageType>() == value) return;

                SetProperty(value);
                if (SelectedSampleFromList?.SampleImageList != null)
                {
                    SetImage(SelectedSampleFromList.SampleImageList.IndexOf(SelectedSampleFromList.SelectedSampleImageRecord));
                }
            }
        }

        public CoordinatePair LastTappedPixel
        {
            get { return GetProperty<CoordinatePair>(); }
            set
            {
                SetProperty(value);
                Log.Info($"User tapped on image coordinates [{value}]");
                SetLastTappedObject();
            }
        }

        public List<KeyValuePair<string, string>> AnnotatedBlobDetails
        {
            get { return GetProperty<List<KeyValuePair<string, string>>>(); }
            set { SetProperty(value); }
        }

        public bool EnableBlobPopup
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsFromReview
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string SelectedRightClickImageType
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Public Methods

        public virtual void OnSelectedSampleRecord(SampleRecordDomain sampleRecord)
        {

        }

        public virtual void SetImage(int imageIndex)
        {
            if (SelectedSampleFromList?.SampleImageList == null)
                return;

            if ((imageIndex < 0) || (imageIndex >= SelectedSampleFromList.SampleImageList.Count))
                return;

            // This is a hack: Expanded image-view has its *own* SetImage
            // method, and (for some reason) *cannot* use the method here.
            if (GetType() == typeof(ExpandedImageGraphViewModel))
                return;

            var sampleImage = SelectedSampleFromList.SampleImageList[imageIndex];
            sampleImage.TotalCumulativeImage = Convert.ToInt32(SelectedSampleFromList.SelectedResultSummary.CumulativeResult.TotalCumulativeImage);

            if (!sampleImage.BrightFieldId.IsEmpty() ||
                (SelectedImageType != ImageType.Annotated && !SelectedSampleFromList.SelectedResultSummary.UUID.IsEmpty()))
            {
	            sampleImage.ImageSet = RecordHelper.GetImage(SelectedImageType, sampleImage.BrightFieldId, SelectedSampleFromList.SelectedResultSummary.UUID);
            }
            
            SelectedSampleFromList.SelectedSampleImageRecord = sampleImage;
            UpdateCurrentImageLabel();
        }

        #endregion

        #region Protected Methods

        protected virtual void OnChannelTraversal(AdjustValue adjustVal)
        {
            try
            {
                if (SelectedSampleFromList?.SelectedSampleImageRecord == null || SelectedSampleFromList.SampleImageList == null || !SelectedSampleFromList.SampleImageList.Any())
                    return;

                int imgIndex = SelectedSampleFromList.SampleImageList.IndexOf(SelectedSampleFromList.SelectedSampleImageRecord);
                switch (adjustVal)
                {
                    case AdjustValue.Idle:
                        break;
                    case AdjustValue.Left:
                        if (imgIndex == 0) return;
                        SetImage(imgIndex - 1);
                        break;
                    case AdjustValue.Right:
                        if (imgIndex == SelectedSampleFromList.SampleImageList.Count - 1) return;
                        SetImage(imgIndex + 1);
                        break;
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_CHANNEL_TRAVERSAL"));
            }
        }

        #endregion

        #region Private Methods

        private void UpdateCurrentImageLabel()
        {
            if (string.IsNullOrEmpty(SelectedSampleFromList?.SampleIdentifier))
            {
                CurrentImageLabel = string.Empty;
                return;
            }

            var total = SelectedSampleFromList.SelectedSampleImageRecord?.TotalCumulativeImage ?? 0;
            var seq = SelectedSampleFromList.SelectedSampleImageRecord?.SequenceNumber ?? 0;

            CurrentImageLabel = $"{SelectedSampleFromList.SampleIdentifier}  " +
                                $"{SelectedSampleFromList.Position}  " +
                                $"(#{seq}/{total})";
        }

        private void OnUpdateSelectedResultRecord(object selectedSampleRecord, PropertyChangedEventArgs e)
        {
            if (SelectedSampleFromList?.SelectedResultSummary == null ||
                e.PropertyName != nameof(SelectedSampleFromList.SelectedResultSummary))
            {
                return;
            }

            OnUpdateSelectedResultRecord();
        }

        private void OnUpdateSelectedResultRecord()
        {
            if (SelectedSampleFromList?.SelectedResultSummary?.UUID.IsEmpty() == false)
            {
                DetailedMeasurements = RecordHelper.RetrieveDetailedMeasurementsForResultRecord(SelectedSampleFromList.SelectedResultSummary.UUID);
            }
        }

        private void UpdateBlobData()
        {
            if (!SelectedImageType.Equals(ImageType.Annotated))
                return;

            var diameter = Misc.UpdateTrailingPoint(LastSelectedBlob.Measurements[BlobCharacteristicKeys.DiameterInMicrons], TrailingPoint.Two);
            var diameterWithUnit = string.Format(LanguageResourceHelper.Get("LID_Label_MicronUnitWithValue"), diameter);
            var circularity = Misc.UpdateTrailingPoint(LastSelectedBlob.Measurements[BlobCharacteristicKeys.Circularity], TrailingPoint.Two);
            var poiYesNo = LastSelectedBlob.Measurements[BlobCharacteristicKeys.IsPOI].Equals(0)
                ? LanguageResourceHelper.Get("LID_ButtonContent_No")
                : LanguageResourceHelper.Get("LID_ButtonContent_Yes");
            var sharpness = Misc.UpdateTrailingPoint(LastSelectedBlob.Measurements[BlobCharacteristicKeys.Sharpness], TrailingPoint.One);
            var avgBrightness = Misc.UpdateTrailingPoint(LastSelectedBlob.Measurements[BlobCharacteristicKeys.AvgSpotBrightness], TrailingPoint.One);
            var brightness = $"{avgBrightness} {LanguageResourceHelper.Get("LID_Label_PercentUnit")}";
            var cellSpotArea = Misc.UpdateTrailingPoint(LastSelectedBlob.Measurements[BlobCharacteristicKeys.CellSpotArea], TrailingPoint.One);
            var cellSpotPercent = $"{cellSpotArea} {LanguageResourceHelper.Get("LID_Label_PercentUnit")}";

            var blobDetails = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(LanguageResourceHelper.Get("LID_Label_Diameter"), diameterWithUnit),
                new KeyValuePair<string, string>(LanguageResourceHelper.Get("LID_Result_Circularity"), circularity),
                new KeyValuePair<string, string>(LanguageResourceHelper.Get("LID_Concatenate_Viable"), poiYesNo),
                new KeyValuePair<string, string>(LanguageResourceHelper.Get("LID_Label_CellSharpness"), sharpness),
                new KeyValuePair<string, string>(LanguageResourceHelper.Get("LID_Label_ViableCellSpotBrightness"), brightness),
                new KeyValuePair<string, string>(LanguageResourceHelper.Get("LID_Label_ViableCellSpotArea"), cellSpotPercent)
            };

            AnnotatedBlobDetails = blobDetails;
            EnableBlobPopup = AnnotatedBlobDetails.Any();
        }

        private void SetLastTappedObject()
        {
            if (DetailedMeasurements?.BlobsByImage == null || DetailedMeasurements?.LargeClustersByImage == null)
                return;

            var tapPoint = new Point(LastTappedPixel.X, LastTappedPixel.Y);

            var closestBlobInfo = FindClosestObject(tapPoint, DetailedMeasurements.BlobsByImage[SelectedImageIndex.Key - 1].BlobList,
                blob => new Point(blob.Coordinates.X, blob.Coordinates.Y));

            var closestClusterInfo = FindClosestObject(tapPoint, DetailedMeasurements.LargeClustersByImage[SelectedImageIndex.Key - 1].LargeClusterDataList,
                cluster =>
                {
                    var clusterCorner1 = new Point(cluster.top_left_x, cluster.top_left_y);
                    var clusterCorner2 = new Point(cluster.bottom_right_x, cluster.bottom_right_y);
                    return clusterCorner1 + 0.5 * (clusterCorner2 - clusterCorner1);
                });

            if (closestBlobInfo.Item2 < closestClusterInfo.Item2)
            {
                LastSelectedBlob = closestBlobInfo.Item1;
                LastSelectedCluster = null;
            }
            else
            {
                LastSelectedCluster = closestClusterInfo.Item1;
                LastSelectedBlob = null;
            }
        }

        private Tuple<TObject, double> FindClosestObject<TObject>(Point tapPoint, List<TObject> objectList, Func<TObject, Point> locationOf) where TObject : class
        {
            TObject closestObject = null;
            var minObjDistance = double.PositiveInfinity;

            foreach (var obj in objectList)
            {
                var distance = Point.Subtract(locationOf(obj), tapPoint).Length;
                if (distance < minObjDistance)
                {
                    closestObject = obj;
                    minObjDistance = distance;
                }
            }

            return Tuple.Create(closestObject, minObjDistance);
        }

        private void OnExpandView()
        {
            try
            {
                if (SelectedSampleFromList == null) return;

                var args = new ExpandedImageGraphEventArgs(SelectedImageType, SelectedRightClickImageType,
                    SelectedSampleFromList?.SampleImageList?.Cast<object>().ToList(),
                    SelectedSampleFromList, (int)SelectedSampleFromList.SelectedSampleImageRecord.SequenceNumber);
                DialogEventBus.ExpandedImageGraph(this, args);
                var newIndex = args.SelectedImageIndex + 1;
                SelectedImageIndex = new KeyValuePair<int, string>(newIndex, newIndex.ToString());
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_EXPAND_VIEW"));
            }
        }

        #endregion

        #region Commands

        #region Image Expand Command

        private RelayCommand _imageExpandCommand;
        public RelayCommand ImageExpandCommand => _imageExpandCommand ?? (_imageExpandCommand = new RelayCommand(OnExpandImage));

        private void OnExpandImage()
        {
            OnExpandView();
        }

        #endregion

        #region Play SlideShow Command

        private RelayCommand _playSlideShowCommand;
        public RelayCommand PlaySlideShowCommand => _playSlideShowCommand ?? (_playSlideShowCommand = new RelayCommand(PlaySlideShow, CanPlaySlideShow));

        private bool CanPlaySlideShow()
        {
            var totalNumImages = SelectedSampleFromList?.ImageIndexList?.Count ?? 0;
            return totalNumImages > 0 && !SlideShowIsRunning;
        }

        private void PlaySlideShow()
        {
            SlideShowIsRunning = true;
        }

        private DispatcherTimer _slideShowTimer;

        protected bool SlideShowIsRunning 
        {
            get { return GetProperty<bool>(); }
            set
            {
                var changed = GetProperty<bool>() != value;
                if (!changed) return;

                SetProperty(value);

                if (value)
                {
                    var totalNumImages = SelectedSampleFromList?.ImageIndexList?.Count ?? 0;
                    if (totalNumImages <= 0) return;

                    _slideShowTimer?.Stop();

                    var imgList = SelectedSampleFromList?.ImageIndexList?.ToList();
                    var currentImageIndex = imgList?.FindIndex(i => i.Key == SelectedImageIndex.Key);
                    if (currentImageIndex >= totalNumImages)
                    {
                        // we're on the last image so set the current image to the first one and start the slide show
                        SelectedImageIndex = new KeyValuePair<int, string>(1, 1.ToString());
                    }

                    var timeSpan = new TimeSpan(0, 0, 0, 0, 
                        ApplicationConstants.SlideShowImageTimeMs);
                    _slideShowTimer = new DispatcherTimer(timeSpan, DispatcherPriority.Normal,
                        OnSlideShowTimerTick, Dispatcher.CurrentDispatcher);
                    _slideShowTimer.Start();
                }
                else
                {
                    _slideShowTimer?.Stop();
                }

                PlaySlideShowCommand.RaiseCanExecuteChanged();
                PauseSlideShowCommand.RaiseCanExecuteChanged();
            }
        }

        private void OnSlideShowTimerTick(object sender, EventArgs args)
        {
            var totalNumImages = SelectedSampleFromList.ImageIndexList.Count;
            var imgList = SelectedSampleFromList.ImageIndexList.ToList();
            var currentImageIndex = imgList.FindIndex(i => i.Key == SelectedImageIndex.Key);
            // select the next image
            var next = currentImageIndex + 1;
            if (next >= totalNumImages)
            {
                // go back to the beginning of the list and end the timer
                SelectedImageIndex = new KeyValuePair<int, string>(1, 1.ToString());
                SlideShowIsRunning = false;
                return;
            }

            SelectedImageIndex = imgList[next];

        }

        #endregion

        #region Pause SlideShow Command

        private RelayCommand _pauseSlideShowCommand;
        public RelayCommand PauseSlideShowCommand => _pauseSlideShowCommand ?? (_pauseSlideShowCommand = new RelayCommand(PauseSlideShow, CanPauseSlideShow));

        private bool CanPauseSlideShow()
        {
            var totalNumImages = SelectedSampleFromList?.ImageIndexList?.Count ?? 0;
            return SlideShowIsRunning && totalNumImages > 0;
        }

        private void PauseSlideShow()
        {
            SlideShowIsRunning = false;
        }

        #endregion

        #endregion
    }
}