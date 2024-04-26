using ScoutDomains;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutDomains.RunResult;
using ScoutModels;
using ScoutModels.Home.QueueManagement;
using ScoutModels.Review;
using ScoutServices.Interfaces;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Delegate;
using ScoutUtilities.Helper;
using ScoutUtilities.Structs;
using ScoutViewModels.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace ScoutViewModels.Common
{
    public class RunSampleHelper : BaseViewModel, IRunSampleHelper
    {
        #region Constructor

        public RunSampleHelper(ICellTypeManager cellTypeManager)
        {
            _cellTypeManager = cellTypeManager;

			IsStopActive = default;
			IsPauseActive = default;
			SelectedSampleRecord = default;
			Message = default;
			Title = default;

			ImageId = 0;
			QueueManagementModel = new QueueManagementModel();
			QueueManagementModel.BpQcGroupList = LoggedInUser.GetCtQcs().ToObservableCollection();
			WorkQueueResultRecord = new WorkQueueRecordDomain();
        }

        protected override void DisposeUnmanaged()
        {
            QueueManagementModel.Dispose();
            base.DisposeUnmanaged();
        }

        #endregion

        #region Properties & Fields

        private readonly object _syncLockImageProcess = new object();

        private readonly ICellTypeManager _cellTypeManager;

        public bool IsStopActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public int ImageId
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public WorkQueueRecordDomain WorkQueueResultRecord
        {
            get { return GetProperty<WorkQueueRecordDomain>(); }
            set { SetProperty(value); }
        }

        public SampleRecordDomain SelectedSampleRecord
        {
            get { return GetProperty<SampleRecordDomain>(); }
            set { SetProperty(value); }
        }

        public bool IsPauseActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string Message
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string Title
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public QueueManagementModel QueueManagementModel
        {
            get { return GetProperty<QueueManagementModel>(); }
            set { SetProperty(value); }
        }

        public start_WorkQueue StartWorkQueue
        {
            get { return GetProperty<start_WorkQueue>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Public Methods

		public void AddResultSample(SampleEswDomain wqi, BasicResultAnswers cumulativeResults, 
            BasicResultAnswers imageResults, ImageSetDto image, int imageSequence)
        {
            lock (_syncLockImageProcess)
            {
                var resultCount = WorkQueueResultRecord.SampleRecords.Count(x =>
                    x.SampleIdentifier == wqi.SampleName);

                var resultSummary = new ResultSummaryDomain
                {
                    CellTypeDomain = _cellTypeManager.GetCellType(wqi.CellTypeIndex),
                    CumulativeResult = cumulativeResults.MarshalToBasicResultDomain()
				};

                if (resultCount == 0)
                {
                    ImageId++;
                    var sampleResult = new SampleRecordDomain
                    {
                        Tag = wqi.SampleTag,
                        BpQcName = wqi.CellTypeQcName,
                        SampleIdentifier = wqi.SampleName,
                        WashName = wqi.WashType,
                        DilutionName = Misc.ConvertToString(wqi.Dilution),
                        Position = wqi.SamplePosition,
                        SelectedResultSummary = resultSummary
                    };
                    DispatcherHelper.ApplicationExecute(() => { WorkQueueResultRecord.SampleRecords.Add(sampleResult); });
                    if (SelectedSampleRecord != null)
                    {
                        foreach (var item in SelectedSampleRecord.SampleImageList)
                            item.ImageSet.Dispose();
                    }
                    SelectedSampleRecord = sampleResult;
                }
                else
                {
                    var samples = WorkQueueResultRecord.SampleRecords.FirstOrDefault(x =>
                        x.SampleIdentifier == wqi.SampleName);
                    if (samples != null)
                    {
                        if (samples.ResultSummaryList.Count == 0)
                        {
                            samples.ResultSummaryList.Add(resultSummary);
                        }
                        
                        samples.SelectedResultSummary = resultSummary;

                        if (SelectedSampleRecord != null)
                        {
                            SelectedSampleRecord.NumImageSets = resultSummary.CumulativeResult.TotalCumulativeImage;
                        }
                    }
                }

                DispatcherHelper.ApplicationExecute(() =>
                {
                    UpdateSampleRecord(wqi, SelectedSampleRecord, imageResults, image, imageSequence);
                });
            }
        }

        public void UpdateSampleRecord(SampleEswDomain sample, SampleRecordDomain sampleRecord,
            BasicResultAnswers imageResults, ImageSetDto image, int imageSequence)
        {
            var sampleImageResult = new SampleImageRecordDomain
            {
	            ImageSet = image,
	            SequenceNumber = (uint) imageSequence,
                ImageID = sample.SamplePosition.Column,
                TotalCumulativeImage = (int)sampleRecord.NumImageSets,
                ResultPerImage = imageResults.MarshalToBasicResultDomain()
			};

            sampleRecord.SelectedSampleImageRecord?.ImageSet?.Dispose();
            sampleRecord.SelectedSampleImageRecord = sampleImageResult;

			// Commenting out this line fixes the memory leak (PC3549-5805).
			//sampleRecord.SampleImageList.Add(sampleRecord.SelectedSampleImageRecord);
			//sampleRecord.ImageIndexList.Add(new KeyValuePair<int, string>(imageSequence, Misc.ConvertToString(imageSequence)));

            foreach (var item in sampleRecord.SampleImageList)
            {
                item.TotalCumulativeImage = sampleRecord.SampleImageList.Count;
            }

            var imageSuccessCount = sampleRecord.SampleImageList.Count(x =>
                x.ResultPerImage.ProcessedStatus.Equals(E_ERRORCODE.eSuccess));
            sampleRecord.NumImageSets = (uint) imageSuccessCount;
        }

        #endregion
    }
}
