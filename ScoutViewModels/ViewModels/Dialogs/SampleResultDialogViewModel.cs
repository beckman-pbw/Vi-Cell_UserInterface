using ApiProxies.Generic;
using ApiProxies.Misc;
using HawkeyeCoreAPI.Facade;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.ClusterDomain;
using ScoutDomains.Common;
using ScoutDomains.RunResult;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutModels.Review;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Interfaces;
using ScoutUtilities.Structs;
using ScoutViewModels.ViewModels.Common;
using ScoutViewModels.ViewModels.ExpandedSampleWorkflow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using DrawPoint = System.Drawing.Point;
using ScoutModels.Interfaces;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class SampleResultDialogViewModel : BaseDialogViewModel
    {
        public SampleResultDialogViewModel(IInstrumentStatusService instrumentStatusService, SampleResultDialogEventArgs<SampleViewModel> args, Window parentWindow) : base(args, parentWindow)
        {
            _instrumentStatusService = instrumentStatusService;
            _sampleHasChanged = false;
            RecordHelper = new ResultRecordHelper();
            ReviewModel = new ReviewModel();
            RunningSamplesState = args.SystemState;
            SampleRecord = args.SampleViewModel.SampleRecord;

            _statusSubscriber = _instrumentStatusService.SubscribeToSystemStatusCallback().Subscribe((OnSampleStateChanged));
        }

        protected override void DisposeManaged()
        {
            _slideShowTimer?.Stop();
            base.DisposeManaged();
        }

        protected override void DisposeUnmanaged()
        {
            ReviewModel?.Dispose();
            RecordHelper?.Dispose();
            _statusSubscriber?.Dispose();
            base.DisposeUnmanaged();
        }

        #region Properties & Fields

        private IDisposable _statusSubscriber;
        private readonly IInstrumentStatusService _instrumentStatusService;
        private bool _sampleHasChanged;
        private bool _enableSampleAnalysisListener;
        private ReanalyzeAction _reanalyzeStatus;
        private SampleRecordDomain _lastSelectedSampleItem;
        private List<KeyValuePair<int, List<histogrambin_t>>> _histogramList = new List<KeyValuePair<int, List<histogrambin_t>>>();
        private DispatcherTimer _slideShowTimer;

        public ResultRecordHelper RecordHelper { get; set; }
        public DetailedResultMeasurementsDomain DetailedMeasurements { get; private set; }

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
                    var totalNumImages = SampleRecord?.ImageIndexList?.Count ?? 0;
                    if (totalNumImages <= 0) return;

                    _slideShowTimer?.Stop();

                    var imgList = SampleRecord?.ImageIndexList?.ToList();
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
            var totalNumImages = SampleRecord.ImageIndexList.Count;
            var imgList = SampleRecord.ImageIndexList.ToList();
            var currentImageIndex = imgList.FindIndex(i => i.Key == SelectedImageIndex.Key);
            // select the next image
            var next = currentImageIndex+1;
            if (next >= totalNumImages)
            {
                // go back to the beginning of the list and end the timer
                SelectedImageIndex = new KeyValuePair<int, string>(1, 1.ToString());
                SlideShowIsRunning = false;
                return;
            }

            SelectedImageIndex = imgList[next];

        }

        public bool IsLoadingIconVisible
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public SystemStatus RunningSamplesState
        {
            get { return GetProperty<SystemStatus>(); }
            set
            {
                SetProperty(value);
                ReanalyzeSampleCommand.RaiseCanExecuteChanged();
                SaveResultCommand.RaiseCanExecuteChanged();
                SignatureCommand.RaiseCanExecuteChanged();
            }
        }

        private SampleRecordDomain _sampleRecord;
        public SampleRecordDomain SampleRecord
        {
            get { return _sampleRecord; }
            set
            {
                if (_sampleRecord == value) return;
                if (_sampleRecord != null) _sampleRecord.PropertyChanged -= OnUpdateSelectedResultRecord;
                _sampleRecord = value;

                RecordHelper.SetImageList(_sampleRecord);
                ShowParameterList = new List<KeyValuePair<string, string>>(RecordHelper.SetListParameter(_sampleRecord, LoggedInUser.CurrentUserId));
                _sampleRecord.UpdateSampleBubbleStatus();
                
                UnsavedReanalyzedSampleRecord = (SampleRecordDomain)_sampleRecord.Clone();

                DisplayPane = DisplayPane.Image;
                SelectedImageIndex = new KeyValuePair<int, string>(1, 1.ToString());
                if (_sampleRecord != null)
                {
                    // This ensures the detailed measurements are loaded
                    // This is needed to be able to click on the image and get details displayed
                    OnUpdateSelectedResultRecord();
                    _sampleRecord.PropertyChanged += OnUpdateSelectedResultRecord;
                }
                NotifyPropertyChanged(nameof(SampleRecord));
                // Set the image type last, this triggers an image load.
                SelectedImageType = ImageType.Annotated;
            }
        }


        private void OnUpdateSelectedResultRecord(object selectedSampleRecord, PropertyChangedEventArgs e)
        {
            if (_sampleRecord?.SelectedResultSummary == null)
                return;
            if (e.PropertyName != nameof(_sampleRecord.SelectedResultSummary))
                return;
            OnUpdateSelectedResultRecord();
        }

        private void OnUpdateSelectedResultRecord()
        {
            if (_sampleRecord?.SelectedResultSummary != null)
            {
                if (!_sampleRecord.SelectedResultSummary.UUID.IsEmpty())
                {
                    DetailedMeasurements = RecordHelper.RetrieveDetailedMeasurementsForResultRecord(_sampleRecord.SelectedResultSummary.UUID);
                }
            }
        }


        public SampleRecordDomain UnsavedReanalyzedSampleRecord
        {
            get { return GetProperty<SampleRecordDomain>(); }
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

        public string SelectedRightClickImageType
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
                if ((_sampleRecord != null) && (_sampleRecord.SampleImageList != null))
                {
                    SetImage(_sampleRecord.SampleImageList.IndexOf(_sampleRecord.SelectedSampleImageRecord));
                }
            }
        }

        public List<KeyValuePair<string, string>> ShowParameterList
        {
            get
            {
                var item = GetProperty<List<KeyValuePair<string, string>>>();
                if (item != null) return item;
                SetProperty(new List<KeyValuePair<string, string>>());
                return GetProperty<List<KeyValuePair<string, string>>>();
            }
            set { SetProperty(value); }
        }

        public KeyValuePair<int, string> SelectedImageIndex
        {
            get { return GetProperty<KeyValuePair<int, string>>(); }
            set
            {
                if (GetProperty<KeyValuePair<int, string>>().Equals(value)) return;

                SetProperty(value);
                if (_sampleRecord != null)
                {
                    var index = _sampleRecord.ImageIndexList.IndexOf(value);
                    SetImage(index);
                }
            }
        }

        public ReviewModel ReviewModel
        {
            get { return GetProperty<ReviewModel>(); }
            private set
            {
                var oldValue = GetProperty<ReviewModel>();
                if (oldValue != null) oldValue.SampleAnalysisOccurred -= HandleSampleAnalysisOccurred;
                SetProperty(value);
                if (value != null) value.SampleAnalysisOccurred += HandleSampleAnalysisOccurred;
            }
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
            set { SetProperty(value); }
        }

        public bool IsReanalyzeEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsTempReanalyzeSuccess
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                DispatcherHelper.ApplicationExecute(() =>
                {
                    SignatureCommand.RaiseCanExecuteChanged();
                    SaveResultCommand.RaiseCanExecuteChanged();
                });
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

        // Detailed measurements of the blob closest to where the user last
        // tapped; null if the image has not yet been tapped or if the user
        // last tapped on a cluster instead of a blob.
        public BlobMeasurementDomain LastSelectedBlob
        {
            get { return GetProperty<BlobMeasurementDomain>(); }
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
                    SetProperty(value);
                    UpdateBlobData();
                }
                else
                {
                    Log.Info("No cells present in current image.");
                    SetProperty((BlobMeasurementDomain) null); // `value` *may* not be `null` (see extra conditions above)
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

        public DrawPoint LastTappedPixel
        {
            get { return GetProperty<DrawPoint>(); }
            set
            {
                SetProperty(value);
                Log.Info($"User tapped on image coordinates [{value}]");
                SetLastTappedObject();
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

        public bool IsBubblePopupOpen
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public BubbleStatusViewModel BubbleStatusViewModel
        {
            get { return GetProperty<BubbleStatusViewModel>(); }
            set { SetProperty(value); }
        }

        public List<BarGraphDomain> GraphViewList
        {
            get { return GetProperty<List<BarGraphDomain>>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Event Handlers

        private void HandleSampleAnalysisOccurred(object sender, ApiEventArgs<HawkeyeError, uuidDLL, ResultRecordDomain> e)
        {
            if (!_enableSampleAnalysisListener) return;
            _enableSampleAnalysisListener = false;

            Log.Debug($"ReanalyzeSample Callback::hawkeyeStatus: {e.Arg1}");

            try
            {
                if (e.Arg1.Equals(HawkeyeError.eSuccess))
                {
                    switch (_reanalyzeStatus)
                    {
                        case ReanalyzeAction.Save:
                            if (SampleRecord.ResultSummaryList.Count > 1)
                            {
                                SampleRecord.ResultSummaryList.RemoveAt(SampleRecord.ResultSummaryList.Count - 1);
                                UpdateSample(e.Arg3);
                            }
                            ShowParameterList = new List<KeyValuePair<string, string>>(
                                RecordHelper.SetListParameter(SampleRecord, LoggedInUser.CurrentUserId));
                            PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_ResultRecordSaved"));
                            IsTempReanalyzeSuccess = false;
                            _sampleHasChanged = true;
                            break;
                        case ReanalyzeAction.Reanalyze:
                            // Update the sample with Arg3 (new result record)
                            UpdateSample(e.Arg3);
                            ShowParameterList = new List<KeyValuePair<string, string>>(
                                RecordHelper.SetListParameter(SampleRecord, LoggedInUser.CurrentUserId));
                            IsTempReanalyzeSuccess = true;
                            break;
                    }
                }
                else
                {
                    ApiHawkeyeMsgHelper.ErrorCommon(e.Arg1);
                }

                IsLoadingIconVisible = false;
            }
            catch (Exception ex)
            {
                IsLoadingIconVisible = false;
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_HANDLE_SAMPLE_ANALYSIS_OCCURED"));
            }
        }

        private void OnSampleStateChanged(SystemStatusDomain sysStatus)
        {
            DispatcherHelper.ApplicationExecute(() => RunningSamplesState = sysStatus.SystemStatus);
        }

        #endregion

        #region Commands

        protected override void OnCancel()
        {
            Close(_sampleHasChanged); // the caller might need to update refresh it's view
        }

        #region Reanalyze Sample Command

        private RelayCommand _reanalyzeSampleCommand;
        public RelayCommand ReanalyzeSampleCommand => _reanalyzeSampleCommand ?? (_reanalyzeSampleCommand = new RelayCommand(PerformReanalyzeSample, CanPerformReanalyzeSample));

        private bool CanPerformReanalyzeSample()
        {
            return LoggedInUser.CurrentUserRoleId != UserPermissionLevel.eNormal &&
                   (RunningSamplesState == SystemStatus.Faulted || RunningSamplesState == SystemStatus.Idle);
        }

        private void PerformReanalyzeSample()
        {
            try
            {
                var args = new SelectCellTypeEventArgs(LanguageResourceHelper.Get("LID_POPUPHeader_SelectCellTypeToReanlyse"), false,
                    SampleRecord.SelectedResultSummary.CellTypeDomain);
                
                var result = DialogEventBus.SelectCellTypeDialog(this, args);
                if (result == true && args.SelectedCellTypeDomain is CellTypeDomain selectedCellType)
				{
					ReanalyzeSample(selectedCellType);
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_REANALYZE_SAMPLE"));
            }
        }

        #endregion

        #region Export Selected Sample Command

        private RelayCommand _exportSelectedSampleCommand;
        public RelayCommand ExportSelectedSampleCommand => _exportSelectedSampleCommand ?? (_exportSelectedSampleCommand = new RelayCommand(ExportSelectedSample));

        private void ExportSelectedSample()
        {
            try
            {
                if (SampleRecord == null) return;

                var sampleList = new List<SampleRecordDomain> { SampleRecord };
                var defaultFileName = SampleRecord.SampleIdentifier + "_" + Misc.ConvertToFileNameFormat(SampleRecord.SelectedResultSummary.RetrieveDate);
                var first = ResultRecordHelper.ExportCompleteRunResult(sampleList).FirstOrDefault();
                ExportModel.PromptAndExportSampleToCsv(first, SampleRecord, defaultFileName);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_EXPORT_DATA"));
            }
        }

        #endregion

        #region Signature Command

        private RelayCommand _signatureCommand;
        public RelayCommand SignatureCommand => _signatureCommand ?? (_signatureCommand = new RelayCommand(SignSample, CanSignSample));

        private bool CanSignSample()
        {
            return IsSecurityTurnedOn &&
                   !LoggedInUser.CurrentUserId.Equals(ApplicationConstants.ServiceUser) &&
                   //// Leave for future reference or use
                   //!LoggedInUser.CurrentUserId.Equals(ApplicationConstants.ServiceAdmin) &&
                   !LoggedInUser.CurrentUserId.Equals(ApplicationConstants.AutomationClient) &&
                   !IsTempReanalyzeSuccess;
        }

        private void SignSample()
        {
            try
            {
                var args = new AddSignatureEventArgs(ReviewModel.RetrieveSignatureDefinitions());
                if (DialogEventBus.AddSignature(this, args) == true)
                {
                    OnAddSignature(args.SignatureSelected);
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex,
                    LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_SIGNATURE_SAMPLE"));
            }
        }

        private void OnAddSignature(ISignature selectedSign)
        {
            var signStatus = ReviewModel.SignResultRecord(SampleRecord.SelectedResultSummary.UUID,
                selectedSign.SignatureIndicator, (ushort)selectedSign.SignatureIndicator.Length);
            if (signStatus.Equals(HawkeyeError.eSuccess))
            {
                var resultSummary = ReviewModel.RetrieveResultSummary(SampleRecord.SelectedResultSummary.UUID);
                SampleRecord.SelectedResultSummary.SignatureList = resultSummary.SignatureList;
                SampleRecord.SelectedResultSummary.SelectedSignature = resultSummary.SignatureList.Last();

                var str = LanguageResourceHelper.Get("LID_StatusBar_ResultRecordSigned");
                _sampleHasChanged = true;
                PostToMessageHub(str);
                Log.Debug(str);
            }
            else
            {
                DisplayErrorDialogByApi(signStatus);
            }
        }

        #endregion

        #region Save Result Command

        private RelayCommand _saveResultCommand;
        public RelayCommand SaveResultCommand => _saveResultCommand ?? (_saveResultCommand = new RelayCommand(PerformSaveResult, CanPerformSaveResult));

        private bool CanPerformSaveResult()
        {
            return RunningSamplesState == SystemStatus.Faulted || RunningSamplesState == SystemStatus.Idle;
        }

        private void PerformSaveResult()
        {
            try
            {
                IsLoadingIconVisible = true;
                _enableSampleAnalysisListener = true;
                _reanalyzeStatus = ReanalyzeAction.Save;

                var cellType = CellTypeFacade.Instance.GetCellTypeCopy_BECall(SampleRecord.SelectedResultSummary.CellTypeDomain.CellTypeName);
                var analysis = AnalysisModel.GetAllAnalyses().FirstOrDefault();
                var imageStatusDifferent = ReviewModel.IsImageStatusDifferent(UnsavedReanalyzedSampleRecord, cellType);
                var reanalyzeStatus = ReviewModel.ReanalyzeSample(SampleRecord.UUID, cellType.CellTypeIndex, analysis.AnalysisIndex, imageStatusDifferent);

                if (!reanalyzeStatus.Equals(HawkeyeError.eSuccess))
                {
                    IsLoadingIconVisible = false;
                    _enableSampleAnalysisListener = false;
                    ApiHawkeyeMsgHelper.ErrorCommon(reanalyzeStatus);
                }
            }
            catch (Exception ex)
            {
                IsLoadingIconVisible = false;
                IsTempReanalyzeSuccess = false;
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_REANALYZE_SAMPLE"));
            }
        }

        #endregion

        #region Image Expand Command

        private RelayCommand _imageExpandCommand;
        public RelayCommand ImageExpandCommand => _imageExpandCommand ?? (_imageExpandCommand = new RelayCommand(OnExpandImage));

        private void OnExpandImage()
        {
            OnExpandView(false);
        }

        #endregion

        #region Graph Expand Command

        private RelayCommand _graphExpandCommand;
        public RelayCommand GraphExpandCommand => _graphExpandCommand ?? (_graphExpandCommand = new RelayCommand(OnExpandGraph));

        private void OnExpandGraph()
        {
            OnExpandView(true);
        }

        #endregion

        #region Select Display Pane Command (Graph/Image)

        private RelayCommand _selectDisplayPaneCommand;
        public RelayCommand SelectDisplayPaneCommand => _selectDisplayPaneCommand ?? (_selectDisplayPaneCommand = new RelayCommand(SelectDisplayPane));

        private void SelectDisplayPane(object objItem)
        {
            try
            {
                switch (objItem as string)
                {
                    case "GraphSelected":
                        DisplayPane = DisplayPane.Graph;
                        SetGraph();
                        break;
                    case "ImageSelected":
                        DisplayPane = DisplayPane.Image;
                        break;
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_SELECT_ITEM"));
            }
        }

        #endregion

        #region Bubble Status Command

        private RelayCommand _bubbleStatusCommand;
        public RelayCommand BubbleStatusCommand => _bubbleStatusCommand ?? (_bubbleStatusCommand = new RelayCommand(OnBubbleStatusUpdate));

        private void OnBubbleStatusUpdate()
        {
            IsBubblePopupOpen = !IsBubblePopupOpen;
            if (BubbleStatusViewModel == null) BubbleStatusViewModel = new BubbleStatusViewModel();
            BubbleStatusViewModel.UpdateBubbleStatus(SampleRecord, false);
        }

        #endregion

        #region Play SlideShow Command

        private RelayCommand _playSlideShowCommand;
        public RelayCommand PlaySlideShowCommand => _playSlideShowCommand ?? (_playSlideShowCommand = new RelayCommand(PlaySlideShow, CanPlaySlideShow));

        private bool CanPlaySlideShow()
        {
            var totalNumImages = SampleRecord?.ImageIndexList?.Count ?? 0;
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
            var totalNumImages = SampleRecord?.ImageIndexList?.Count ?? 0;
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

        private void OnChannelTraversal(AdjustValue adjustVal)
        {
            try
            {
                if (SampleRecord?.SelectedSampleImageRecord == null || SampleRecord.SampleImageList == null || !SampleRecord.SampleImageList.Any())
                    return;
                
                int imgCount = SampleRecord.ImageIndexList.Count;
                var imgList = SampleRecord.ImageIndexList.ToList();
                var imgIdx = imgList.FindIndex(i => i.Key == SelectedImageIndex.Key);
                int nextIdx;
                switch (adjustVal)
                {
                    case AdjustValue.Idle:
                        break;
                    case AdjustValue.Left:
                        // select the previous image
                        nextIdx = imgIdx - 1;
                        if (nextIdx >= 0)
                        {
                            SelectedImageIndex = imgList[nextIdx];
                        }
                        break;
                    case AdjustValue.Right:
                        // select the next image
                        nextIdx = imgIdx + 1;
                        if (nextIdx < imgCount)
                        {
                            SelectedImageIndex = imgList[nextIdx];
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_CHANNEL_TRAVERSAL"));
            }
        }

        private void OnExpandView(bool isGraph)
        {
            try
            {
                if (SampleRecord == null) return;
                SlideShowIsRunning = false;
                if (isGraph)
                {
                    var args = new ExpandedImageGraphEventArgs(ExpandedContentType.BarChart, SelectedRightClickImageType,
                        GraphViewList?.Cast<object>()?.ToList(), Graph, GraphViewList?.IndexOf(Graph) ?? -1);
                    DialogEventBus.ExpandedImageGraph(this, args);
                    Graph = GraphViewList?.ElementAt(args.SelectedGraphIndex);
                }
                else
                {
                    var args = new ExpandedImageGraphEventArgs(SelectedImageType, SelectedRightClickImageType,
                        SampleRecord?.SampleImageList?.Cast<object>()?.ToList(),
                        SampleRecord, (int)SampleRecord.SelectedSampleImageRecord.SequenceNumber);
                    DialogEventBus.ExpandedImageGraph(this, args);
                    var newIndex = args.SelectedImageIndex;
                    var imgList = SampleRecord.ImageIndexList.ToList();
                    SelectedImageIndex = imgList[newIndex];
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_EXPAND_VIEW"));
            }
        }

        private void SetGraph()
        {
            _histogramList = RecordHelper.GetHistogramList(SampleRecord.SelectedResultSummary.UUID);
            GetGraphDetails();
        }

        private void GetGraphDetails()
        {
            if (_lastSelectedSampleItem?.UUID.ToString() == SampleRecord?.UUID.ToString() && GraphViewList.Any())
                return;

            _lastSelectedSampleItem = SampleRecord;
            var graphHelper = new GraphHelper();
            GraphViewList = graphHelper.CreateGraph(SampleRecord, _histogramList, true);
            if (GraphViewList != null && GraphViewList.Any())
                Graph = GraphViewList.FirstOrDefault();
            NotifyAllPropertiesChanged();
        }

        private void ReanalyzeSample(CellTypeDomain selectedCellType)
        {
            // todo: move this to the Model
            try
            {
                var newCellType = SelectCellTypeModel.GetReanalyzeSampleCellType(selectedCellType.CellTypeIndex);
                if (newCellType == null)
                {
                    throw new Exception($"Failed to get sample cell type to reanalyze");
                }

                var analysisDomain = AnalysisModel.GetAnalysisDomains(false).FirstOrDefault();
                if (analysisDomain == null)
                {
                    throw new Exception($"Failed to get analysis type to reanalyze sample");
                }

                IsLoadingIconVisible = true;
                IsReanalyzeEnable = false;
                _enableSampleAnalysisListener = true;
                _reanalyzeStatus = ReanalyzeAction.Reanalyze;

                var fromImage = ReviewModel.IsImageStatusDifferent(UnsavedReanalyzedSampleRecord, newCellType);
                var reanalyzeStatus = ReviewModel.ReanalyzeSample(SampleRecord.UUID, newCellType.CellTypeIndex, 
                    analysisDomain.AnalysisIndex, fromImage);

                if (reanalyzeStatus.Equals(HawkeyeError.eSuccess))
                {
                    PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_SampleReanalyzed"));
                }
                else
                {
                    IsTempReanalyzeSuccess = false;
                    IsLoadingIconVisible = false;
                    _enableSampleAnalysisListener = false;
                    ApiHawkeyeMsgHelper.ErrorCommon(reanalyzeStatus);
                }
            }
            catch (Exception ex)
            {
                IsLoadingIconVisible = false;
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_REANALYZE_SAMPLE"));
            }
        }

        private void UpdateSample(ResultRecordDomain recordDomain)
        {
            var selectedImgIndex = SampleRecord.SampleImageList.IndexOf(SampleRecord.SelectedSampleImageRecord);

            DispatcherHelper.ApplicationExecute(() =>
            {
                SampleRecord.ResultSummaryList.Add(recordDomain.ResultSummary);
                SampleRecord.SelectedResultSummary = recordDomain.ResultSummary;
                foreach (var sample in SampleRecord.SampleImageList)
                {
                    sample.ResultPerImage = recordDomain.ResultPerImage[(int)sample.SequenceNumber - 1];
                }

                // This updates the displayed image.
                SetImage(selectedImgIndex);
            });

            IsReanalyzeEnable = true;
        }

        private void SetImage(int imageIndex)
        {
            if (SampleRecord?.SampleImageList == null || !SampleRecord.SampleImageList.Any() || imageIndex < 0)
                return;

            var sampleImage = SampleRecord.SampleImageList[imageIndex];
            sampleImage.TotalCumulativeImage = Convert.ToInt32(SampleRecord.SelectedResultSummary.CumulativeResult.TotalCumulativeImage);
            var imgData = RecordHelper.GetImage(SelectedImageType, sampleImage.BrightFieldId, SampleRecord.SelectedResultSummary.UUID);

            sampleImage.ImageSet = imgData;
            SampleRecord.SelectedSampleImageRecord = sampleImage;
            NotifyAllPropertiesChanged();
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

        #endregion
    }
}