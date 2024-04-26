using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.Common;
using ScoutDomains.RunResult;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutModels.Home.QueueManagement;
using ScoutModels.Review;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using ScoutUtilities.Interfaces;
using ScoutUtilities.Structs;
using ScoutUtilities.UIConfiguration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using HawkeyeCoreAPI;
using HawkeyeCoreAPI.Facade;

namespace ScoutViewModels.ViewModels.Common
{
    public class ReviewViewModel : ImageViewModel
    {
        #region Constructor 

        public ReviewViewModel() : base(new ResultRecordHelper())
        {
            ReviewModel = new ReviewModel();
            Initialize();
        }

        protected override void DisposeUnmanaged()
        {
            ReviewModel?.Dispose();
            base.DisposeUnmanaged();
        }

        private void Initialize()
        {
            IsFromReview = true;
            ImageActive = true;
            IsNoImage = true;
            SetDefaultDates();
            SampleRecordList = new ObservableCollection<SampleRecordDomain>();

            ShowParameterList = new List<KeyValuePair<string, string>>(
                ResultRecordHelper.GetShowParameterList(new GenericDataDomain(), LoggedInUser.CurrentUserId));
            SelectedRightClickImageType = ApplicationConstants.ImageViewRightClickMenuImageFitSize;
        }

        #endregion

        #region Properties & Fields

        private SampleRecordDomain _lastSelectedSampleItem;
        private bool _enableSampleAnalysisListener;
        private ReanalyzeAction _reanalyzeStatus;
        private List<KeyValuePair<int, List<histogrambin_t>>> _histogramList = new List<KeyValuePair<int, List<histogrambin_t>>>();

        public bool IsSummaryExportEnable => SampleRecordList != null && SampleRecordList.Any();

        public UpdateSelectedSampleCell UpdateSelectedSampleCell { get; set; } // set in CellTypeViewModel

        public SampleRecordDomain SelectedSampleRecord
        {
            get { return GetProperty<SampleRecordDomain>(); }
            set { SetProperty(value); }
        }

        public SampleRecordDomain UnsavedReanalyzedSampleRecord
        {
            get { return GetProperty<SampleRecordDomain>(); }
            set { SetProperty(value); }
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

        public SignatureViewModel Signature
        {
            get
            {
                var item = GetProperty<SignatureViewModel>();
                if (item != null) return item;
                SetProperty(new SignatureViewModel(ReviewModel)); // todo: see if we need to keep this as a property or we can set it when we change ReviewModel
                return GetProperty<SignatureViewModel>();
            }
            set { SetProperty(value); }
        }

        public ObservableCollection<SampleRecordDomain> SampleRecordList
        {
            get
            {
                var item = GetProperty<ObservableCollection<SampleRecordDomain>>();
                if (item != null) return item;
                SetProperty(new ObservableCollection<SampleRecordDomain>());
                return GetProperty<ObservableCollection<SampleRecordDomain>>();
            }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(IsSummaryExportEnable));
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

        public bool IsReviewEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsNormalUserActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsReanalyzeEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsExportEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsNoImage
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsTempReanalyzeSuccess
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool GraphActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsSignatureEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool ImageActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public DateTime FromDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public DateTime ToDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public List<BarGraphDomain> GraphViewList
        {
            get { return GetProperty<List<BarGraphDomain>>(); }
            set { SetProperty(value); }
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

        #endregion

        #region Commands

        #region Get Sample List Command

        private RelayCommand _getSampleListCommand;
        public RelayCommand GetSampleListCommand => _getSampleListCommand ?? (_getSampleListCommand = new RelayCommand(GetWorkQueueList));

        private void GetWorkQueueList()
        {
            try
            {
                if (!Validation.OnDateValidate(FromDate, ToDate)) return;

                GetSampleList();
                NotifyPropertyChanged(nameof(IsSummaryExportEnable));
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_HANDLE_GET_WORK_QUEUE_LIST"));
            }
        }

        #endregion

        #region Reanalyze Sample Command

        private RelayCommand _reanalyzeSampleCommand;
        public RelayCommand ReanalyzeSampleCommand => _reanalyzeSampleCommand ?? (_reanalyzeSampleCommand = new RelayCommand(PerformReanalyzeSample));

        private void PerformReanalyzeSample()
        {
            try
            {
                var args = new SelectCellTypeEventArgs(LanguageResourceHelper.Get("LID_POPUPHeader_SelectCellTypeToReanlyse"), false,
                    SelectedSampleFromList.SelectedResultSummary.CellTypeDomain);
                var result = DialogEventBus.SelectCellTypeDialog(this, args);

                if (result == true && args.SelectedCellTypeDomain is CellTypeDomain selectedCellType &&
                    selectedCellType.CellTypeIndex != SelectedSampleFromList.SelectedResultSummary.CellTypeDomain.CellTypeIndex)
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
                if (SelectedSampleFromList == null) return;

                var sampleList = new List<SampleRecordDomain> { SelectedSampleFromList };
                var defaultFileName = SelectedSampleFromList.SampleIdentifier + "_" + Misc.ConvertToFileNameFormat(SelectedSampleFromList.SelectedResultSummary.RetrieveDate);
                var first = ResultRecordHelper.ExportCompleteRunResult(sampleList).FirstOrDefault();
                ExportModel.PromptAndExportSampleToCsv(first, SelectedSampleFromList, defaultFileName);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_EXPORT_DATA"));
            }
        }

        #endregion

        #region Signature Command

        private RelayCommand _signatureCommand;
        public RelayCommand SignatureCommand => _signatureCommand ?? (_signatureCommand = new RelayCommand(SignatureSample));

        private void SignatureSample()
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

        #endregion

        #region Save Result Command

        private RelayCommand _saveResultCommand;
        public RelayCommand SaveResultCommand => _saveResultCommand ?? (_saveResultCommand = new RelayCommand(OnSaveResult));

        private void OnSaveResult()
        {
            try
            {
                SetLoadingIndicator(true);
                _enableSampleAnalysisListener = true;
                _reanalyzeStatus = ReanalyzeAction.Save;

                var cellType = CellTypeFacade.Instance.GetCellTypeCopy_BECall(SelectedSampleFromList.SelectedResultSummary.CellTypeDomain.CellTypeName);
                var analysis = AnalysisModel.GetAllAnalyses().FirstOrDefault();
                var imageStatusDifferent = ReviewModel.IsImageStatusDifferent(UnsavedReanalyzedSampleRecord, cellType);
                var reanalyzeStatus = ReviewModel.ReanalyzeSample(SelectedSampleFromList.UUID, cellType.CellTypeIndex, analysis.AnalysisIndex, imageStatusDifferent);

                if (!reanalyzeStatus.Equals(HawkeyeError.eSuccess))
                {
                    SetLoadingIndicator(false);
                    _enableSampleAnalysisListener = false;
                    ApiHawkeyeMsgHelper.ErrorCommon(reanalyzeStatus);
                }
            }
            catch (Exception ex)
            {
                SetLoadingIndicator(false);
                IsTempReanalyzeSuccess = false;
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_REANALYZE_SAMPLE"));
            }
        }

        #endregion

        #region Open Sample Command

        private RelayCommand _openSampleCommand;
        public RelayCommand OpenSampleCommand => _openSampleCommand ?? (_openSampleCommand = new RelayCommand(OpenReviewSampleExecute));

        private async void OpenReviewSampleExecute()
        {
            SetLoadingIndicator(true);
            try
            {
                await Task.Run(OpenReviewSample);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
            finally
            {
                SetLoadingIndicator(false);
            }
        }

        private void OpenReviewSample()
        {
            var args = new OpenSampleEventArgs();
            if (DialogEventBus.OpenSampleDialog(this, args) == true && args.SelectedSampleRecord != null)
            {
                SelectedSampleRecord = args.SelectedSampleRecord as SampleRecordDomain;
                OnSelectedSample();
            }
            SetLoadingIndicator(false);
        }

        private void OnSelectedSample()
        {
            try
            {
                RecordHelper.SetImageList(SelectedSampleRecord);
                SelectedImageType = ImageType.Annotated;
                SelectedSampleRecord.UpdateSampleBubbleStatus();

                SelectedSampleFromList = (SampleRecordDomain)SelectedSampleRecord.Clone();
                UnsavedReanalyzedSampleRecord = (SampleRecordDomain)SelectedSampleRecord.Clone();

                SetSelectedImage(SelectedSampleFromList);

                if (SelectedSampleFromList.ResultSummaryList == null || !SelectedSampleFromList.ResultSummaryList.Any())
                {
                    SelectedSampleFromList = QueueResultModel.GetSampleRecordByUUID(SelectedSampleFromList.UUID);
                    RecordHelper.SetImageList(SelectedSampleFromList);
                    SelectedSampleFromList.SortImages();
                }
                var param = RecordHelper.SetListParameter(SelectedSampleFromList, LoggedInUser.CurrentUserId);
                ShowParameterList = new List<KeyValuePair<string, string>>(param);

                UpdateSelectedSampleCell?.Invoke(SelectedSampleFromList.SelectedResultSummary.CellTypeDomain);

                IsExportEnable = IsReviewEnable = true;
                IsTempReanalyzeSuccess = IsNoImage = GraphActive = false;
                ImageActive = true;

                SetAdminAccess();
                SetSignatureVisibility();

                _lastSelectedSampleItem = null;
                NotifyAllPropertiesChanged();
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_SELECTED_SAMPLE"));
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

        #region Selected Sample To Open Command

        #endregion

        #region Select Item (Graph/Image) Command

        private RelayCommand _selectItemCommand;
        public RelayCommand SelectItemCommand => _selectItemCommand ?? (_selectItemCommand = new RelayCommand(SelectItem));

        private void SelectItem(object objItem)
        {
            try
            {
                switch (objItem as string)
                {
                    case "GraphSelected":
                        GraphActive = true;
                        ImageActive = false;
                        SetGraph();
                        break;
                    case "ImageSelected":
                        ImageActive = true;
                        GraphActive = false;
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
            BubbleStatusViewModel.UpdateBubbleStatus(SelectedSampleFromList, false);
        }

        #endregion

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
                            if (SelectedSampleFromList.ResultSummaryList.Count > 1)
                            {
                                SelectedSampleFromList.ResultSummaryList.RemoveAt(SelectedSampleFromList.ResultSummaryList.Count - 1);
                                UpdateSample(e.Arg3);
                            }
                            SetSignatureVisibility();
                            ShowParameterList = new List<KeyValuePair<string, string>>(
                                RecordHelper.SetListParameter(SelectedSampleFromList, LoggedInUser.CurrentUserId));
                            PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_ResultRecordSaved"));
                            break;
                        case ReanalyzeAction.Reanalyze:
                            // Update the sample with Arg3 (new result record)
                            UpdateSample(e.Arg3);
                            ShowParameterList = new List<KeyValuePair<string, string>>(
                                RecordHelper.SetListParameter(SelectedSampleFromList, LoggedInUser.CurrentUserId));
                            IsTempReanalyzeSuccess = true;
                            IsSignatureEnable = false;
                            break;
                    }
                }
                else
                {
                    ApiHawkeyeMsgHelper.ErrorCommon(e.Arg1);
                }

                SetLoadingIndicator(false);
            }
            catch (Exception ex)
            {
                SetLoadingIndicator(false);
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_HANDLE_SAMPLE_ANALYSIS_OCCURED"));
            }
        }

        #endregion

        #region Private Methods

        private void SetGraph()
        {
            _histogramList = RecordHelper.GetHistogramList(SelectedSampleFromList.SelectedResultSummary.UUID);
            GetGraphDetails();
        }

        private void OnExpandView(bool isGraph)
        {
            try
            {
                if (SelectedSampleFromList == null) return;
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
                        SelectedSampleFromList?.SampleImageList?.Cast<object>()?.ToList(),
                        SelectedSampleFromList, (int)SelectedSampleFromList.SelectedSampleImageRecord.SequenceNumber);
                    DialogEventBus.ExpandedImageGraph(this, args);
                    var newIndex = args.SelectedImageIndex;
                    var imgList = SelectedSampleFromList.ImageIndexList.ToList();
                    SelectedImageIndex = imgList[newIndex];
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_EXPAND_VIEW"));
            }
        }

        private void SetSignatureVisibility()
        {
            IsSignatureEnable = false;
            if (IsSecurityTurnedOn)
            {
                var signatureList = ReviewModel.RetrieveSignatureDefinitions();
                if (signatureList.Any())
                {
                    IsSignatureEnable = !LoggedInUser.CurrentUserId.Equals(ApplicationConstants.ServiceUser);
                }
            }
        }

        private void GetGraphDetails()
        {
            if (_lastSelectedSampleItem?.UUID.ToString() == SelectedSampleRecord?.UUID.ToString() && GraphViewList.Any())
                return;

            _lastSelectedSampleItem = SelectedSampleRecord;
            var graphHelper = new GraphHelper();
            GraphViewList = graphHelper.CreateGraph(SelectedSampleFromList, _histogramList, true);
            if (GraphViewList != null && GraphViewList.Any())
                Graph = GraphViewList.FirstOrDefault();
        }

        private void SetAdminAccess()
        {
            switch (LoggedInUser.CurrentUserRoleId)
            {
                case UserPermissionLevel.eNormal:
                    IsNormalUserActive = false;
                    break;
                case UserPermissionLevel.eElevated:
                case UserPermissionLevel.eAdministrator:
                case UserPermissionLevel.eService:
                    IsNormalUserActive = true;
                    break;
            }
        }

        private void OnAddSignature(ISignature selectedSign)
        {
            var signStatus = ReviewModel.SignResultRecord(SelectedSampleFromList.SelectedResultSummary.UUID,
                selectedSign.SignatureIndicator, (ushort)selectedSign.SignatureIndicator.Length);
            if (signStatus.Equals(HawkeyeError.eSuccess))
            {
                var resultSummary = ReviewModel.RetrieveResultSummary(SelectedSampleFromList.SelectedResultSummary.UUID);
                SelectedSampleFromList.SelectedResultSummary.SignatureList = resultSummary.SignatureList;
                SelectedSampleFromList.SelectedResultSummary.SelectedSignature = resultSummary.SignatureList.Last();

                var str = LanguageResourceHelper.Get("LID_StatusBar_ResultRecordSigned");
                PostToMessageHub(str);
                Log.Debug(str);
            }
            else
                DisplayErrorDialogByApi(signStatus);
        }

        private void SetSelectedImage(SampleRecordDomain sampleRecord)
        {
            if (sampleRecord.SampleImageList != null && sampleRecord.SampleImageList.Any())
            {
                var sampleImage = sampleRecord.SampleImageList.FirstOrDefault();
                if (sampleImage != null)
                {
                    sampleImage.TotalCumulativeImage = Convert.ToInt32(sampleRecord.SelectedResultSummary
                        .CumulativeResult.TotalCumulativeImage);
                    var imgData = RecordHelper.GetImage(SelectedImageType, sampleImage.BrightFieldId,
                        sampleRecord.SelectedResultSummary.UUID);
                    sampleImage.ImageSet = imgData;
                    SelectedSampleFromList.SelectedSampleImageRecord = sampleImage;
                }
            }
        }

        private void GetSampleList()
        {
            var startDate = DateTimeConversionHelper.DateTimeToUnixSecondRounded(FromDate);
            var endDate = DateTimeConversionHelper.DateTimeToEndOfDayUnixSecondRounded(ToDate);
            var sampleRecordList = ReviewModel.GetFlattenedResultRecordList_wrappedInSampleRecords(startDate, endDate, LoggedInUser.CurrentUserId);

            if (sampleRecordList == null || !sampleRecordList.Any())
            {
                SampleRecordList = new ObservableCollection<SampleRecordDomain>();
                return;
            }

            var maximumSearchCount = UISettings.MaximumSearchCountForOpenSample;
            if (sampleRecordList.Count <= maximumSearchCount)
            {
                SampleRecordList = sampleRecordList.OrderByDescending(s => s.RetrieveDate).ToObservableCollection();
                SelectedSampleRecord = sampleRecordList.LastOrDefault();
            }
            else if (sampleRecordList.Count > maximumSearchCount)
            {
                RefineSearch(maximumSearchCount);
            }

            ReviewModel.SetSampleHierarchy(SampleRecordList);
        }

        private void SetDefaultDates()
        {
            FromDate = DateTime.Today.AddDays(ApplicationConstants.DefaultFilterFromDaysToSubtract);
            ToDate = DateTime.Today;
        }

        private void RefineSearch(int maximumSearchCount)
        {
            var message = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_RefineSearch"), Misc.ConvertToString(maximumSearchCount));
            DialogEventBus.DialogBoxOk(this, message);
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

                SetLoadingIndicator(true);
                IsReanalyzeEnable = false;
                _enableSampleAnalysisListener = true;
                _reanalyzeStatus = ReanalyzeAction.Reanalyze;

                var fromImage = ReviewModel.IsImageStatusDifferent(UnsavedReanalyzedSampleRecord, newCellType);
                var reanalyzeStatus = ReviewModel.ReanalyzeSample(SelectedSampleFromList.UUID, newCellType.CellTypeIndex, analysisDomain.AnalysisIndex, fromImage);

                if (reanalyzeStatus.Equals(HawkeyeError.eSuccess))
                {
                    PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_SampleReanalyzed"));
                }
                else
                {
                    IsTempReanalyzeSuccess = false;
                    SetLoadingIndicator(false);
                    _enableSampleAnalysisListener = false;
                    ApiHawkeyeMsgHelper.ErrorCommon(reanalyzeStatus);
                }
            }
            catch (Exception ex)
            {
                SetLoadingIndicator(false);
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_REANALYZE_SAMPLE"));
            }
        }

        #endregion

        #region Public Methods

        public void SetLoadingIndicator(bool status)
        {
            MessageBus.Default.Publish(new Notification<bool>(status, MessageToken.AdornerVisible));
        }

        public void UpdateSample(ResultRecordDomain recordDomain)
        {
            var selectedImgIndex = SelectedSampleFromList.SampleImageList.IndexOf(SelectedSampleFromList.SelectedSampleImageRecord);

            DispatcherHelper.ApplicationExecute(() =>
            {
                SelectedSampleFromList.ResultSummaryList.Add(recordDomain.ResultSummary);
                SelectedSampleFromList.SelectedResultSummary = recordDomain.ResultSummary;
                foreach (var sample in SelectedSampleFromList.SampleImageList)
                {
                    sample.ResultPerImage = recordDomain.ResultPerImage[(int)sample.SequenceNumber - 1];
                }

                // This updates the displayed image.
                SetImage(selectedImgIndex);
            });

            IsReanalyzeEnable = true;
            if (GraphActive)
            {
                SetGraph();
            }
        }

        public void RetainOriginalResult()
        {
            if (SelectedSampleFromList?.SampleImageList == null || SelectedSampleRecord?.SelectedResultSummary == null)
                return;

            var imageIndex = SelectedSampleFromList.SampleImageList.IndexOf(SelectedSampleFromList.SelectedSampleImageRecord);
            var recordDomain = RecordHelper.OnSelectedSampleRecordForReport(SelectedSampleRecord.SelectedResultSummary.UUID);

            foreach (var sample in SelectedSampleRecord.SampleImageList)
            {
                sample.ResultPerImage = recordDomain.ResultPerImage[(int)sample.SequenceNumber - 1];
            }

            SelectedSampleFromList = (SampleRecordDomain)SelectedSampleRecord.Clone();
            SetImage(imageIndex);
            ShowParameterList = new List<KeyValuePair<string, string>>(
                RecordHelper.SetListParameter(SelectedSampleFromList, LoggedInUser.CurrentUserId));

            if (GraphActive)
                SetGraph();
        }

        #endregion
    }
}