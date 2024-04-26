using ApiProxies.Generic;
using HawkeyeCoreAPI.Facade;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.Common;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Admin;
using ScoutModels.Common;
using ScoutModels.QualityControl;
using ScoutModels.Review;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using ScoutUtilities.Interfaces;
using ScoutUtilities.Structs;
using ScoutViewModels.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ScoutServices.Interfaces;
using ScoutViewModels.ViewModels.Tabs.SettingsPanel;
using ScoutServices.Enums;
using ScoutViewModels.Interfaces;

namespace ScoutViewModels.ViewModels.QualityControl
{
    public class QualityControlViewModel : ImageViewModel
    {
        #region Constructor

        public QualityControlViewModel(ILockManager lockManager, IScoutViewModelFactory viewModelFactory) : base(new ResultRecordHelper())
        {
            _histogramList = new List<KeyValuePair<int, List<histogrambin_t>>>();
            _reviewModel = new ReviewModel();
            _lockManager = lockManager;
            _viewModelFactory = viewModelFactory;
            _lockStateSubscriber = _lockManager.SubscribeStateChanges().Subscribe(LockStatusChanged);

            SetDefaultValue();
            SetSignatureVisibility();
            _qcSubscriber = MessageBus.Default.Subscribe<Notification>(OnQualityControlsUpdated);
        }

        protected override void DisposeUnmanaged()
        {
            _reviewModel?.Dispose();
            MessageBus.Default.UnSubscribe(ref _qcSubscriber);
            _lockStateSubscriber.Dispose();
            base.DisposeUnmanaged();
        }

        private readonly ILockManager _lockManager;
        private readonly IScoutViewModelFactory _viewModelFactory;
        private Subscription<Notification> _qcSubscriber;
        private IDisposable _lockStateSubscriber;

        private void SetDefaultValue()
        {
            try
            {
                _isImageFlag = true;
                _isGraphFlag = true;

                QualityControlList = GetOrderedList(CellTypeFacade.Instance.GetAllQualityControls_BECall(out var allCells)).ToObservableCollection();
                SelectedQualityControl = QualityControlList.FirstOrDefault();

                Signature = new SignatureViewModel(_reviewModel);
                ShowParameterList = new List<KeyValuePair<string, string>>();
                GraphList = new List<LineGraphDomain>();
                BarGraph = new BarGraphDomain();
                BarGraphViewList = new List<BarGraphDomain>();
                QualityControlListActive = true;
                IsExportVisible = true;
                AssayParameter = assay_parameter.ap_Concentration;
                DynamicAssayHeader = LanguageResourceHelper.Get("LID_DataGridHeader_AssayValue");
                SelectedRightClickImageType = ApplicationConstants.ImageViewRightClickMenuImageFitSize;
                SetAdminAccess();
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_QC_DEFAULT_VALUE"));
            }
        }

        private void LockStatusChanged(LockResult res)
        {
            IsDataEnable = SampleRecordList.Any() && SelectedQualityControl.NotExpired && !_lockManager.IsLocked();
            SwitchToQcDetails();
        }
        #endregion

        #region Properties & Fields

        private bool _isImageFlag;
        private bool _isGraphFlag;
        private ReviewModel _reviewModel;
        private SampleRecordDomain _lastSelectedSampleItem;
        private List<KeyValuePair<int, List<histogrambin_t>>> _histogramList;

        public string HeaderModule => LanguageResourceHelper.Get("LID_HB_QualityControl");

        public SignatureViewModel Signature
        {
            get { return GetProperty<SignatureViewModel>(); }
            set { SetProperty(value); }
        }

        public List<KeyValuePair<string, string>> ShowParameterList
        {
            get { return GetProperty<List<KeyValuePair<string, string>>>(); }
            set { SetProperty(value); }
        }

        public bool IsDeleteEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsSignListAvailable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public List<LineGraphDomain> GraphList
        {
            get { return GetProperty<List<LineGraphDomain>>(); }
            set { SetProperty(value); }
        }

        public bool IsDataEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsAddEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool ImageActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsLineGraphActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool QualityControlListActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<QualityControlDomain> QualityControlList
        {
            get { return GetProperty<ObservableCollection<QualityControlDomain>>(); }
            set { SetProperty(value); }
        }

        public IEnumerable<assay_parameter> AssayParameterList => Enum.GetValues(typeof(assay_parameter)).Cast<assay_parameter>();

        public assay_parameter AssayParameter
        {
            get { return GetProperty<assay_parameter>(); }
            set
            {
                SetProperty(value);
                SetAssayHeaderValue(value);
            }
        }

        private QualityControlDomain _selectedQualityControl;
        public QualityControlDomain SelectedQualityControl
        {
            get { return _selectedQualityControl; }
            set
            {
                _selectedQualityControl = value;
                NotifyPropertyChanged(nameof(SelectedQualityControl));
                if (value?.QcName == null)
                    return;
                GetSampleList(value.QcName.Trim());
            }
        }

        public CellTypeDomain SelectedCellType
        {
            get { return GetProperty<CellTypeDomain>(); }
            set { SetProperty(value); }
        }

        public LineGraphDomain Graph
        {
            get { return GetProperty<LineGraphDomain>(); }
            set { SetProperty(value); }
        }

        public bool IsSignatureEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsSignatureVisible
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsExportVisible
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public BarGraphDomain BarGraph
        {
            get { return GetProperty<BarGraphDomain>(); }
            set { SetProperty(value); }
        }

        public bool GraphActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public List<BarGraphDomain> BarGraphViewList
        {
            get { return GetProperty<List<BarGraphDomain>>(); }
            set { SetProperty(value); }
        }

        private List<SampleRecordDomain> _sampleRecordList;
        public List<SampleRecordDomain> SampleRecordList
        {
            get { return _sampleRecordList ?? (_sampleRecordList = new List<SampleRecordDomain>()); }
            set
            {
                _sampleRecordList = value;
                NotifyPropertyChanged(nameof(SampleRecordList));
            }
        }

        public bool IsBubblePopupOpen
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string DynamicAssayHeader
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public BubbleStatusViewModel BubbleStatusViewModel
        {
            get { return GetProperty<BubbleStatusViewModel>(); }
            set { SetProperty(value); }
        }

        private KeyValuePair<dynamic, double> _selectedValue;
        public KeyValuePair<dynamic, double> SelectedValue
        {
            get { return _selectedValue; }
            set
            {
                _selectedValue = value;
                NotifyPropertyChanged(nameof(SelectedValue));

                if (Graph == null)
                    return;

                var index = Graph.GraphDetailList.IndexOf(value);

                if (index < 0)
                    index = Graph.MultiGraphDetailList.IndexOf(value);
                if (index >= 0)
                    SelectedSampleFromList = SampleRecordList[index];
            }
        }

        #endregion

        #region Commands

        #region Add & Delete QC Commands

        private RelayCommand _addQualityControlCommand;
        public RelayCommand AddQualityControlCommand => _addQualityControlCommand ?? (_addQualityControlCommand = new RelayCommand(AddQualityControl));

        private void AddQualityControl()
        {
            try
            {
                if (SelectedQualityControl == null)
                    return;

                if (SelectedQualityControl.AcceptanceLimit == null)
                {
                    SelectedQualityControl.AcceptanceLimit = (int?)ApplicationConstants.DefaultAcceptanceValue;
                }

                if (QualityControlValidation(SelectedQualityControl, AssayParameter))
                {
                    var msg = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_AddConfirmation"), SelectedQualityControl.QcName);
                    if (DialogEventBus.DialogBoxYesNo(this, msg) != true)
                    {
                        return;
                    }

					string retiredQCName ="";

					var dupQC = QualityControlList.FirstOrDefault(q => q.QcName.Equals(SelectedQualityControl.QcName));
					if (dupQC != null)
					{ // Setup to rename existing QC and create a new QC (Backend will mark existing QC as retired).
						retiredQCName += " (" + Misc.DateFormatConverter(DateTime.Now, "LongDate") + ")";
					}

					CreateQualityControl(SelectedQualityControl);

                    var username = ScoutModels.LoggedInUser.CurrentUserId;
                    var addResult = CellTypeFacade.Instance.AddQc(username, "", SelectedQualityControl, retiredQCName);
                    if (addResult.Equals(HawkeyeError.eSuccess))
                    {
                        GetQualityControlList();
                        PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_QChasBeenAdded"));
                        Log.Debug(LanguageResourceHelper.Get("LID_MSGBOX_QCAddedSuccess"));
                    }
                    else
                        ApiHawkeyeMsgHelper.ErrorCreateQualityControl(addResult);
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex,
                    LanguageResourceHelper.Get("LID_EXCEPTIONMSG_QC_ADD_QUALITY"));
            }
        }

        private bool QualityControlValidation(QualityControlDomain selectedQualityControl, assay_parameter AssayParameter)
        {
            if (string.IsNullOrWhiteSpace(selectedQualityControl.QcName))
            {
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ERRMSGBOX_QCNameBlank"));

                Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_QCNameBlank"));
                return false;
            }
            if (string.IsNullOrWhiteSpace(selectedQualityControl.LotInformation))
            {
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ERRMSGBOX_LotNumberBlank"));

                Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_LotNumberBlank"));
                return false;
            }

            if (string.IsNullOrWhiteSpace(selectedQualityControl.AssayParameter.ToString()))
            {
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ERRMSGBOX_AssayValueBlank"));

                Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_AssayValueBlank"));
                return false;
            }

            if (selectedQualityControl.AssayValue == null)
            {
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ERRMSGBOX_AssayValueBlank"));

                Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_AssayValueBlank"));
                return false;
            }

            switch (AssayParameter)
            {

                case assay_parameter.ap_Size:
                    if (selectedQualityControl.AssayValue < ApplicationConstants.LowerConcentrationAssayLimit || selectedQualityControl.AssayValue > ApplicationConstants.UpperConcentrationAssayLimit)
                    {
                        string message = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_AssayRange"),
                            Misc.UpdateTrailingPoint(ApplicationConstants.LowerConcentrationAssayLimit, TrailingPoint.Two),
                            Misc.UpdateTrailingPoint(ApplicationConstants.UpperConcentrationAssayLimit, TrailingPoint.Two), "µm");

                        DialogEventBus.DialogBoxOk(this, message);

                        Log.Debug(message);
                        return false;
                    }

                    break;
                case assay_parameter.ap_PopulationPercentage:
                    if (selectedQualityControl.AssayValue < ApplicationConstants.LowerViabilityAssayLimit || selectedQualityControl.AssayValue > ApplicationConstants.UpperViabilityAssayLimit)
                    {
                        string message = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_AssayRange"),
                            Misc.UpdateTrailingPoint(ApplicationConstants.LowerViabilityAssayLimit, TrailingPoint.Two),
                            Misc.UpdateTrailingPoint(ApplicationConstants.UpperViabilityAssayLimit, TrailingPoint.Two), "%");

                        DialogEventBus.DialogBoxOk(this, message);

                        Log.Debug(message);
                        return false;
                    }

                    break;
            }

            if (selectedQualityControl.AcceptanceLimit == null)
            {
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ERRMSGBOX_AcceptancelimitsBlank"));

                Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_AcceptancelimitsBlank"));
                return false;
            }

            if (selectedQualityControl.AcceptanceLimit < ApplicationConstants.MinimumQualityControlAcceptanceLimit
                || selectedQualityControl.AcceptanceLimit > ApplicationConstants.MaximumQualityControlAcceptanceLimit)
            {
                string message = string.Format(LanguageResourceHelper.Get("LID_QCHeader_AcceptanceLimitsRange"),
                    Misc.ConvertToString(ApplicationConstants.MinimumQualityControlAcceptanceLimit),
                    Misc.ConvertToString(ApplicationConstants.MaximumQualityControlAcceptanceLimit));

                DialogEventBus.DialogBoxOk(this, message);

                Log.Debug(message);
                return false;
            }

            return true;
        }
        #endregion

        #region Export Commands

        private RelayCommand _exportSelectedCommand;
        public RelayCommand ExportSelectedCommand => _exportSelectedCommand ?? (_exportSelectedCommand = new RelayCommand(ExportSelectedData));

        private void ExportSelectedData()
        {
            try
            {
                if (SelectedSampleFromList == null)
                    return;

                var fileName = SelectedSampleFromList.SampleIdentifier + "_" + Misc.ConvertToFileNameFormat(SelectedSampleFromList.SelectedResultSummary.RetrieveDate);
                var result = HawkeyeCoreAPI.SampleSet.GetSampleDefinitionBySampleIdApiCall(SelectedSampleFromList.UUID, out var samplePosition);
                if (result == HawkeyeError.eSuccess)
                {
                    SelectedSampleFromList.Position = samplePosition;
                }
                var resultRecord = ResultRecordHelper.ExportCompleteRunResult(new List<SampleRecordDomain> { SelectedSampleFromList }).FirstOrDefault();
                ExportModel.PromptAndExportSampleToCsv(resultRecord, SelectedSampleFromList, fileName);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_QC_EXPORT_DATA"));
            }
        }

        private RelayCommand _exportSummaryCommand;
        public RelayCommand ExportSummaryCommand => _exportSummaryCommand ?? (_exportSummaryCommand = new RelayCommand(ExportSummaryDetails));

        private void ExportSummaryDetails()
        {
            try
            {
                foreach (var sample in SampleRecordList)
                {
                    var result = HawkeyeCoreAPI.SampleSet.GetSampleDefinitionBySampleIdApiCall(sample.UUID, out var samplePosition);
                    if (result == HawkeyeError.eSuccess)
                    {
                        sample.Position = samplePosition;
                    }
                }
                ExportModel.PromptAndExportQualityControlToCsv(SelectedQualityControl, SampleRecordList,
                    ResultRecordHelper.ExportCompleteRunResult(SampleRecordList), SelectedQualityControl.QcName);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex,
                    LanguageResourceHelper.Get("LID_EXCEPTIONMSG_QC_EXPORT_DATA"));
            }
        }

        #endregion

        #region Image Commands

        private RelayCommand _imageCommand;
        public RelayCommand ImageCommand => _imageCommand ?? (_imageCommand = new RelayCommand(OnImageActive, null));

        private void OnImageActive()
        {
            try
            {
                if (_isImageFlag)
                {
                    if (_isGraphFlag)
                        SetSampleList();
                    else
                        OnFirstTimeLoadImage();
                    _isImageFlag = false;
                }

                IsSignListAvailable = SelectedSampleFromList.SelectedResultSummary.SignatureList.Any();
                IsLineGraphActive = QualityControlListActive = IsAddEnable = IsDeleteEnable = false;
                ImageActive = IsSignatureVisible = true;
                PlaySlideShowCommand.RaiseCanExecuteChanged();

            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex,
                    LanguageResourceHelper.Get("LID_EXCEPTIONMSG_QC_IMAGE_ACTIVE"));
            }
        }

        private RelayCommand _imageExpandCommand;
        public RelayCommand ImageExpandCommand => _imageExpandCommand ?? (_imageExpandCommand = new RelayCommand(OnExpandImage, null));

        private void OnExpandImage()
        {
            OnExpandView(false);
        }

        #endregion

        #region Graph Commands

        private RelayCommand _graphCommand;
        public RelayCommand GraphCommand => _graphCommand ?? (_graphCommand = new RelayCommand(OnBarGraphActive, null));

        public void OnBarGraphActive()
        {
            try
            {
                if (_isImageFlag && _isGraphFlag)
                    SetSampleList();
                IsSignListAvailable = SelectedSampleFromList.SelectedResultSummary.SignatureList.Any();
                SetBarGraph();
                GraphActive = IsSignatureVisible = true;
                IsLineGraphActive = QualityControlListActive = IsDeleteEnable = IsAddEnable = ImageActive = false;
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex,
                    LanguageResourceHelper.Get("LID_EXCEPTIONMSG_QC_BAR_GRAPHACTIVE"));
            }
        }

        private RelayCommand _graphExpandCommand;
        public RelayCommand GraphExpandCommand => _graphExpandCommand ?? (_graphExpandCommand = new RelayCommand(OnExpandGraph, null));

        private void OnExpandGraph()
        {
            OnExpandView(true);
        }

        #endregion

        #region Signature Command

        private RelayCommand _signatureCommand;
        public RelayCommand SignatureCommand => _signatureCommand ?? (_signatureCommand = new RelayCommand(SignatureSample));

        protected void SignatureSample()
        {
            try
            {
                var args = new AddSignatureEventArgs(ReviewModel.RetrieveSignatureDefinitions(), true);
                if (DialogEventBus.AddSignature(this, args) == true)
                {
                    OnAddSignature(args.SignatureSelected, args.SignaturePassword);
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_QC_ADD_SIGNATURE"));
            }
        }

        #endregion

        #region Double Click Command

        private RelayCommand _doubleClickCommand;
        public RelayCommand DoubleClickCommand => _doubleClickCommand ?? (_doubleClickCommand = new RelayCommand(OnDoubleTap, null));

        private void OnDoubleTap(object param)
        {
            if (IsDataEnable)
                OnLineGraphActive();
        }

        #endregion

        #region Quality Control Command

        private RelayCommand _qualityControlCommand;
        public RelayCommand QualityControlCommand => _qualityControlCommand ?? (_qualityControlCommand = new RelayCommand(OnLineGraphActive, null));

        private async void OnLineGraphActive()
        {
            try
            {
                var result = UpdateLineGraph();
                MessageBus.Default.Publish(new Notification<bool>(true, MessageToken.AdornerVisible));
                await result;
                UpdateHistoricalGraph(SelectedSampleFromList);
                MessageBus.Default.Publish(new Notification<bool>(false, MessageToken.AdornerVisible));
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex,
                    LanguageResourceHelper.Get("LID_EXCEPTIONMSG_QC_LINEGRAPHACTIVE"));
            }
        }

        #endregion

        #region Quality Control List Command

        private RelayCommand _qualityControlListCommand;
        public RelayCommand QualityControlListCommand => _qualityControlListCommand ?? (_qualityControlListCommand = new RelayCommand(SwitchToQcDetails));

        private void SwitchToQcDetails()
        {
            try
            {
                IsSignatureVisible = IsSignListAvailable = false;
                ImageActive = IsLineGraphActive = GraphActive = false;
                QualityControlListActive = true;
                SetAdminAccess();
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_QC_DETAILS"));
            }
        }

        #endregion

        #region Close Dialog Command

        private RelayCommand _closeDialogCommand;
        public RelayCommand CloseDialogCommand => _closeDialogCommand ?? (_closeDialogCommand = new RelayCommand(PerformCloseDialog));

        private void PerformCloseDialog(object param)
        {
            if (QualityControlList.Any())
            {
                SelectedQualityControl = QualityControlList.FirstOrDefault();
            }
        }

        #endregion

        #region Open Quality Control Window

        private RelayCommand _openQualityControlWindow;
        public RelayCommand OpenQualityControlWindow => _openQualityControlWindow ?? (_openQualityControlWindow = new RelayCommand(OpenQualityControlDialog));

        private void OpenQualityControlDialog()
        {
            OnQualityControlCreate();
            var args = new AddCellTypeEventArgs(QualityControlList.Count);
            var result = DialogEventBus.AddCellTypeDialog(this, args);
            if (result == true)
            {
                GetSampleList(args.NewQualityControlName);
                GetQualityControlList();
                SelectedQualityControl = QualityControlList.FirstOrDefault(q => q.QcName.Equals(args.NewQualityControlName)) ?? QualityControlList.First();
            }
        }

        #endregion

        #region Bubble Status Command

        private RelayCommand _bubbleStatusCommand;
        public RelayCommand BubbleStatusCommand => _bubbleStatusCommand ?? (_bubbleStatusCommand = new RelayCommand(OnBubbleStatusUpdate));

        private void OnBubbleStatusUpdate()
        {
            if (IsBubblePopupOpen)
            {
                IsBubblePopupOpen = false;
            }
            else
            {
                IsBubblePopupOpen = true;

                if (BubbleStatusViewModel == null)
                {
                    BubbleStatusViewModel = new BubbleStatusViewModel();
                }

                BubbleStatusViewModel.UpdateBubbleStatus(SelectedSampleFromList, false);
            }
        }

        #endregion

        #endregion

        #region Private Methods

        private void SetQualityControlList(string selectedQcName)
        {
            List<CellTypeDomain> _cellTypes = new List<CellTypeDomain>();
            List<QualityControlDomain> qcList = new List<QualityControlDomain>();
            List<CellTypeQualityControlGroupDomain> _ctqcgroup = new List<CellTypeQualityControlGroupDomain>();

            CellTypeFacade.Instance.GetAllowedCtQc(LoggedInUser.CurrentUserId, ref _cellTypes, ref qcList, ref _ctqcgroup);
            if (qcList.Any())
            {
                var updatedList = new ObservableCollection<QualityControlDomain>(GetOrderedList(qcList));                
                QualityControlList = updatedList;
                if (!String.IsNullOrEmpty(selectedQcName))
                {
                    var selectedQC = updatedList.FirstOrDefault(q => q.QcName.Equals(selectedQcName)) ?? updatedList.First();
                    SelectedQualityControl = selectedQC;
                }
            }
            else
                QualityControlList = new ObservableCollection<QualityControlDomain>();
        }

        private void OnQualityControlsUpdated(Notification msg)
        {
            if (string.IsNullOrEmpty(msg?.Token))
                return;

            if (msg.Token.Equals(MessageToken.CellTypesUpdated) || msg.Token.Equals(MessageToken.QualityControlsUpdated))
            {
                var name = SelectedQualityControl?.QcName;
                SetQualityControlList(name);
            }
        }

        private void SetBarGraph()
        {
            if (_lastSelectedSampleItem?.UUID.ToString() == SelectedSampleFromList?.UUID.ToString() && BarGraphViewList.Any())
                return;

            _lastSelectedSampleItem = SelectedSampleFromList;
            var graphHelper = new GraphHelper();
            _histogramList = RecordHelper.GetHistogramList(SelectedSampleFromList.SelectedResultSummary.UUID);
            BarGraphViewList = graphHelper.CreateGraph(SelectedSampleFromList, _histogramList, true);

            if (BarGraphViewList != null && BarGraphViewList.Any())
                BarGraph = BarGraphViewList.First();
        }

        private void GetSampleList(string qcName)
        {
            try
            {
                _isImageFlag = true;
                _isGraphFlag = true;
                SampleRecordList = QualityControlModel.RetrieveSampleRecordsForQualityControl(qcName);
                IsDataEnable = SampleRecordList.Any() && SelectedQualityControl.NotExpired && !_lockManager.IsLocked();
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_QC_SAMPLE_LIST"));
            }
        }

        private void OnExpandView(bool isGraph)
        {
            try
            {
                if (SelectedSampleFromList == null) return;
                SlideShowIsRunning = false;
                if (isGraph)
                {
                    var barGraphList = BarGraphViewList?.Cast<object>()?.ToList();
                    var index = BarGraphViewList?.IndexOf(BarGraph) ?? -1;
                    var args = new ExpandedImageGraphEventArgs(ExpandedContentType.BarChart, SelectedRightClickImageType, barGraphList, BarGraph, index);
                    DialogEventBus.ExpandedImageGraph(this, args);
                    BarGraph = BarGraphViewList?.ElementAt(args.SelectedGraphIndex);
                }
                else
                {
                    var imageList = SelectedSampleFromList?.SampleImageList?.Cast<object>()?.ToList();
                    var index = SelectedSampleFromList?.SelectedSampleImageRecord == null ? -1 : (int)SelectedSampleFromList.SelectedSampleImageRecord.SequenceNumber;
                    var args = new ExpandedImageGraphEventArgs(SelectedImageType, SelectedRightClickImageType, imageList, SelectedSampleFromList, index);
                    DialogEventBus.ExpandedImageGraph(this, args);
                    var newIndex = args.SelectedImageIndex;
                    var imgList = SelectedSampleFromList.ImageIndexList.ToList();
                    SelectedImageIndex = imgList[newIndex];
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_QC_EXPAND_VIEW"));
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

        public override void OnSelectedSampleRecord(SampleRecordDomain sample)
        {
            try
            {
                IsSignListAvailable = SelectedSampleFromList.SelectedResultSummary.SignatureList.Any();
                if (ImageActive &&
                    sample.SelectedSampleImageRecord != null &&
                    sample.SelectedSampleImageRecord.ImageSet.BrightfieldImage == null &&
                    sample.SampleImageList.Any())
                {
                    OnFirstTimeLoadImage();
                }
              
                if (GraphActive)
                {
                    SetBarGraph();
                }
                //We need to call SetImage even if we're not on the Image tab. If we don't and we switch
                //samples while on another tab then go to the Image tab, we'll have the wrong image.
                SetImage(SelectedSampleFromList.SampleImageList.IndexOf(SelectedSampleFromList.SelectedSampleImageRecord));
                UpdateHistoricalGraph(sample);
                ShowParameterList = new List<KeyValuePair<string, string>>(RecordHelper.SetListParameter(sample, LoggedInUser.CurrentUserId));
                SelectedSampleFromList.UpdateSampleBubbleStatus();
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_QC_SELECT_SAMPLE_RECORD"));
            }
        }

        private void SetAssayHeaderValue(assay_parameter assayParameter)
        {
            switch (assayParameter)
            {
                case assay_parameter.ap_Concentration:
                    DynamicAssayHeader = LanguageResourceHelper.Get("LID_GraphLabel_AssayConcentrationPerml");
                    break;
                case assay_parameter.ap_PopulationPercentage:
                    DynamicAssayHeader = LanguageResourceHelper.Get("LID_GraphLabel_AssayConcentrationPerUnit");
                    break;
                case assay_parameter.ap_Size:
                    DynamicAssayHeader = LanguageResourceHelper.Get("LID_GraphLabel_AssayConcentrationMicrometerUnit");
                    break;
            }
        }

        private void UpdateHistoricalGraph(SampleRecordDomain sample)
        {
            var index = SampleRecordList.FindIndex(x => x == sample);
            if (Graph != null)
                SelectedValue = Graph.GraphDetailList[index];
        }

        private void OnFirstTimeLoadImage()
        {
            SelectedImageType = ImageType.Raw;
            SelectedImageIndex = new KeyValuePair<int, string>(0, Misc.ConvertToString(0));
            SelectedImageIndex = new KeyValuePair<int, string>(ApplicationConstants.IndexPoint, Misc.ConvertToString(ApplicationConstants.IndexPoint));
        }

        private void SetAdminAccess()
        {
            switch (LoggedInUser.CurrentUserRoleId)
            {
                case UserPermissionLevel.eNormal:
                    IsAddEnable = IsDeleteEnable = IsExportVisible = false;
                    break;
                case UserPermissionLevel.eElevated:
                    IsDeleteEnable = false;
                    IsAddEnable = true;
                    break;
                case UserPermissionLevel.eAdministrator:
                    IsDeleteEnable = QualityControlList.Any();
                    IsAddEnable = true;
                    break;
                case UserPermissionLevel.eService:
                    IsAddEnable = IsDeleteEnable = false;
                    break;
            }
        }

        private void OnAddSignature(ISignature selectedSign, string password)
        {
            var validateStatus = UserModel.ValidateMe(password);
            if (validateStatus.Equals(HawkeyeError.eSuccess))
            {
                var signStatus = ReviewModel.SignResultRecord(SelectedSampleFromList.SelectedResultSummary.UUID,
                    selectedSign.SignatureIndicator, (ushort)selectedSign.SignatureIndicator.Length);

                if (signStatus.Equals(HawkeyeError.eSuccess))
                {
                    var resultSummary = ReviewModel.RetrieveResultSummary(SelectedSampleFromList.SelectedResultSummary.UUID);
                    SelectedSampleFromList.SelectedResultSummary.SignatureList = resultSummary.SignatureList;
                    SelectedSampleFromList.SelectedResultSummary.SelectedSignature = resultSummary.SignatureList.Last();
                    IsSignListAvailable = SelectedSampleFromList.SelectedResultSummary.SignatureList.Any();
                    PostToMessageHub(
                        LanguageResourceHelper.Get("LID_StatusBar_ResultRecordSigned"));
                    Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_SignMsg"));
                }
                else
                {
                    DisplayErrorDialogByApi(signStatus);
                }
            }
            else
            {
                DisplayErrorDialogByApi(validateStatus);
            }
        }

        private void GetQualityControlList()
        {
            var qcList = CellTypeFacade.Instance.GetAllQualityControls_BECall(out var allCells);
            QualityControlList = new ObservableCollection<QualityControlDomain>();
            if (qcList.Any())
            {
                QualityControlList = new ObservableCollection<QualityControlDomain>(GetOrderedList(qcList));
                SelectedQualityControl = QualityControlList.FirstOrDefault();
            }
            SetAdminAccess();
        }

        private List<QualityControlDomain> GetOrderedList(List<QualityControlDomain> qcList)
        {
            if (qcList == null) return new List<QualityControlDomain>();

            return qcList.OrderByDescending(x => x.ExpirationDate).ToList();
        }

        private Task<bool> UpdateLineGraph()
        {
            return Task.Factory.StartNew(() =>
            {
                if (_isImageFlag && _isGraphFlag)
                    SetSampleList();

                SetLineGraph();
                IsSignListAvailable = SelectedSampleFromList.SelectedResultSummary.SignatureList.Any();
                IsLineGraphActive = IsSignatureVisible = true;
                QualityControlListActive = GraphActive = ImageActive = IsAddEnable = IsDeleteEnable = false;
            }).ContinueWith(x => true);
        }

        private void SetUpperLimit(ref double lowerLimit, ref double upperLimit)
        {
            if (SelectedQualityControl?.AssayValue == null || SelectedQualityControl?.AcceptanceLimit == null)
                return;

            var assayValue = SelectedQualityControl.AssayValue;

            lowerLimit = (double)(assayValue.Value - (assayValue.Value * (SelectedQualityControl.AcceptanceLimit * 0.01)));
            upperLimit = (double)(assayValue.Value + (assayValue.Value * (SelectedQualityControl.AcceptanceLimit * 0.01)));
        }

        private void OnQualityControlCreate()
        {
            try
            {
                var cellTypes = CellTypeFacade.Instance.GetAllCellTypes_BECall();
                if (cellTypes.Any())
                    SelectedCellType = cellTypes.First();
                AssayParameter = assay_parameter.ap_Concentration;
                SelectedQualityControl = new QualityControlDomain();
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex,
                    LanguageResourceHelper.Get("LID_EXCEPTIONMSG_QC_CREATE_QUALITY_CONTROL"));
            }
        }

        private void CreateQualityControl(QualityControlDomain selectedQc)
        {
            Log.Debug("CreateQualityControl::");
            selectedQc.CellTypeIndex = SelectedCellType.CellTypeIndex;
            selectedQc.CellTypeName = SelectedCellType.CellTypeName;
            selectedQc.AcceptanceLimit = SelectedQualityControl.AcceptanceLimit;
            selectedQc.AssayValue = SelectedQualityControl.AssayValue;
            selectedQc.AssayParameter = AssayParameter;
        }

        private void SetLineGraph()
        {
            double lowerLimit = 0;
            double upperLimit = 0;
            var graphHelper = new GraphHelper();
            GraphList = graphHelper.CreateLineGraphList(3);
            SetUpperLimit(ref lowerLimit, ref upperLimit);
            GraphList = graphHelper.GetLineGraphListForQC(GraphList, SampleRecordList, lowerLimit, upperLimit);
            if (GraphList == null || !GraphList.Any())
                return;
            Graph = new LineGraphDomain();
            SelectedValue = new KeyValuePair<dynamic, double>();
            switch (SelectedQualityControl.AssayParameter)
            {
                case assay_parameter.ap_Concentration:
                    Graph = GraphList[0];
                    break;
                case assay_parameter.ap_PopulationPercentage:
                    Graph = GraphList[1];
                    break;
                case assay_parameter.ap_Size:
                    Graph = GraphList[2];
                    break;
            }
        }

        private void SetSampleList()
        {
            foreach (var sample in SampleRecordList)
            {
                RecordHelper.OnSelectedSampleRecord(sample);
            }

            if (SampleRecordList.Any())
            {
                SelectedSampleFromList = SampleRecordList[0];
            }

            _isGraphFlag = false;
        }

        #endregion Private Methods
    }
}