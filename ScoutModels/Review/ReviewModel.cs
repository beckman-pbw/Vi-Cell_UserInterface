using ApiProxies;
using ApiProxies.Commands.QueueManagement;
using ApiProxies.Commands.Review;
using ApiProxies.Misc;
using JetBrains.Annotations;
using ScoutDomains;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.RunResult;
using ScoutModels.Common;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using ScoutDomains.Analysis;
using ScoutModels.Reports;
using ScoutUtilities.Interfaces;

namespace ScoutModels.Review
{
    public class ReviewModel : BaseDisposableNotifyPropertyChanged
    {
        #region Contructor

        public ReviewModel()
        {
        }

        protected override void DisposeUnmanaged()
        {
            ApiEventBroker.Instance.Unsubscribe<HawkeyeError, uuidDLL, ResultRecordDomain>(ApiEventType.Sample_Analysis, HandleSampleAnalysisOccurred);
            base.DisposeUnmanaged();
        }

        #endregion

        #region Properties

        public event EventHandler<ApiEventArgs<HawkeyeError, uuidDLL, ResultRecordDomain>> SampleAnalysisOccurred;

        private IList<SampleRecordDomain> _sampleRecordList;
        public IList<SampleRecordDomain> SampleRecordList
        {
            get { return _sampleRecordList ?? (_sampleRecordList = new List<SampleRecordDomain>()); }
            set { _sampleRecordList = value; }
        }

        #endregion

        #region Static Methods

        public static bool IsImageStatusDifferent(SampleRecordDomain sampleRecord, CellTypeDomain cellType)
        {
            var originalResultSummary = sampleRecord.ResultSummaryList.First();
            var sameDeclusterDegree = cellType.DeclusterDegree.Equals(originalResultSummary.CellTypeDomain.DeclusterDegree);
            return !sameDeclusterDegree;
        }

        #endregion

        #region Public Method

        [MustUseReturnValue("Use HawkeyeError")] 
        public HawkeyeError ReanalyzeSample(uuidDLL sample_id, uint celltype_index, uint analysis_index, bool fromImages)
        {
            try
            {
                ApiEventBroker.Instance.Subscribe<HawkeyeError, uuidDLL, ResultRecordDomain>(ApiEventType.Sample_Analysis, HandleSampleAnalysisOccurred);
                
                var apiCommand = new ReanalyzeSample(sample_id, celltype_index, analysis_index, fromImages);
                var result = apiCommand.Invoke();
                if (result != HawkeyeError.eSuccess)
                {
                    ApiEventBroker.Instance.Unsubscribe<HawkeyeError, uuidDLL, ResultRecordDomain>(ApiEventType.Sample_Analysis, HandleSampleAnalysisOccurred);
                }

                return result;
            }
            catch
            {
                ApiEventBroker.Instance.Unsubscribe<HawkeyeError, uuidDLL, ResultRecordDomain>(ApiEventType.Sample_Analysis, HandleSampleAnalysisOccurred);
                throw;
            }
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SignResultRecord(uuidDLL recordId, string signatureShortText, ushort shortTextLen)
        {
            try
            {
                var hawkeyeError = HawkeyeCoreAPI.Signature.SignResultRecordAPI(recordId, signatureShortText, shortTextLen);
                Log.Debug($"SignResultRecord({recordId}, {signatureShortText}, {shortTextLen}):: hawkeyeError: " + hawkeyeError);
                return hawkeyeError;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to SignResultRecord({recordId}, {signatureShortText}, {shortTextLen})", e);
                return HawkeyeError.eHardwareFault;
            }
            
        }

        public static List<ISignature> RetrieveSignatureDefinitions()
        {
            var signatureDomain = new List<SignatureDomain>();
            ushort numSignatures = 0;
            var hawkeyeError = HawkeyeCoreAPI.Signature.RetrieveSignatureDefinitionsAPI(ref signatureDomain, ref numSignatures);
            Log.Debug("RetrieveSignatureDefinitions:: hawkeyeError: " + hawkeyeError + ", num_signatures: " + numSignatures);
            return new List<ISignature>(signatureDomain);
        }

        public static List<SampleRecordDomain> GetFlattenedResultRecordList_wrappedInSampleRecords(UInt64 start, UInt64 end, string userId)
        {
            var sampleList = ResultModel.RetrieveSampleRecords(start, end, userId);

            var flatResultListBySample = new List<SampleRecordDomain>();
            if (sampleList == null)
                return flatResultListBySample;

            foreach (var sample in sampleList)
            {
                var flatSampleResults = sample.FlattenResultSummaries_wrappedInDuplicateSamples();
                SetSampleHierarchy(flatSampleResults);
                flatResultListBySample.AddRange(flatSampleResults);
            }

            return flatResultListBySample;
        }

        public static void SetSampleHierarchy(IList<SampleRecordDomain> flattenedSampleRecordList)
        {
            if (flattenedSampleRecordList == null || !flattenedSampleRecordList.Any())
                return;

            if (flattenedSampleRecordList.Count == 1)
            {
                flattenedSampleRecordList[0].SampleHierarchy = SampleHierarchyType.None;
				var cellType = flattenedSampleRecordList[0].ResultSummaryList[0].CellTypeDomain;
	            cellType.QCCellTypeForDisplay = cellType.CellTypeName;
				if (flattenedSampleRecordList[0].BpQcName != "")
                {
	                cellType.QCCellTypeForDisplay = Misc.GetParenthesisQualityControlName (flattenedSampleRecordList[0].BpQcName, cellType.CellTypeName);
                }
            }
            else
            {
                var oldestRecordUuid = flattenedSampleRecordList.OrderBy(s => s.SelectedResultSummary.RetrieveDate).FirstOrDefault()?.SelectedResultSummary.UUID;
                foreach (var sr in flattenedSampleRecordList)
                {
                    if (sr.SelectedResultSummary.UUID.Equals(oldestRecordUuid))
                    {
                        sr.SampleHierarchy = SampleHierarchyType.Parent;
                    }
                    else
                    {
                        sr.SampleHierarchy = SampleHierarchyType.Child;

                        // do not show the quality control name if this is a reanalyzed QC
                        if (!string.IsNullOrEmpty(sr.BpQcName) && sr.IsReanalyzedResult(sr.SelectedResultSummary))
                        {
                            sr.BpQcName = string.Empty;
                        }
					}

                    var cellType = sr.SelectedResultSummary.CellTypeDomain;
                    cellType.QCCellTypeForDisplay = cellType.CellTypeName;
                    if (sr.BpQcName != "")
                    {
	                    cellType.QCCellTypeForDisplay = Misc.GetParenthesisQualityControlName(sr.BpQcName, cellType.CellTypeName);
                    }
                }
            }
        }

        public static ResultSummaryDomain RetrieveResultSummary(uuidDLL id)
        {
            var hawkeyeError = HawkeyeCoreAPI.Result.RetrieveResultSummaryRecordAPI(id, out var resSummary);
#if DEBUG

            Log.Debug($"RetrieveResultSummary({id})::resSummary == null: '{resSummary == null}'::hawkeyeError: " + hawkeyeError);
#endif
            LogResultSummary(resSummary);
            return resSummary;
        }

        public ResultRecordDomain RetrieveResultRecord(uuidDLL id)
        {
            ResultRecordDomain resRecord;
            var hawkeyeError = HawkeyeCoreAPI.Result.RetrieveResultRecordAPI(id, out resRecord);
#if DEBUG
            LogResultRecord(resRecord);
#endif
			return resRecord;
        }
        
        public List<SampleImageRecordDomain> RetrieveSampleImageSetRecordsList(uuidDLL[] uuids)
        {
            if (uuids == null) return new List<SampleImageRecordDomain>();
            var hawkeyeError = HawkeyeCoreAPI.Sample.RetrieveSampleImageSetRecordListAPI(uuids, out var sampleImgRecordList);
#if DEBUG
			Log.Debug($"RetrieveSampleImageSetRecordsList: ({string.Join(", ", uuids)})");
#endif
	        Misc.LogOnHawkeyeError($"RetrieveSampleImageSetRecordsList", hawkeyeError);
            return sampleImgRecordList;
        }

        public ImageDto RetrieveImage(uuidDLL id, string filepath)
        {
            if (id.IsNullOrEmpty()) return new ImageDto();
            var apiProxy = new RetrieveImage(id, filepath);
            
            var result = apiProxy.Invoke();
            var msg = Logger.GetHawkeyeErrorMessage(result);
            if (!string.IsNullOrEmpty(msg)) Log.Warn(msg);

            if (apiProxy.Result.Equals(HawkeyeError.eSuccess))
                return apiProxy.Results.Item1;
            ApiHawkeyeMsgHelper.ErrorCommon(apiProxy.Result);
            return new ImageDto();
        }

        public ImageDto RetrieveAnnotatedImage(uuidDLL resultId, uuidDLL imageId, string filepath)
        {
            if (resultId.IsNullOrEmpty()) new ImageDto();
            if (imageId.IsNullOrEmpty()) new ImageDto();
            var apiProxy = new RetrieveAnnotatedImage(resultId, imageId, filepath);
            
            var result = apiProxy.Invoke();
            var msg = Logger.GetHawkeyeErrorMessage(result);
            if (!string.IsNullOrEmpty(msg)) Log.Warn(msg);
            
            if (apiProxy.Result.Equals(HawkeyeError.eSuccess))
                return apiProxy.Results.Item1;
            ApiHawkeyeMsgHelper.ErrorCommon(apiProxy.Result);
            return new ImageDto();
        }

        public ImageDto RetrieveBWImage(uuidDLL imageId, string filepath)
        {
            if (imageId.IsNullOrEmpty()) new ImageDto();
            var apiProxy = new RetrieveBlackwhiteImage(imageId, filepath);

            var result = apiProxy.Invoke();
            var msg = Logger.GetHawkeyeErrorMessage(result);
            if (!string.IsNullOrEmpty(msg)) Log.Warn(msg);
            
            if (apiProxy.Result.Equals(HawkeyeError.eSuccess))
                return apiProxy.Results.Item1;

            ApiHawkeyeMsgHelper.ErrorCommon(apiProxy.Result);

            return new ImageDto();
        }

        private static void LogResultSummary(ResultSummaryDomain resultSummary)
        {
            Log.Debug("LogResultSummary:: uuid: " + resultSummary.UUID + 
                         ", timeStamp: " + resultSummary.TimeStamp + ", userId: " + resultSummary.UserId);
        }

        private static void LogResultRecord(ResultRecordDomain resultRecord)
        {
			var resultSummary = resultRecord.ResultSummary;
            if (resultSummary != null)
            {
	            Log.Debug($"ResultRecord:: uuid: {resultRecord.ResultSummary.UUID}, timeStamp: {resultRecord.ResultSummary.TimeStamp}, userId: {resultRecord.ResultSummary.UserId}");
            }
            else
            {
                Log.Debug("   ResultSummary : Invalid or null result summary");
            }
        }

        private void HandleSampleAnalysisOccurred(object sender, ApiEventArgs<HawkeyeError, uuidDLL, ResultRecordDomain> e)
        {
            ApiEventBroker.Instance.Unsubscribe<HawkeyeError, uuidDLL, ResultRecordDomain>(ApiEventType.Sample_Analysis, HandleSampleAnalysisOccurred);
            SampleAnalysisOccurred?.Invoke(this, e);
        }

        #endregion
    }
}
