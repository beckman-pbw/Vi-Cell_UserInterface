using HawkeyeCoreAPI.Facade;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.Common;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutDomains.RunResult;
using ScoutLanguageResources;
using ScoutModels.Common;
using ScoutModels.Review;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace ScoutModels.Home.QueueManagement
{
    public class QueueResultModel : NotifyPropertyChangesDisposable
    {       
        private bool _isQueueCreationCarouselGridScrollVisible;
        public bool IsQueueCreationCarouselGridScrollVisible
        {
            get { return _isQueueCreationCarouselGridScrollVisible; }
            set
            {
                _isQueueCreationCarouselGridScrollVisible = value;
                NotifyPropertyChanged(nameof(IsQueueCreationCarouselGridScrollVisible));
            }
        }

        private bool _isQueueCreationWellPlateGridScrollVisible;
        public bool IsQueueCreationWellPlateGridScrollVisible
        {
            get { return _isQueueCreationWellPlateGridScrollVisible; }
            set
            {
                _isQueueCreationWellPlateGridScrollVisible = value;
                NotifyPropertyChanged(nameof(IsQueueCreationWellPlateGridScrollVisible));
            }
        }

        private bool _isRunResultCarouselGridScrollVisible;
        public bool IsRunResultCarouselGridScrollVisible
        {
            get { return _isRunResultCarouselGridScrollVisible; }
            set
            {
                _isRunResultCarouselGridScrollVisible = value;
                NotifyPropertyChanged(nameof(IsRunResultCarouselGridScrollVisible));
            }
        }

        private bool _isRunResultWellPlateGridScrollVisible;

        public bool IsRunResultWellPlateGridScrollVisible
        {
            get { return _isRunResultWellPlateGridScrollVisible; }
            set
            {
                _isRunResultWellPlateGridScrollVisible = value;
                NotifyPropertyChanged(nameof(IsRunResultWellPlateGridScrollVisible));
            }
        }

        public ResultRecordHelper RecordHelper { get; set; }

        public UpdateGraph UpdateGraph { get; set; }

        public QueueResultModel()
        {
            RecordHelper = new ResultRecordHelper();
            IsEnableAdvanceOptionRunQueue = IsPlateOrderedByRow = true;
        }

        protected override void DisposeUnmanaged()
        {
            RecordHelper?.Dispose();
            base.DisposeUnmanaged();
        }

        private List<SampleDomain> _plateSamplesByColumn;
        public List<SampleDomain> PlateSamplesByColumn
        {
            get { return _plateSamplesByColumn; }
            set
            {
                _plateSamplesByColumn = value;
                NotifyPropertyChanged(nameof(PlateSamplesByColumn));
            }
        }

        private List<SampleDomain> _plateSamplesByRow;
        public List<SampleDomain> PlateSamplesByRow
        {
            get { return _plateSamplesByRow; }
            set
            {
                _plateSamplesByRow = value;
                NotifyPropertyChanged(nameof(PlateSamplesByRow));
            }
        }

        private bool _isRowWisePosition = true;
        public bool IsRowWisePosition
        {
            get { return _isRowWisePosition; }
            set
            {
                _isRowWisePosition = value;
                NotifyPropertyChanged(nameof(IsRowWisePosition));
                if (IsRowWisePosition)
                {
                    PlateSamplesByRow = GetRowWiseSortedPlateCollection(PlateSamples);
                }
                else
                {
                    PlateSamplesByColumn = GetColumnWiseSortedPlateCollection(PlateSamples);
                }
            }
        }

        #region Play Pause Public Property

        private List<SampleDomain> _carouselSamples;
        public List<SampleDomain> CarouselSamples
        {
            get { return _carouselSamples ?? (_carouselSamples = QueueManagementModel.CreateEmptyCarousel()); }
            set
            {
                _carouselSamples = value;
                NotifyPropertyChanged(nameof(CarouselSamples));
            }
        }

        private List<SampleDomain> _plateSamples;
        public List<SampleDomain> PlateSamples
        {
            get { return _plateSamples ?? (_plateSamples = QueueManagementModel.CreateEmptyPlate()); }
            set
            {
                _plateSamples = value;
                NotifyPropertyChanged(nameof(PlateSamples));
            }
        }

        #region Private Play Pause Active Enable

        private int _playItemPosition;
        private bool _isEnablePlayPause;
        private bool _isClearingCarousel;

        private bool _isSelectionEnable;
        private bool _isQueueRunning;
        private bool _isUsingCarousel;
        private bool _isStopActive;
        private bool _isPauseActive;
        private bool _isPlateOrderedByRow;
        private bool _isAbortSampleActive;
        private string _playPauseStatusContent;
        private string _abortStatusContent;

        #endregion

        #region Play Pause Active Enable

        public bool IsAbortSampleActive
        {
            get { return _isAbortSampleActive; }
            set
            {
                _isAbortSampleActive = value;
                NotifyPropertyChanged(nameof(IsAbortSampleActive));
            }
        }

        public string AbortStatusContent
        {
            get { return _abortStatusContent; }
            set
            {
                _abortStatusContent = value;
                NotifyPropertyChanged(nameof(AbortStatusContent));
            }
        }

        public bool IsPlateOrderedByRow
        {
            get { return _isPlateOrderedByRow; }
            set
            {
                _isPlateOrderedByRow = value;
                NotifyPropertyChanged(nameof(IsPlateOrderedByRow));
                UpdatePlateSamplesList();
            }
        }

        private void UpdatePlateSamplesList()
        {
            if (IsPlateOrderedByRow)
            {
                PlateSamplesByRow = GetRowWiseSortedPlateCollection(PlateSamples);
            }
            else
            {
                PlateSamplesByColumn = GetColumnWiseSortedPlateCollection(PlateSamples); 
            }
        }

        private List<SampleDomain> GetRowWiseSortedPlateCollection(List<SampleDomain> collection)
        {
            var i = 0;
            var items = new List<SampleDomain>(collection);
            items = collection.OrderBy(x => x.Row).Select(x =>
                {
                    x.RowIndex = i++;
                    return x;
                })
            .ToList();

            return items;
        }

        private List<SampleDomain> GetColumnWiseSortedPlateCollection(List<SampleDomain> collection)
        {
            var i = 0;
            var items = new List<SampleDomain>(collection);
            items = collection.OrderBy(x => x.Column).Select(x =>
                {
                    x.ColumnIndex = i++;
                    return x;
                })
            .ToList();

            return items;
        }

        public void SortCarouselWellPlateCollectionsWhileRunning()
        {
            var samples = IsUsingCarousel
                ? CarouselSamples
                : IsPlateOrderedByRow
                    ? PlateSamplesByRow
                    : PlateSamplesByColumn;

            SortCarouselWellPlateCollectionsWhileRunning(IsUsingCarousel, IsPlateOrderedByRow, samples);
        }

        public static void SortCarouselWellPlateCollectionsWhileRunning(bool isUsingCarousel, bool isPlateOrderByRow, List<SampleDomain> samples)
        {
            if (isUsingCarousel)
            {
                DispatcherHelper.ApplicationExecute(() =>
                {
                    samples = samples.OrderBy(x => x.Tag).Select(x => x).OrderBy(x => x.IsEnabled ? 0 : 1).ToList();
                });
            }
            else
            {
                if (isPlateOrderByRow)
                {
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        samples = samples.OrderBy(x => x.SamplePosition.Row).Select(x => x).OrderBy(x => x.IsEnabled ? 0 : 1).ToList();
                    });
                }
                else
                {
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        samples = samples.OrderBy(x => x.SamplePosition.Column).Select(x => x).OrderBy(x => x.IsEnabled ? 0 : 1).ToList();
                    });
                }
            }
        }

        public bool IsPauseActive
        {
            get { return _isPauseActive; }
            set
            {
                _isPauseActive = value;
                NotifyPropertyChanged(nameof(IsPauseActive));
            }
        }

        public bool IsStopActive
        {
            get { return _isStopActive; }
            set
            {
                _isStopActive = value;
                NotifyPropertyChanged(nameof(IsStopActive));
            }
        }

        public bool IsUsingCarousel
        {
            get { return _isUsingCarousel; }
            set
            {
                _isUsingCarousel = value;
                NotifyPropertyChanged(nameof(IsUsingCarousel));
            }
        }

        public bool IsQueueRunning
        {
            get { return _isQueueRunning; }
            set
            {
                _isQueueRunning = value;
                NotifyPropertyChanged(nameof(IsQueueRunning));
            }
        }

        public bool IsSelectionEnable
        {
            get { return _isSelectionEnable; }
            set
            {
                _isSelectionEnable = value;
                NotifyPropertyChanged(nameof(IsSelectionEnable));
            }
        }

        public int PlayItemPosition
        {
            get { return _playItemPosition; }
            set { SetFieldAndNotify(ref _playItemPosition, value); }
        }

        public bool IsClearingCarousel
        {
            get { return _isClearingCarousel; }
            set { SetFieldAndNotify(ref _isClearingCarousel, value); }
        }

        public bool IsEnablePlayPause
        {
            get { return _isEnablePlayPause; }
            set { SetFieldAndNotify(ref _isEnablePlayPause, value); }
        }

        public string PlayPauseStatusContent
        {
            get { return _playPauseStatusContent; }
            set
            {
                _playPauseStatusContent = value;
                NotifyPropertyChanged(nameof(PlayPauseStatusContent));
            }
        }

        private bool _isEnableAdvanceOptionRunQueue;
        public bool IsEnableAdvanceOptionRunQueue
        {
            get { return _isEnableAdvanceOptionRunQueue; }
            set
            {
                _isEnableAdvanceOptionRunQueue = value;
                NotifyPropertyChanged(nameof(IsEnableAdvanceOptionRunQueue));
            }
        }


        #endregion

        #region Private ProgressBar Status

        private SampleProgressStatus _mixingDyeBrush;
        private SampleProgressStatus _aspirationBrush;
        private SampleProgressStatus _imageAnalysisBrush;
        private SampleProgressStatus _cleaningBrush;
        private bool _isLoadingIndicatorActive;
        private string _runSampleStatus;

        #endregion

        #region public ProgressBar Status

        public bool IsLoadingIndicatorActive
        {
            get { return _isLoadingIndicatorActive; }
            set
            {
                _isLoadingIndicatorActive = value;
                NotifyPropertyChanged(nameof(IsLoadingIndicatorActive));
            }
        }

        public SampleProgressStatus AspirationBrush
        {
            get { return _aspirationBrush; }
            set
            {
                _aspirationBrush = value;
                NotifyPropertyChanged(nameof(AspirationBrush));
            }
        }

        public SampleProgressStatus MixingDyeBrush
        {
            get { return _mixingDyeBrush; }
            set
            {
                _mixingDyeBrush = value;
                NotifyPropertyChanged(nameof(MixingDyeBrush));
            }
        }

        public SampleProgressStatus ImageAnalysisBrush
        {
            get { return _imageAnalysisBrush; }
            set
            {
                _imageAnalysisBrush = value;
                NotifyPropertyChanged(nameof(ImageAnalysisBrush));
            }
        }

        public SampleProgressStatus CleaningBrush
        {
            get { return _cleaningBrush; }
            set
            {
                _cleaningBrush = value;
                NotifyPropertyChanged(nameof(CleaningBrush));
            }
        }

        public string RunSampleStatus
        {
            get { return _runSampleStatus; }
            set
            {
                _runSampleStatus = value;
                NotifyPropertyChanged(nameof(RunSampleStatus));
            }
        }

        #endregion


        private double _gridHorizontalOffset;

        public double GridHorizontalOffset
        {
            get { return _gridHorizontalOffset; }
            set
            {
                _gridHorizontalOffset = value;
                NotifyPropertyChanged(nameof(GridHorizontalOffset));
            }
        }

        private double _horizontalOffset;

        public double HorizontalOffset
        {
            get { return _horizontalOffset; }
            set
            {
                _horizontalOffset = value;
                NotifyPropertyChanged(nameof(HorizontalOffset));
            }
        }


        private bool _canContentScroll;

        public bool CanContentScroll
        {
            get { return _canContentScroll; }
            set
            {
                _canContentScroll = value;
                NotifyPropertyChanged(nameof(CanContentScroll));
            }
        }


        private WorkQueueRecordDomain _workQueueResultRecord;

        public WorkQueueRecordDomain WorkQueueResultRecord
        {
            get { return _workQueueResultRecord; }
            set
            {
                _workQueueResultRecord = value;
                NotifyPropertyChanged(nameof(WorkQueueResultRecord));
            }
        }


        private SampleRecordDomain _selectedSampleRecord;

        public SampleRecordDomain SelectedSampleRecord
        {
            get { return _selectedSampleRecord ?? (_selectedSampleRecord = new SampleRecordDomain()); }
            set
            {
                _selectedSampleRecord = value;
                NotifyPropertyChanged(nameof(SelectedSampleRecord));
                if (_selectedSampleRecord != null)
                {
                    UpdateGraph?.Invoke();
                }
            }
        }

        private List<KeyValuePair<string, string>> _showParameterList;

        public List<KeyValuePair<string, string>> ShowParameterList
        {
            get { return _showParameterList ?? (_showParameterList = new List<KeyValuePair<string, string>>()); }
            set
            {
                _showParameterList = value;
                NotifyPropertyChanged(nameof(ShowParameterList));
            }
        }

        private List<CellTypeQualityControlGroupDomain> _bpQcGroupList;

        public List<CellTypeQualityControlGroupDomain> BpQcGroupList
        {
            get { return _bpQcGroupList; }
            set
            {
                _bpQcGroupList = value;
                NotifyPropertyChanged(nameof(BpQcGroupList));
            }
        }

        public List<CellTypeDomain> AllCellTypesList { get; set; }

        #endregion

        public static void SetResultSampleRecord(WorkQueueItemDto wqi, BasicResultAnswers cumulativeResults, 
            BasicResultAnswers imageResult, ImageSetDto imageSet, int imageSequence, WorkQueueRecordDomain workQueueRecordDomain,
            SampleRecordDomain sampleRecordDomain)
        {
            var resultSummary = new ResultSummaryDomain();
            var allCellTypes = LoggedInUser.CurrentUser.AssignedCellTypes;
            resultSummary.CellTypeDomain = allCellTypes.FirstOrDefault(c => c.CellTypeIndex == wqi.CelltypeIndex);
            var newCumulative = cumulativeResults.MarshalToBasicResultDomain();

            // The sample record must have been initialized during sample setup.
            var sampleRec = workQueueRecordDomain.SampleRecords.LastOrDefault(x => x.SampleIdentifier.Equals(wqi.Label));
            if(sampleRec == null)
            {
                // "SampleRecords" will be cleared <see ref QueueCreationViewModel::ClearQueueCreationViewModel>
                // If work queue is completed and user interface is still accessing the result for "RunResultScreen".
                return;
            }

            if (newCumulative.TotalCumulativeImage > sampleRec.NumImageSets || sampleRec.SelectedResultSummary == null)
            {
                resultSummary.CumulativeResult = newCumulative;
                sampleRec.NumImageSets = resultSummary.CumulativeResult.TotalCumulativeImage;
            }
            else
            {
                // Use the most-recent cumulative result
                resultSummary.CumulativeResult = sampleRec.SelectedResultSummary.CumulativeResult;
            }

            sampleRec.ResultSummaryList.Add(resultSummary);
            sampleRec.SelectedResultSummary = resultSummary;

            var sampleImageResult = new SampleImageRecordDomain
            {
	            ImageSet = imageSet,
	            SequenceNumber = (uint)imageSequence,
                ImageID = wqi.Location.Column,
                ResultPerImage = imageResult.MarshalToBasicResultDomain()
            };

            if (sampleRecordDomain.SampleIdentifier.Equals(wqi.Label))
            {
                sampleRecordDomain.SelectedSampleImageRecord = sampleImageResult;
            }

            sampleRec.SampleImageList.Add(sampleImageResult);
            sampleRec.ImageIndexList.Add(new KeyValuePair<int, string>(imageSequence, Misc.ConvertToString(imageSequence)));

            var imageSuccessCount = sampleRec.SampleImageList.Count(x =>
                x.ResultPerImage.ProcessedStatus.Equals(E_ERRORCODE.eSuccess));
            sampleRec.NumImageSets = (uint)imageSuccessCount;
            sampleRec.UpdateSampleBubbleStatus();

            foreach (var item in sampleRec.SampleImageList)
            {
                item.TotalCumulativeImage = sampleRec.SampleImageList.Count;
            }
        }

        public void onWQComplete()
        {
            CanContentScroll = true;
            ClearSampleProgressStatus();
            SetDefaultStateOfPlayPauseStop();
            IsEnableAdvanceOptionRunQueue = true;
        }

        public void onWQIComplete(WorkQueueItemDto wqi, uuidDLL uuid)
        {
            IsSelectionEnable = false;
            Log.Debug("onWQIComplete:: Time-Stamp: " + ScoutUtilities.Misc.ConvertToString(DateTime.Now));
           
            DispatcherHelper.ApplicationExecute(() => 
            { 
                ClearSampleProgressStatus(); 
            });
        }

        public void onWQIStatus(SampleEswDomain sample, uuidDLL uuid)
        {
            IsSelectionEnable = true;

            if (!sample.SamplePosition.IsValid())
                return;

            SampleDomain runningSample = GetCurrentRunningSample(sample);
            if (runningSample == null)
                return;

            switch (sample.SampleStatus)
            {
                case SampleStatus.InProcessAspirating:
                    GetSampleCarouselFromWorkQueueItem(sample, runningSample);
                    UpdateSampleProgressStatus(sample.SampleStatus, runningSample);
                    AddSampleRecord(sample, uuid);
                    runningSample.ExecutionStatus = ExecutionStatus.Running;
                    SortCarouselWellPlateCollectionsWhileRunning();
                    runningSample.Color = Brushes.Yellow;
                    break;

                case SampleStatus.InProcessMixing:
                case SampleStatus.InProcessImageAcquisition:
                case SampleStatus.InProcessCleaning:
                case SampleStatus.AcquisitionComplete:
                case SampleStatus.NotProcessed:
                    UpdateSampleProgressStatus(sample.SampleStatus, runningSample);
                    break;

                case SampleStatus.Completed:
                    UpdateSampleProgressStatus(sample.SampleStatus, runningSample);
                    SetCarouselDataAfterCompleted(sample);
                    break;

                case SampleStatus.SkipError:
                    runningSample.ExecutionStatus = ExecutionStatus.Error;
                    LogDetailsAndClearSkippedSample(runningSample);
                    break;

                case SampleStatus.SkipManual:
                    // Don't log an error for a missing sample 
                    runningSample.Clear();
                    runningSample.SampleStatusColor = SampleStatusColor.Skip;
                    break;
            }

            LogWorkQueueItem(sample);
            CanContentScroll = false;
        }

        public static void LogDetailsAndClearSkippedSample(SampleDomain runningSample)
        {
           var message = string.Format(
               LanguageResourceHelper.Get("LID_ClearSkippedSample"),
               runningSample.SampleID, 
               runningSample.SelectedCellTypeQualityControlGroup?.Name,
               runningSample.SampleRowPosition);

            Log.Warn(message);
            MessageBus.Default.Publish(new SystemMessageDomain 
            { 
                IsMessageActive = true, 
                Message = message, 
                MessageType = MessageType.Warning 
            });

            runningSample.Clear();
            runningSample.SampleStatusColor = SampleStatusColor.Skip;
        }

        public void GetSampleCarouselFromWorkQueueItem(SampleEswDomain wqi, SampleDomain sample)
        {
            sample.SamplePosition = wqi.SamplePosition;
            sample.SampleID = wqi.SampleName;
            sample.SelectedDilution = Misc.ConvertToString(wqi.Dilution);
            sample.SelectedWash = wqi.WashType;
            var temp = QueueManagementModel.GetCellTypeBpQC(BpQcGroupList.ToList(), wqi);
            sample.SelectedCellTypeQualityControlGroup = temp;
            sample.Comment = wqi.SampleTag;
            sample.IsEnabled = true;
            sample.SampleStatusColor = SampleStatusColor.Selected;
        }

        public void AddSampleRecord(SampleEswDomain wqi, uuidDLL uuid)
        {
            SelectedSampleRecord = AddSampleRecord(wqi, uuid, WorkQueueResultRecord, BpQcGroupList);
        }

        public static SampleRecordDomain AddSampleRecord(SampleEswDomain wqi, uuidDLL uuid, 
            WorkQueueRecordDomain workQueueResultRecord, List<CellTypeQualityControlGroupDomain> bpQcGroupList)
        {
            var bpQcName = string.IsNullOrEmpty(wqi.CellTypeQcName)
                ? bpQcGroupList.FirstOrDefault()?.CellTypeQualityControlChildItems?.FirstOrDefault(x =>
                      x != null && x.CellTypeIndex == wqi.CellTypeIndex)?.Name ?? string.Empty
                : wqi.CellTypeQcName;

            var currentSampleRecord = new SampleRecordDomain
            {
                BpQcName = bpQcName,
                ShowParameterList = ResultRecordHelper.GetShowParameterList(new GenericDataDomain(), LoggedInUser.CurrentUserId),
                Tag = wqi.SampleTag,
                UUID = uuid,
                SampleIdentifier = wqi.SampleName,
                WashName = wqi.WashType,
                DilutionName = Misc.ConvertToString(wqi.Dilution),
                Position = wqi.SamplePosition,
                SelectedResultSummary = null,
                SelectedSampleImageRecord = null,
                IsSampleCompleted = false
            };

            if (workQueueResultRecord == null) workQueueResultRecord = new WorkQueueRecordDomain();
            workQueueResultRecord.SampleRecords.Add(currentSampleRecord);
            
            return currentSampleRecord;
        }

        private void UpdateSampleProgressStatus(SampleStatus status, SampleDomain item)
        {
            string runSampleStatus = string.Empty;
            IsLoadingIndicatorActive = false;
            switch (status)
            {
                case SampleStatus.NotProcessed:
                    runSampleStatus = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(RunSampleProgressIndicator.eNotProcessed));
                    SetSampleProgressStatus(SampleProgressStatus.IsReady, SampleProgressStatus.IsReady, SampleProgressStatus.IsReady, SampleProgressStatus.IsReady, runSampleStatus);
                    break;
					
                case SampleStatus.InProcessAspirating:
                    IsLoadingIndicatorActive = true;
                    runSampleStatus = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(RunSampleProgressIndicator.eInProcess_Aspirating));
                    SetSampleProgressStatus(SampleProgressStatus.IsRunning, SampleProgressStatus.IsInActive, SampleProgressStatus.IsInActive, SampleProgressStatus.IsInActive, runSampleStatus);
                    item.RunSampleProgress = RunSampleProgressIndicator.eInProcess_Aspirating;
                    break;
					
                case SampleStatus.InProcessMixing:
                    IsLoadingIndicatorActive = true;
                    runSampleStatus = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(RunSampleProgressIndicator.eInProcess_Mixing));
                    SetSampleProgressStatus(SampleProgressStatus.IsActive, SampleProgressStatus.IsRunning, SampleProgressStatus.IsInActive, SampleProgressStatus.IsInActive, runSampleStatus);
                    item.RunSampleProgress = RunSampleProgressIndicator.eInProcess_Mixing;
                    break;
					
                case SampleStatus.InProcessImageAcquisition:
                    IsLoadingIndicatorActive = true;
                    runSampleStatus = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(RunSampleProgressIndicator.eInProcess_ImageAcquisition));
                    SetSampleProgressStatus(SampleProgressStatus.IsActive, SampleProgressStatus.IsActive, SampleProgressStatus.IsRunning, SampleProgressStatus.IsInActive, runSampleStatus);
                    item.RunSampleProgress = RunSampleProgressIndicator.eInProcess_ImageAcquisition;
                    break;
					
                case SampleStatus.InProcessCleaning:
                    IsLoadingIndicatorActive = true;
                    runSampleStatus = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(RunSampleProgressIndicator.eInProcess_Cleaning));
                    SetSampleProgressStatus(SampleProgressStatus.IsActive, SampleProgressStatus.IsActive, SampleProgressStatus.IsActive, SampleProgressStatus.IsRunning, runSampleStatus);
                    item.RunSampleProgress = RunSampleProgressIndicator.eInProcess_Cleaning;
                    break;
					
                case SampleStatus.AcquisitionComplete:
                    IsLoadingIndicatorActive = true;
                    runSampleStatus = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(RunSampleProgressIndicator.eAcquisition_Complete));
                    SetSampleProgressStatus(SampleProgressStatus.IsActive, SampleProgressStatus.IsActive, SampleProgressStatus.IsActive, SampleProgressStatus.IsRunning, runSampleStatus);
                    item.RunSampleProgress = RunSampleProgressIndicator.eAcquisition_Complete;
                    break;
					
                case SampleStatus.Completed:
                    runSampleStatus = IsUsingCarousel
                        ? string.Format(LanguageResourceHelper.Get("LID_Status_Completed_POS"), item.SamplePosition.Column)
                        : string.Format(LanguageResourceHelper.Get("LID_Status_Completed_POS"), item.RowWisePosition);
                    SetSampleProgressStatus(SampleProgressStatus.IsActive, SampleProgressStatus.IsActive, SampleProgressStatus.IsActive, SampleProgressStatus.IsActive, runSampleStatus);
                    item.RunSampleProgress = RunSampleProgressIndicator.eCompleted;
                    break;
					
                case SampleStatus.SkipManual: break;
                case SampleStatus.SkipError: break;
                default:
                    SetSampleProgressStatus(SampleProgressStatus.IsInActive, SampleProgressStatus.IsInActive, SampleProgressStatus.IsInActive, SampleProgressStatus.IsInActive, runSampleStatus);
                    break;
            }
        }

        private void ClearSampleProgressStatus()
        {
            SetSampleProgressStatus(SampleProgressStatus.IsInActive, SampleProgressStatus.IsInActive, SampleProgressStatus.IsInActive, SampleProgressStatus.IsInActive, string.Empty);
            IsLoadingIndicatorActive = false;
        }

        private void SetSampleProgressStatus(
            SampleProgressStatus aspirationBrush, 
            SampleProgressStatus mixingDyeBrush,
            SampleProgressStatus imageAnalysisBrush, 
            SampleProgressStatus cleaningBrush, 
            string runSampleStatus)
        {
            AspirationBrush = aspirationBrush;
            MixingDyeBrush = mixingDyeBrush;
            ImageAnalysisBrush = imageAnalysisBrush;
            CleaningBrush = cleaningBrush;
            RunSampleStatus = runSampleStatus;
        }

        private void SetCarouselDataAfterCompleted(SampleEswDomain wqi)
        {
            SampleDomain runningSample = GetCurrentRunningSample(wqi);

            runningSample.Color = Brushes.Transparent;

            if (runningSample != null)
            {
                runningSample.Clear();
                runningSample.SampleStatusColor = SampleStatusColor.Completed;
                var completedSample =
                    SamplesToProcessList.FirstOrDefault(
                        sample => sample.SamplePosition.Column == wqi.SamplePosition.Column &&
                                  sample.SamplePosition.Row == wqi.SamplePosition.Row);
                if (completedSample != null)
                    SamplesToProcessList.Remove(completedSample);
            }
        }

        public SampleDomain GetCurrentRunningSample(SampleEswDomain wqi)
        {
            SampleDomain runningSample;
            if (IsUsingCarousel)
            {
                runningSample = CarouselSamples.FirstOrDefault(
                    sample => sample.SamplePosition.Column == wqi.SamplePosition.Column &&
                              sample.SamplePosition.Row == wqi.SamplePosition.Row);
            }
            else
            {
                runningSample = PlateSamples.FirstOrDefault(
                    sample => sample.SamplePosition.Column == wqi.SamplePosition.Column &&
                              sample.SamplePosition.Row == wqi.SamplePosition.Row);
            }

            return runningSample;
        }

        private void ClearAllOnceQueueFinishedOrStop()
        {
            if (IsUsingCarousel)
            {
                lock (CarouselSamples)
                {
                    foreach (var sample in CarouselSamples.Where(sample => sample.SampleStatusColor == SampleStatusColor.Selected && sample.SampleStatusColor != SampleStatusColor.Completed))
                    {
                        sample.Clear();
                    }
                }
            }
            else
            {
                foreach (var sample in PlateSamples.Where(sample => sample.SampleStatusColor == SampleStatusColor.Selected && sample.SampleStatusColor != SampleStatusColor.Completed))
                {
                    sample.Clear();
                }
            }

            SamplesToProcessList = new List<SampleDomain>();
        }

        public void SetPausingAbortingStatus(SystemStatus systemStatus)
        {
            PlayPauseStatusContent = string.Empty;
            AbortStatusContent = string.Empty;
            switch (systemStatus)
            {
                case SystemStatus.Idle:
                    onWQComplete();
                    ClearAllOnceQueueFinishedOrStop();
                    break;

                case SystemStatus.ProcessingSample:
                    IsEnableAdvanceOptionRunQueue = false;
                    IsQueueRunning = IsPauseActive = IsStopActive = true;
                    break;

                case SystemStatus.Pausing:
                    IsEnablePlayPause = IsStopActive = false;
                    PlayPauseStatusContent = LanguageResourceHelper.Get("LID_MSGBOX_PausingPleaseWait");
                    break;

                case SystemStatus.Paused:
                    IsQueueRunning = IsPauseActive = false;
                    IsEnablePlayPause = IsStopActive = true;
                    PlayPauseStatusContent = LanguageResourceHelper.Get("LID_Label_Resume");
                    if (SamplesToProcessList?.Count > 0)
                    {
                        var nextPauseSampleItem = SamplesToProcessList.FirstOrDefault();
                        if (nextPauseSampleItem != null)
                        {
                            nextPauseSampleItem.ExecutionStatus = ExecutionStatus.Pause;
                        }
                    }
                    ClearSampleProgressStatus();
                    break;

                case SystemStatus.Stopping:
                    IsAbortSampleActive = true;
                    IsEnablePlayPause = IsStopActive = false;
                    IsEnableAdvanceOptionRunQueue = true;
                    PlayPauseStatusContent = string.Empty;
                    AbortStatusContent = LanguageResourceHelper.Get("LID_Label_Aborting");
                    break;

                case SystemStatus.Stopped:
                case SystemStatus.Faulted:
                    AbortStatusContent = string.Empty;
                    onWQComplete();
                    ClearAllOnceQueueFinishedOrStop();
                    break;

                case SystemStatus.SearchingTube:
                    IsQueueRunning = IsPauseActive = IsStopActive = true;
                    IsLoadingIndicatorActive = true;
                    IsEnableAdvanceOptionRunQueue = false;
                    SetSampleProgressStatus(SampleProgressStatus.IsReady, SampleProgressStatus.IsReady, SampleProgressStatus.IsReady, SampleProgressStatus.IsReady, LanguageResourceHelper.Get("LID_ProgressIndication_FindingTubes"));
                    break;
            }
        }

        private void SetDefaultStateOfPlayPauseStop()
        {
            EnablePlayPauseBaseCarouselType();
            IsQueueRunning = IsStopActive = IsPauseActive = false;
        }


        private void EnablePlayPauseBaseCarouselType()
        {
            if (IsUsingCarousel)
                IsEnablePlayPause = true;
            else
                IsEnablePlayPause = PlateSamples.Count(sample => sample.SampleStatusColor != SampleStatusColor.Empty) > 0;
        }

        public List<SampleDomain> SamplesToProcessList { get; set; }
        public void GetSamplesToProcessList()
        {
            if (IsUsingCarousel) 
            {
                SamplesToProcessList = GetSamplesToProcessList(CarouselSamples);
            }
            else
            {
                SamplesToProcessList = GetSamplesToProcessList(IsPlateOrderedByRow
                    ? PlateSamplesByRow
                    : PlateSamplesByColumn);
            }
        }

        private List<SampleDomain> GetSamplesToProcessList(List<SampleDomain> sampleList)
        {
            var playList = sampleList?
                .Where(sample => sample.SampleStatusColor != SampleStatusColor.Empty && sample.SampleStatusColor != SampleStatusColor.Completed)
                .Select(sample =>
                {
                    sample.RunSampleProgress = RunSampleProgressIndicator.eNotProcessed;
                    sample.Comment = string.IsNullOrEmpty(sample.Comment) ? null : sample.Comment;
                    sample.BpQcName = string.IsNullOrEmpty(sample.BpQcName) ? null : sample.BpQcName;
                    return sample;
                }).ToList();

            return playList ?? new List<SampleDomain>();
        }

        public void SetShowParameterList(CellTypeQualityControlGroupDomain selectedCellTypeQualityControlGroup, WorkQueueItemDto wkDto)
        {
            var prevRec =
                WorkQueueResultRecord.SampleRecords.FirstOrDefault(x => x.SampleIdentifier.Equals(wkDto.Label));

            if (prevRec != null)
            {
                prevRec.SelectedResultSummary.UserId = LoggedInUser.CurrentUserId;
                prevRec.SelectedResultSummary.RetrieveDate = DateTime.Now;
                var showParameterList = new List<KeyValuePair<string, string>>();
                switch (selectedCellTypeQualityControlGroup.SelectedCtBpQcType)
                {
                    case CtBpQcType.CellType:
                        showParameterList =
                            RecordHelper.SetListParameter(prevRec, LoggedInUser.CurrentUserId);
                        break;
                    case CtBpQcType.QualityControl:
                        showParameterList = RecordHelper.SetListParameter(prevRec, LoggedInUser.CurrentUserId);
                        break;
                }

                prevRec.ShowParameterList = showParameterList;
            }
        }


        public void ClearWorkQueueRecordData()
        {
            WorkQueueResultRecord = new WorkQueueRecordDomain();
            SelectedSampleRecord = new SampleRecordDomain
            {
                ShowParameterList = new List<KeyValuePair<string, string>>(
                    ResultRecordHelper.GetShowParameterList(new GenericDataDomain(), LoggedInUser.CurrentUserId))
            };
        }

        public static SampleRecordDomain GetSampleRecordByUUID(uuidDLL uuid)
        {
            SampleRecordDomain sampleRecord;
            Log.Debug("RetrieveSampleRecord:: uiud: " + uuid);
            var hawkeyeError = HawkeyeCoreAPI.Sample.RetrieveSampleRecordAPI(uuid, out sampleRecord);
            Log.Debug("RetrieveSampleRecord() hawkeyeError:- " + hawkeyeError);
            LogSampleRecord(sampleRecord);
            return sampleRecord;
        }

        public static List<SampleRecordDomain> GetSampleRecordListByUuid(uuidDLL[] uuids)
        {
            var hawkeyeError = HawkeyeCoreAPI.Sample.RetrieveSampleRecordListAPI(uuids, out var sampleRecordList, out var retrieveSize);
            Log.Debug("RetrieveSampleRecordList:: hawkeyeError: " + hawkeyeError);
            return sampleRecordList;
        }

        private static void LogSampleRecord(SampleRecordDomain sampleRecord)
        {
            if (sampleRecord != null)
            {
                string str;
                if (sampleRecord.UUID.u != null)
                    str = sampleRecord.UUID.ToString();
                else
                    str = "invalid";

                Log.Debug($"SampleRecordDomain:: timeStamp: {sampleRecord.TimeStamp}, userId: {sampleRecord.UserId}, UUID: {str}");
            }
        }

        public static void LogWorkQueueItem(SampleEswDomain sample)
        {
            Log.Debug("WorkQueueItem::");
            Log.Debug("   celltypeIndex: " + sample.CellTypeIndex);
            Log.Debug("   Dilution: " + sample.Dilution);
            Log.Debug("   bp_qc_name: " + sample.CellTypeQcName);
            Log.Debug("   postWash: " + sample.WashType);
            Log.Debug("   status: " + sample.SampleStatus);
            Log.Debug("   SamplePositin Row: " + sample.SamplePosition.Row);
            Log.Debug("   SamplePosition Column: " + sample.SamplePosition.Column);
            Log.Debug("   Comment: " + sample.SampleTag);
            Log.Debug("   Label: " + sample.SampleName);
        }
    }

}