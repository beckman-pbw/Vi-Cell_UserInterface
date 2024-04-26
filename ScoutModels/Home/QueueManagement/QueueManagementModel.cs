using ApiProxies;
using ApiProxies.Commands.QueueManagement;
using ApiProxies.Misc;
using JetBrains.Annotations;
using ScoutDomains;
using ScoutDomains.Common;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutModels.Common;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Interfaces;
using ScoutUtilities.Reactive;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace ScoutModels.Home.QueueManagement
{
    public class QueueManagementModel : BaseDisposableNotifyPropertyChanged
    {
        #region Public Property

        public event EventHandler<ApiEventArgs<WorkQueueItemDto, uuidDLL>> WorkQueueItemStatusUpdated;

        public event EventHandler<ApiEventArgs<WorkQueueItemDto, uuidDLL>> WorkQueueItemCompleted;

        public event EventHandler<ApiEventArgs<uuidDLL>> WorkQueueCompleted;

        public event EventHandler<ApiEventArgs<WorkQueueItemDto, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers>> WorkQueueImageResultOccurred;

        #endregion

        #region Constructor

        public QueueManagementModel()
        {
            Initialize();
        }

        private AsyncMessagePump<ApiEventArgs> _eventPump;
        private void Initialize()
        {
            _eventPump = new AsyncMessagePump<ApiEventArgs>(
                NewThreadSchedulerAccessor.GetNormalPriorityScheduler(typeof(QueueManagementModel).Name),
                RouteNextApiEvent);
            SubscribeToWorkQueue(true);
        }

        protected override void DisposeManaged()
        {
            _eventPump?.Dispose();
            base.DisposeManaged();
        }

        protected override void DisposeUnmanaged()
        {
            SubscribeToWorkQueue(false);
            base.DisposeUnmanaged();
        }   

        #endregion

        #region

        private bool _isPlatePositionClear;
        public bool IsPlatePositionClear
        {
            get { return _isPlatePositionClear; }
            set
            {
                _isPlatePositionClear = value;
                NotifyPropertyChanged(nameof(IsPlatePositionClear));
            }
        }

        private List<SampleDomain> _plateQueueRowDesign;
        public List<SampleDomain> PlateQueueRowDesign
        {
            get { return _plateQueueRowDesign; }
            set
            {
                _plateQueueRowDesign = value;
                NotifyPropertyChanged(nameof(PlateQueueRowDesign));
            }
        }

        private List<SampleDomain> _plateQueueColumnDesign;
        public List<SampleDomain> PlateQueueColumnDesign
        {
            get { return _plateQueueColumnDesign; }
            set
            {
                _plateQueueColumnDesign = value;
                NotifyPropertyChanged(nameof(PlateQueueColumnDesign));
            }
        }

        private ObservableCollection<SampleDomain> _plateQueueDesignCollection;
        public ObservableCollection<SampleDomain> PlateQueueDesignCollection
        {
            get
            {
                if (_plateQueueDesignCollection == null)
                {
                    _plateQueueDesignCollection = new ObservableCollection<SampleDomain>();
                    foreach (var item in QueueManagementModel.CreateEmptyPlate())
                    {
                        item.SampleStatusColor = SampleStatusColor.Defined;
                        _plateQueueDesignCollection.Add(item);
                    }
                }
                return _plateQueueDesignCollection;
            }
        }


        private ICollectionView _plateQueueDesignCollectionView;
        public ICollectionView PlateQueueDesignCollectionView
        {
            get
            {
                if (_plateQueueDesignCollectionView == null)
                {
                    _plateQueueDesignCollectionView = new ListCollectionView(PlateQueueDesignCollection);
                    SortPlateQueueDesignCollectionView();
                }
                return _plateQueueDesignCollectionView;
            }
        }

        private void SortPlateQueueDesignCollectionView()
        {
            if (PlateQueueDesignCollectionView != null)
            {
                using (PlateQueueDesignCollectionView.DeferRefresh())
                {
                    PlateQueueDesignCollectionView.SortDescriptions.Clear();
                    if (IsRowWiseAddPosition)
                    {
                        ((ListCollectionView)PlateQueueDesignCollectionView).CustomSort = new SampleDomainRowWiseSorter();
                    }
                    else
                    {
                        ((ListCollectionView)PlateQueueDesignCollectionView).CustomSort = new SampleDomainColumnWiseSorter();
                    }
                }
            }
        }

        private bool _isRowWiseAddPosition = true;
        public bool IsRowWiseAddPosition
        {
            get { return _isRowWiseAddPosition; }
            set
            {
                _isRowWiseAddPosition = value;
                Misc.IsRowWiseAddPosition = value;
                NotifyPropertyChanged(nameof(IsRowWiseAddPosition));
                SortPlateQueueDesignCollectionView();
            }
        }

        #endregion

        #region Carousel Plate Fields and Properties

        private CellTypeQualityControlGroupDomain _selectedCellTypeQualityControlGroup;
        public CellTypeQualityControlGroupDomain SelectedCellTypeQualityControlGroup
        {
            get { return _selectedCellTypeQualityControlGroup ?? (_selectedCellTypeQualityControlGroup = new CellTypeQualityControlGroupDomain()); }
            set
            {
                _selectedCellTypeQualityControlGroup = value;
                NotifyPropertyChanged(nameof(SelectedCellTypeQualityControlGroup));
            }
        }
       
        private string _selectedSampleId;
        public string SelectedSampleId
        {
            get
            {
                if (string.IsNullOrEmpty(_selectedSampleId))
                {
                    _selectedSampleId = "";
                }

                return _selectedSampleId;
            }
            set
            {
                _selectedSampleId = value;
                NotifyPropertyChanged(nameof(SelectedSampleId));
            }
        }
     
        private string _selectedComment;
        public string SelectedComment
        {
            get { return _selectedComment; }
            set
            {
                _selectedComment = value;
                NotifyPropertyChanged(nameof(SelectedComment));
            }
        }

        private SamplePostWash _selectedWashDomain;
        public SamplePostWash SelectedWashDomain
        {
            get { return _selectedWashDomain; }
            set
            {
                _selectedWashDomain = value;
                NotifyPropertyChanged(nameof(SelectedWashDomain));
            }
        }
      
        private string _selectedDilutionDomain;
        public string SelectedDilutionDomain
        {
            get { return _selectedDilutionDomain; }
            set
            {
                _selectedDilutionDomain = value;
                NotifyPropertyChanged(nameof(SelectedDilutionDomain));
            }
        }

        private List<SamplePostWash> _washDomainList;      
        public List<SamplePostWash> WashDomainList
        {
            get { return _washDomainList ?? (_washDomainList = new List<SamplePostWash>(GetWashDomainList())); }
            set
            {
                _washDomainList = value;
                NotifyPropertyChanged("WashDomainList");
            }
        }

        private ObservableCollection<CellTypeQualityControlGroupDomain> _bpQcGroupList;
        public ObservableCollection<CellTypeQualityControlGroupDomain> BpQcGroupList
        {
            get { return _bpQcGroupList; }
            set
            {
                _bpQcGroupList = value;
                NotifyPropertyChanged("BpQcGroupList");
            }
        }

        private List<SampleDomain> _carouselQueueDesign;
        public List<SampleDomain> CarouselQueueDesign
        {
            get { return _carouselQueueDesign ?? (_carouselQueueDesign = CreateEmptyCarousel()); }
            set
            {
                _carouselQueueDesign = value;
                NotifyPropertyChanged(nameof(CarouselQueueDesign));
            }
        }

        private ObservableCollection<SampleDomain> _carouselQueueDesignCollection;
        public ObservableCollection<SampleDomain> CarouselQueueDesignCollection
        {
            get
            {
                if (_carouselQueueDesignCollection == null)
                {
                    _carouselQueueDesignCollection = new ObservableCollection<SampleDomain>();
                    foreach (var item in CarouselQueueDesign)
                    {
                        _carouselQueueDesignCollection.Add(item);
                    }
                }
                return _carouselQueueDesignCollection;
            }
        }

        private CollectionView _carouselQueueDesignCollectionView;
        public CollectionView CarouselQueueDesignCollectionView => _carouselQueueDesignCollectionView ?? (_carouselQueueDesignCollectionView = new ListCollectionView(CarouselQueueDesignCollection));

        private int _currentWorkQueueItemIndex;
        public int CurrentWorkQueueItemIndex
        {
            get { return _currentWorkQueueItemIndex; }
            set { _currentWorkQueueItemIndex = value; }
        }
      
        private uint _queue_length;
        public uint queue_length
        {
            get { return _queue_length; }
            set { _queue_length = value; }
        }

        #endregion

        #region Carousel PLate API Calls

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError SetWorkQueue(
            List<SampleDomain> samplesToProcessList,
            SampleDomain sampleDomain, 
            SubstrateType substrateType,
            Precession precession)
        {
            var wq = new WorkQueue();
            wq = SetModifyWorkQueue(samplesToProcessList, substrateType, precession);
            wq.additionalWorkSettings = SetModifySampleDomain(sampleDomain);
            LogWorkQueue(wq, ConvertObservableCollectionToArray(samplesToProcessList));
            var hawkeyeError = HawkeyeCoreAPI.WorkQueue.SetWorkQueueAPI(wq, ConvertObservableCollectionToArray(samplesToProcessList));
            Log.Debug("SetWorkQueue Status:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError PauseQueueExecution()
        {
            return new PauseQueue().Invoke();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ResumeQueueExecution()
        {
            return new ResumeQueue().Invoke();
        }

      
        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError StopQueueExecution()
        {
            return new StopQueue().Invoke();
        }
        
        #endregion

        public void SubscribeToWorkQueue(bool subscribe)
        {
            if (subscribe)
            {
                ApiEventBroker.Instance.Subscribe<WorkQueueItemDto, uuidDLL>(ApiEventType.WorkQueue_Item_Status, HandleWorkQueueItemStatusUpdate);
                ApiEventBroker.Instance.Subscribe<WorkQueueItemDto, uuidDLL>(ApiEventType.WorkQueue_Item_Completed, HandleWorkQueueItemCompleted);
                ApiEventBroker.Instance.Subscribe<uuidDLL>(ApiEventType.WorkQueue_Completed, HandleWorkQueueCompleted);
                ApiEventBroker.Instance.Subscribe<WorkQueueItemDto, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers>(
                        ApiEventType.WorkQueue_Image_Result, HandleWorkQueueImageResult);
            }
            else
            {
                ApiEventBroker.Instance.Unsubscribe<WorkQueueItemDto, uuidDLL>(ApiEventType.WorkQueue_Item_Status, HandleWorkQueueItemStatusUpdate);
                ApiEventBroker.Instance.Unsubscribe<WorkQueueItemDto, uuidDLL>(ApiEventType.WorkQueue_Item_Completed, HandleWorkQueueItemCompleted);
                ApiEventBroker.Instance.Unsubscribe<uuidDLL>(ApiEventType.WorkQueue_Completed, HandleWorkQueueCompleted);
                ApiEventBroker.Instance.Unsubscribe<WorkQueueItemDto, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers>(
                        ApiEventType.WorkQueue_Image_Result, HandleWorkQueueImageResult);
            }
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RotateCarousel(ref SamplePosition samplePos)
        {
            var hawkeyeError = HawkeyeCoreAPI.Service.RotateCarouselAPI(out samplePos);
            Log.Debug("RotateCarousel:: hawkeyeError: " + hawkeyeError + " row:" + samplePos.Row + " col:" + samplePos.Column);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError EjectSampleStage(string username, string password)
        {
            var hawkeyeError = HawkeyeCoreAPI.Service.EjectSampleStageAPI(username, password);
            Log.Debug("EjectSampleStage:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }

        public static void GetSystemStatus(out SystemStatus wqStatus)
        {
            wqStatus = SystemStatus.Idle;
            HawkeyeCoreAPI.SystemStatus.GetSystemStatusAPI(ref wqStatus);
        }

        public static SamplePosition GetSampleWellPosition()
        {
            SamplePosition samplePos;
            var hawkeyeError = HawkeyeCoreAPI.Service.GetSampleWellPositionAPI(out samplePos);
            Log.Debug("GetSampleWellPosition:: hawkeyeError: " + hawkeyeError + ", row:" + samplePos.Row + ", col:" + samplePos.Column);
            return samplePos;
        }

        private static WorkQueue SetModifyWorkQueue(List<SampleDomain> samplesToProcessList, SubstrateType substrateType, Precession precession)
        {
            var workQueue = new WorkQueue();
            workQueue.numWQI = (ushort)samplesToProcessList.Count;
            workQueue.curWQIIndex = 0;
            workQueue.substrate = substrateType;
            workQueue.precession = precession;
            workQueue.additionalWorkSettings = new WorkQueueItem();
            workQueue.workQueueItems = new IntPtr();
            return workQueue;
        }

        public WorkQueueItem SetModifySampleDomain(SampleDomain sample)
        {
            var workQueueItem = new WorkQueueItem
            {
                label = sample.SampleID.ToIntPtr(),
                comment = sample.Comment.ToIntPtr()
            };

            if (sample.SelectedCellTypeQualityControlGroup != null && sample.SelectedCellTypeQualityControlGroup.SelectedCtBpQcType != CtBpQcType.CellType)
            {
                var bpQc = sample.SelectedCellTypeQualityControlGroup.Name.Split('(');
                if (bpQc.Length > 0)
                    workQueueItem.bp_qc_name = bpQc[0].Trim().ToIntPtr();
            }
            else
            {
                workQueueItem.bp_qc_name = IntPtr.Zero;
            }

            workQueueItem.location = sample.SamplePosition;
            if (sample.SelectedCellTypeQualityControlGroup != null)
                workQueueItem.celltypeIndex = sample.SelectedCellTypeQualityControlGroup.CellTypeIndex;

            workQueueItem.saveEveryNthImage = Convert.ToUInt32(sample.NthImage);

            if (!string.IsNullOrEmpty(sample.SelectedDilution))
            {
                workQueueItem.dilutionFactor = Convert.ToUInt32(sample.SelectedDilution);
            }
            else
            {
                workQueueItem.dilutionFactor = 0;
            }

            workQueueItem.postWash = sample.SelectedWash;
            workQueueItem.status = SampleStatus.NotProcessed;
            workQueueItem.analysisIndices = 0;
            workQueueItem.numAnalyses = 1;

            return workQueueItem;
        }

        private void LogWorkQueue(WorkQueue wq, WorkQueueItem[] workqueueItemList)
        {
            Log.Debug("WorkQueue:: " +
                        "Current WorkQueue Index: " + wq.curWQIIndex +
                        ", Num Work Queue Item: " + wq.numWQI + 
                        ", Precession : " + wq.precession + 
                        ", Substrate : " + wq.substrate);
            foreach (var workqueueItem in workqueueItemList)
            {
                Log.Debug("WorkQueueItem:: analysisIndices: " + workqueueItem.analysisIndices + 
                             ", label: " + workqueueItem.label.ToSystemString() + 
                             ", bp_qc_name: " + workqueueItem.bp_qc_name.ToSystemString() + 
                             ", celltypeIndex: " + workqueueItem.celltypeIndex + 
                             ", comment: " + workqueueItem.comment.ToSystemString() + 
                             ", dilutionFactor: " + workqueueItem.dilutionFactor + 
                             ", SamplePosition column: " + workqueueItem.location.Column + 
                             ", SamplePosition row: " + workqueueItem.location.Row  + 
                             ", numAnalyses: " + workqueueItem.numAnalyses + 
                             ", postWash: " + workqueueItem.postWash  + 
                             ", sampleStatus: " + workqueueItem.status);
            }

            Log.Debug("AdditionalWorkSettings:: " + wq.additionalWorkSettings + 
                        ", analysisIndices : " + wq.additionalWorkSettings.analysisIndices +
                        ", label : " + wq.additionalWorkSettings.label.ToSystemString() +
                        ", bp_qc_name : " + wq.additionalWorkSettings.bp_qc_name.ToSystemString() +
                        ", celltypeIndex : " + wq.additionalWorkSettings.celltypeIndex +
                        ", comment : " + wq.additionalWorkSettings.comment.ToSystemString() +
                        ", dilutionFactor : " + wq.additionalWorkSettings.dilutionFactor +
                        ", SamplePosition column : " + wq.additionalWorkSettings.location.Column +
                        ", SamplePosition row : " + wq.additionalWorkSettings.location.Row +
                        ", numAnalyses : " + wq.additionalWorkSettings.numAnalyses +
                        ", postWash : " + wq.additionalWorkSettings.postWash +
                        ", SampleStatusColor : " + wq.additionalWorkSettings.status);
        }

        private WorkQueueItem[] ConvertObservableCollectionToArray(List<SampleDomain> queue)
        {
            WorkQueueItem[] workQueueItem = new WorkQueueItem[queue.Count];
            for (int i = 0; i < queue.Count; i++)
            {
                workQueueItem[i] = SetModifySampleDomain(queue[i]);
            }

            return workQueueItem;
        }

        public static List<SampleDomain> CreateEmptyCarousel()
        {
            var sampleList = new List<SampleDomain>();
            for (var i = 1; i <= ApplicationConstants.NumOfCarouselPositions; i++)
            {
                var sample = new SampleDomain
                {
                    SamplePosition = new SamplePosition('Z', i),
                    SampleID = string.Empty,
                    SelectedDilution = string.Empty,
                    SelectedWash = 0,
                    Comment = string.Empty,
                    SampleStatusColor = SampleStatusColor.Empty,
                    SampleRowPosition = i.ToString("00"),
                    Tag = i
                };

                sampleList.Add(sample);
            }

            return sampleList;
        }

        public static List<SampleDomain> CreateEmptyPlate()
        {
            var gridSamples = new List<SampleDomain>();
            int rowSize = 8;
            int columnSize = 12;
            int rowIndex = 0;
            for (int i = 1; i <= rowSize; i++)
            {
                for (int j = 1; j <= columnSize; j++)
                {
                    rowIndex++;
                    var objPlateGrid = new SampleDomain
                    {
                        SampleStatusColor = SampleStatusColor.Empty,
                        Id = i * 100 + j,
                        SamplePosition = new SamplePosition(NumberToAlpha(i), j),
                        Row = i,
                        Column = j,
                        RowIndex = rowIndex,
                        RowName = NumberToAlpha(i).ToString(),
                        ColumnName = j.ToString(),
                        RowWisePosition = NumberToAlpha(i) + j.ToString(),
                        SampleRowPosition = NumberToAlpha(i) + j.ToString("00"),
                        SampleID = string.Empty,
                        Type = 2
                    };
                    gridSamples.Add(objPlateGrid);
                }
            }

            int tag = 0;
            gridSamples.ForEach(s =>
            {
                tag++;
                s.Tag = tag;
            });

            return gridSamples;
        }

        public static void ClearSampleDomainList(List<SampleDomain> samples)
        {
            foreach (var sample in samples.Where(
                    x => x.SampleStatusColor == SampleStatusColor.Defined))
            {
                sample.Clear();
            }
        }

        public static CellTypeQualityControlGroupDomain GetCellTypeBpQC(List<CellTypeQualityControlGroupDomain> bpQcGroupList, 
            SampleEswDomain wqi)
        {
            return bpQcGroupList.SelectMany(x => x.CellTypeQualityControlChildItems)
                .FirstOrDefault(x => x.CellTypeIndex == wqi.CellTypeIndex);
        }

        public static char NumberToAlpha(int value)
        {
            char ReturnValue = 'A';
            if (value.Equals(0))
                return ReturnValue;
            return (char)((ReturnValue) + (value - 1));
        }

        #region Data Access
      
        private List<SamplePostWash> GetWashDomainList()
        {
            return Enum.GetValues(typeof(SamplePostWash)).Cast<SamplePostWash>().ToList();
        }


        private void HandleWorkQueueImageResult(object sender,
            ApiEventArgs<WorkQueueItemDto, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers> e)
        {
            _eventPump.Send(e);
        }

        private void HandleWorkQueueCompleted(object sender, ApiEventArgs<uuidDLL> e)
        {
            _eventPump.Send(e);
        }

        private void HandleWorkQueueItemCompleted(object sender, ApiEventArgs<WorkQueueItemDto, uuidDLL> e)
        {
            _eventPump.Send(e);
        }

        private void HandleWorkQueueItemStatusUpdate(object sender, ApiEventArgs<WorkQueueItemDto, uuidDLL> e)
        {
            _eventPump.Send(e);
        }

        private void RouteNextApiEvent(ApiEventArgs args)
        {
            switch (args.EventType)
            {
                case ApiEventType.WorkQueue_Completed:
                    WorkQueueCompleted?.Invoke(this, (ApiEventArgs<uuidDLL>) args);
                    break;

                case ApiEventType.WorkQueue_Item_Status:
                    WorkQueueItemStatusUpdated?.Invoke(this, (ApiEventArgs<WorkQueueItemDto, uuidDLL>) args);
                    break;

                case ApiEventType.WorkQueue_Item_Completed:
                    WorkQueueItemCompleted?.Invoke(this, (ApiEventArgs<WorkQueueItemDto, uuidDLL>) args);
                    break;

                case ApiEventType.WorkQueue_Image_Result:
                    WorkQueueImageResultOccurred?.Invoke(this,
                        (ApiEventArgs<WorkQueueItemDto, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers>)
                        args);
                    break;
            }
        }
        #endregion
    }

}
