using ScoutDomains;
using ScoutDomains.ClusterDomain;
using ScoutDomains.Common;
using ScoutDomains.RunResult;
using ScoutLanguageResources;
using ScoutModels.Common;
using ScoutUtilities;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using ScoutUtilities.Common;
using CoordinatePair = System.Drawing.Point;
using Point = System.Windows.Point;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class ExpandedImageGraphViewModel : BaseDialogViewModel
    {
        #region Constructor

        public ExpandedImageGraphViewModel(ExpandedImageGraphEventArgs args, System.Windows.Window parentWindow) : base(args, parentWindow)
        {
            Initialize();

            switch (args.ContentType)
            {
                case ExpandedContentType.Image:
                    IsImageActive = true;
                    IsGraphActive = false;
                    IsFromScatterGraph = false;
                    DialogTitle = LanguageResourceHelper.Get("LID_Label_Image");
                    break;
                case ExpandedContentType.BarChart:
                    IsImageActive = false;
                    IsGraphActive = true;
                    IsFromScatterGraph = false;
                    DialogTitle = LanguageResourceHelper.Get("LID_Label_Graph");
                    break;
                case ExpandedContentType.ScatterChart:
                    IsImageActive = false;
                    IsGraphActive = false;
                    IsFromScatterGraph = true;
                    DialogTitle = LanguageResourceHelper.Get("LID_Label_Graph");
                    break;
            }

            SelectedRightClickImageType = args.SelectedRightClickImageType;
            SelectedImageType = args.SelectedImageType;

            IsQMImage = args.IsQueueManagementDialog;
            IsResultListVisible = args.IsExpandedResultListVisible;
            IsHorizontalPaginationVisible = args.IsHorizontalPaginationVisible;
            IsListAvailable = args.IsListAvailable;
            IsSetFocusEnable = args.IsSetFocusEnable;
            ShowSlideShowButtons = args.ShowSlideShowButtons;

            try
            {
                if (args.SelectedBarGraphDomain is BarGraphDomain barGraphDomain)
                {
                    Graph = barGraphDomain;
                    if (args.BarGraphDomainList != null && args.BarGraphDomainList.Any() && args.BarGraphDomainList[0] is BarGraphDomain)
                    {
                        GraphViewList = args.BarGraphDomainList?.Cast<BarGraphDomain>().ToList();
                    }
                }
                else if (args.SelectedBarGraphDomain is LineGraphDomain lineGraphDomain)
                {
                    SelectedGraphItemConcentration = lineGraphDomain;
                    if (args.BarGraphDomainList != null && args.BarGraphDomainList.Any() && args.BarGraphDomainList[0] is LineGraphDomain)
                    {
                        ConcentrationGraphViewList = args.BarGraphDomainList?.Cast<LineGraphDomain>().ToList();
                    }
                }

                if (args.SelectedSampleRecordDomain is SampleRecordDomain sampleRecordDomain)
                {
                    SelectedSampleFromList = sampleRecordDomain;
                    SampleImageDomain = SelectedSampleFromList?.SelectedSampleImageRecord;
                    SetImageList(args);

                    if (SampleImageDomain != null)
                    {
                        ResultSummary = SelectedSampleFromList.SelectedResultSummary;
                        ImageIndexList = SelectedSampleFromList.ImageIndexList;
                        SelectedImageIndex = ImageIndexList.FirstOrDefault(i => i.Key == SampleImageDomain.SequenceNumber);
                    }
                }
                else if (args.SelectedSampleRecordDomain is SampleImageRecordDomain sampleImage)
                {
                    SampleImageDomain = sampleImage; // should we do this instead?
                    //SampleImageDomainService = sampleImage;
                    SetImageList(args);
                }
                else if (args.SampleImageRecordDomain is SampleImageRecordDomain sampleImageRecord)
                {
                    SampleImageDomain = sampleImageRecord; // should we do this instead?
                    //SampleImageDomainService = sampleImageRecord;
                    SetImageList(args);
                }
            }
            catch (Exception e)
            {
                Log.Error("Failed to cast ExpandedImageGraphEventArgs properties", e);
            }
        }

        private void SetImageList(ExpandedImageGraphEventArgs args)
        {
            if (args.SampleImageRecordDomains != null && args.SampleImageRecordDomains.Any() && args.SampleImageRecordDomains[0] is SampleImageRecordDomain)
            {
                SampleImageResultList = args.SampleImageRecordDomains?.Cast<SampleImageRecordDomain>().ToList();
                
                ImageIndexList = new ObservableCollection<KeyValuePair<int, string>>();
                for(var i = 1; i <= SampleImageResultList.Count; i++)
                {
                    ImageIndexList.Add(new KeyValuePair<int, string>(i, ScoutUtilities.Misc.ConvertToString(i)));
                }
            }
        }

        private void Initialize()
        {
            ShowDialogTitleBar = true;

            IsImageActive = true;
            IsHorizontalPaginationVisible = true;
            IsListAvailable = true;
            RecordHelper = new ResultRecordHelper();

            CurrentGraphIndex = 0;
            CurrentImageIndex = 0;
        }

        protected override void DisposeManaged()
        {
            _slideShowTimer?.Stop();
            base.DisposeManaged();
        }

        protected override void DisposeUnmanaged()
        {
            RecordHelper?.Dispose();
            base.DisposeUnmanaged();
        }

        #endregion

        #region Properties & Fields

        private DispatcherTimer _slideShowTimer;

        public ResultSummaryDomain ResultSummary { get; set; }
        public ResultRecordHelper RecordHelper { get; set; }
        public DetailedResultMeasurementsDomain DetailedMeasurements { get; private set; }

        public int CurrentImageIndex { get; set; } // the non-customer-facing number of the current image
        public int CurrentGraphIndex { get; set; } // the non-customer-facing number of the current graph

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
                    OnUpdateSelectedResultRecord();
                    value.PropertyChanged += OnUpdateSelectedResultRecord;
                }
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
                    // Not sure this is really used, but the checks should be the same everywhere
                    // Check that the blob data includes the expected characteristics
                    // These values are used as an index so they MUST be there or we crash
                    // Clicking on a cluster could cause a crash
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

        public ImageType SelectedImageType
        {
            get { return GetProperty<ImageType>(); }
            set
            {
                if (GetProperty<ImageType>() == value) return;

                SetProperty(value, NotifyType.Force);
                var index = ImageIndexList.IndexOf(SelectedImageIndex);
                SetImage(index);
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

        public ObservableCollection<KeyValuePair<int, string>> ImageIndexList
        {
            get
            {
                var item = GetProperty<ObservableCollection<KeyValuePair<int, string>>>();
                if (item != null) return item;
                SetProperty(new ObservableCollection<KeyValuePair<int, string>>());
                return GetProperty<ObservableCollection<KeyValuePair<int, string>>>();
            }
            set { SetProperty(value); }
        }

        public KeyValuePair<int, string> SelectedImageIndex
        {
            get { return GetProperty<KeyValuePair<int, string>>(); }
            set
            {
                SetProperty(value, NotifyType.Force);
                if (ImageIndexList.Contains(value))
                {
                    SetImage(ImageIndexList.IndexOf(value));
                }
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

        public string SelectedRightClickImageType
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public List<LineGraphDomain> ConcentrationGraphViewList
        {
            get { return GetProperty<List<LineGraphDomain>>(); }
            set { SetProperty(value); }
        }

        public LineGraphDomain SelectedGraphItemConcentration
        {
            get { return GetProperty<LineGraphDomain>(); }
            set { SetProperty(value); }
        }

        public List<BarGraphDomain> GraphViewList
        {
            get { return GetProperty<List<BarGraphDomain>>(); }
            set { SetProperty(value); }
        }

        public BarGraphDomain Graph
        {
            get
            {
                var item = GetProperty<BarGraphDomain>();
                if (item != null) return item;
                SetProperty(new BarGraphDomain());
                return GetProperty<BarGraphDomain>();
            }
            set
            {
                SetProperty(value);
                if (GraphViewList != null && GraphViewList.Any())
                {
                    CurrentGraphIndex = GraphViewList.IndexOf(value);
                }
            }
        }

        public List<SampleImageRecordDomain> SampleImageResultList
        {
            get
            {
                var item = GetProperty<List<SampleImageRecordDomain>>();
                if (item != null) return item;
                SetProperty(new List<SampleImageRecordDomain>());
                return GetProperty<List<SampleImageRecordDomain>>();
            }
            set { SetProperty(value); }
        }

        public SampleImageRecordDomain SampleImageDomain
        {
            get { return GetProperty<SampleImageRecordDomain>(); }
            set { SetProperty(value, NotifyType.Force); }
        }

        public SampleImageRecordDomain SampleImageDomainService
        {
            get { return GetProperty<SampleImageRecordDomain>(); }
            set { SetProperty(value); }
        }

        public bool IsSetFocusEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsHorizontalPaginationVisible
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsQMImage
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsListAvailable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsResultListVisible
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public DisplayPane DisplayPane
        {
            get { return GetProperty<DisplayPane>(); }
            set
            {
                SetProperty(value);
                PlaySlideShowCommand.RaiseCanExecuteChanged();
                PauseSlideShowCommand.RaiseCanExecuteChanged();
            }
        }

        public bool ShowSlideShowButtons
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        protected bool SlideShowIsRunning // doesn't need to be public
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

        #region Setting Image/Graph/Scatter bools

        public bool IsFromGraph
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                if (value)
                {
                    IsImageActive = IsFromScatterGraph = false;
                    IsGraphActive = true;
                }
            }
        }

        public bool IsFromScatterGraph
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                if (value)
                {
                    IsScatterGraphActive = true;
                    IsGraphActive = IsImageActive = false;
                }
            }
        }

        public bool IsGraphActive
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                if (value)
                {
                    IsScatterGraphActive = IsImageActive = false;
                }
            }
        }

        public bool IsScatterGraphActive
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                if (value)
                {
                    IsImageActive = IsGraphActive = false;
                }
            }
        }

        public bool IsImageActive
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                if (value)
                {
                    IsScatterGraphActive = IsGraphActive = false;
                }
            }
        }

        #endregion

        #endregion

        #region Commands

        #region Play SlideShow Command

        private RelayCommand _playSlideShowCommand;
        public RelayCommand PlaySlideShowCommand => _playSlideShowCommand ?? (_playSlideShowCommand = new RelayCommand(PlaySlideShow, CanPlaySlideShow));

        private bool CanPlaySlideShow()
        {
            var totalNumImages = SelectedSampleFromList?.ImageIndexList?.Count ?? 0;
            return DisplayPane == DisplayPane.Image && !SlideShowIsRunning &&
                   totalNumImages > 0;
        }

        private void PlaySlideShow()
        {
            SlideShowIsRunning = true;
        }

        #endregion

        #region Pause SlideShow Command

        private RelayCommand _pauseSlideShowCommand;
        public RelayCommand PauseSlideShowCommand => _pauseSlideShowCommand ?? (_pauseSlideShowCommand = new RelayCommand(PauseSlideShow, CanPauseSlideShow));

        private bool CanPauseSlideShow()
        {
            var totalNumImages = SelectedSampleFromList?.ImageIndexList?.Count ?? 0;
            return DisplayPane == DisplayPane.Image && SlideShowIsRunning &&
                   totalNumImages > 0;
        }

        private void PauseSlideShow()
        {
            SlideShowIsRunning = false;
        }

        #endregion

        #endregion

        #region Private Methods

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

        private void SetImage(int selectedImageIndex)
        {
            if (selectedImageIndex < 0 || SampleImageResultList.Count <= selectedImageIndex)
                return;

            if (IsQMImage || ResultSummary?.UUID.u == null)
            {
                SampleImageDomain = SampleImageResultList[selectedImageIndex];
            }
            else
            {
                var sampleImage = SampleImageResultList[selectedImageIndex];
                var imageData = RecordHelper.GetImage(SelectedImageType, 
                    SampleImageResultList[selectedImageIndex].BrightFieldId, ResultSummary.UUID);
                sampleImage.TotalCumulativeImage = Convert.ToInt32(
                    ResultSummary.CumulativeResult.TotalCumulativeImage);
                sampleImage.ImageSet = imageData;
                SampleImageDomain = sampleImage;
            }

            CurrentImageIndex = selectedImageIndex;
        }

        private void OnChannelTraversal(AdjustValue val)
        {
            if (SampleImageDomain == null) return;

            var imgIndex = SampleImageResultList.IndexOf(SampleImageDomain);

            switch (val)
            {
                case AdjustValue.Idle:
                    break;
                case AdjustValue.Left:
                    if (imgIndex == 0) return;
                    SetImage(imgIndex - 1);
                    break;
                case AdjustValue.Right:
                    if (imgIndex == SampleImageResultList.Count - 1) return;
                    SetImage(imgIndex + 1);
                    break;
            }
        }

        private void UpdateBlobData()
        {
            if (!SelectedImageType.Equals(ImageType.Annotated))
                return;

            var diameter = ScoutUtilities.Misc.UpdateTrailingPoint(LastSelectedBlob.Measurements[BlobCharacteristicKeys.DiameterInMicrons], TrailingPoint.Two);
            var diameterWithUnit = string.Format(LanguageResourceHelper.Get("LID_Label_MicronUnitWithValue"), diameter);
            var circularity = ScoutUtilities.Misc.UpdateTrailingPoint(LastSelectedBlob.Measurements[BlobCharacteristicKeys.Circularity], TrailingPoint.Two);
            var poiYesNo = LastSelectedBlob.Measurements[BlobCharacteristicKeys.IsPOI].Equals(0)
                ? LanguageResourceHelper.Get("LID_ButtonContent_No")
                : LanguageResourceHelper.Get("LID_ButtonContent_Yes");
            var sharpness = ScoutUtilities.Misc.UpdateTrailingPoint(LastSelectedBlob.Measurements[BlobCharacteristicKeys.Sharpness], TrailingPoint.One);
            var avgBrightness = ScoutUtilities.Misc.UpdateTrailingPoint(LastSelectedBlob.Measurements[BlobCharacteristicKeys.AvgSpotBrightness], TrailingPoint.One);
            var brightness = $"{avgBrightness} {LanguageResourceHelper.Get("LID_Label_PercentUnit")}";
            var cellSpotArea = ScoutUtilities.Misc.UpdateTrailingPoint(LastSelectedBlob.Measurements[BlobCharacteristicKeys.CellSpotArea], TrailingPoint.One);
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

        private void OnUpdateSelectedResultRecord(object selectedSampleRecord, PropertyChangedEventArgs e)
        {
            if (SelectedSampleFromList?.SelectedResultSummary == null || e.PropertyName != nameof(SelectedSampleFromList.SelectedResultSummary))
                return;

            if (selectedSampleRecord == SelectedSampleFromList)
            {
                Log.Error($"{nameof(selectedSampleRecord)} = {nameof(SelectedSampleFromList)}", new Exception("Exception to capture stack trace."));
            }

            OnUpdateSelectedResultRecord();
        }

        private void OnUpdateSelectedResultRecord()
        {
            if (SelectedSampleFromList?.SelectedResultSummary?.UUID != null)
            {
                DetailedMeasurements = RecordHelper.RetrieveDetailedMeasurementsForResultRecord(SelectedSampleFromList.SelectedResultSummary.UUID);
            }
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

        #endregion
    }
}
